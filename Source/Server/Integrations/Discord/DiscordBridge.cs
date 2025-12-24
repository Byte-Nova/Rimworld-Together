using Discord;
using Discord.WebSocket;
using GameServer.Managers;
using Shared.Misc;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static Shared.CommonEnumerators;

namespace GameServer.Integrations.Discord
{
    public static class DiscordBridge
    {
        private static readonly SemaphoreSlim StartSemaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim SendSemaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim MentionResolveSemaphore = new SemaphoreSlim(1, 1);

        private static DiscordSocketClient Client { get; set; }
        private static bool Started { get; set; }
        private static bool StopRequested { get; set; }

        private static ulong ChatChannelId { get; set; }
        private static ulong AdminChannelId { get; set; }
        private static string CommandPrefix { get; set; } = "!";

        private static HashSet<ulong> AdminRoleIds { get; set; } = new HashSet<ulong>();

        private static readonly ConcurrentQueue<OutboundMessage> Outbox = new ConcurrentQueue<OutboundMessage>();
        private static readonly SemaphoreSlim OutboxSignal = new SemaphoreSlim(0, int.MaxValue);
        private static Task OutboxWorkerTask { get; set; }

        private static readonly AllowedMentions NoMentions = AllowedMentions.None;

        private static readonly ConcurrentDictionary<string, MentionCacheEntry> MentionCache = new ConcurrentDictionary<string, MentionCacheEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex MentionTokenRegex = new Regex(@"(?<!\w)@(?:""([^""]{1,32})""|'([^']{1,32})'|([^\s@]{1,32}))", RegexOptions.Compiled);

        private static int BatchWindowMs { get; set; } = 250;
        private static int BurstCount { get; set; } = 4;
        private static int BurstWindowMs { get; set; } = 100;
        private static int MaxChunkLen { get; set; } = 1900;

        private static int MentionCacheMinutes { get; set; } = 30;
        private static int MaxMentionsPerMessage { get; set; } = 10;

        private static int ConsoleMirrorWindowMs { get; set; } = 2500;
        private static DateTime ConsoleMirrorUntilUtc { get; set; } = DateTime.MinValue;

        private static readonly object ConsoleRelayLock = new object();
        private static string LastConsoleRelayKey { get; set; } = string.Empty;
        private static DateTime LastConsoleRelayUtc { get; set; } = DateTime.MinValue;
        private static int LastConsoleRelayRepeats { get; set; } = 0;
        private static int ConsoleRepeatWindowMs { get; set; } = 1200;
        private static int ConsoleRepeatEvery { get; set; } = 5;

        public static void BeginConsoleMirrorWindow()
        {
            ConsoleMirrorUntilUtc = DateTime.UtcNow.AddMilliseconds(ConsoleMirrorWindowMs);
        }

        public static void TryRelayConsoleCommandToDiscord(string command)
        {
            if (!Started) return;
            if (Client == null) return;
            if (AdminChannelId == 0) return;
            if (string.IsNullOrWhiteSpace(command)) return;

            BeginConsoleMirrorWindow();
            Enqueue(AdminChannelId, $"`> {command.Trim()}`");
        }

        public static void TryRelayServerConsoleLine(string text, LogMode mode)
        {
            if (!Started) return;
            if (Client == null) return;
            if (AdminChannelId == 0) return;
            if (string.IsNullOrWhiteSpace(text)) return;

            bool isImportant = mode == LogMode.Warning || mode == LogMode.Error;

            if (!isImportant && DateTime.UtcNow > ConsoleMirrorUntilUtc) return;

            string cleaned = text.TrimEnd();

            bool shouldSend = true;
            lock (ConsoleRelayLock)
            {
                string key = ((int)mode).ToString() + "|" + cleaned;
                DateTime now = DateTime.UtcNow;

                if (key == LastConsoleRelayKey && (now - LastConsoleRelayUtc).TotalMilliseconds <= ConsoleRepeatWindowMs)
                {
                    LastConsoleRelayRepeats++;

                    if (ConsoleRepeatEvery > 1 && (LastConsoleRelayRepeats % ConsoleRepeatEvery) != 0)
                    {
                        shouldSend = false;
                    }
                    else
                    {
                        cleaned = $"{cleaned} (repeated {LastConsoleRelayRepeats}x)";
                    }

                    LastConsoleRelayUtc = now;
                }
                else
                {
                    LastConsoleRelayKey = key;
                    LastConsoleRelayRepeats = 0;
                    LastConsoleRelayUtc = now;
                }
            }

            if (!shouldSend) return;

            string prefix = "";
            if (mode == LogMode.Warning) prefix = "⚠️ ";
            else if (mode == LogMode.Error) prefix = "❌ ";
            else if (mode == LogMode.Title) prefix = "✅ ";

            Enqueue(AdminChannelId, $"[{DateTime.Now:HH:mm:ss}] | {prefix}{cleaned}");
        }

        public static void TryStart()
        {
            _ = Task.Run(StartAsync);
        }

        private static async Task StartAsync()
        {
            await StartSemaphore.WaitAsync();
            try
            {
                if (Started) return;
                if (GameServer.Core.Master.ServerConfig == null) return;

                var cfg = GameServer.Core.Master.ServerConfig;
                if (!cfg.EnableDiscordBridge) return;

                var token = (cfg.DiscordBotToken ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(token))
                {
                    Printer.Warning("[Discord] EnableDiscordBridge is true but DiscordBotToken is empty.");
                    return;
                }

                ChatChannelId = ParseUlong(cfg.DiscordChatChannelId);
                AdminChannelId = ParseUlong(cfg.DiscordAdminChannelId);
                CommandPrefix = string.IsNullOrWhiteSpace(cfg.DiscordCommandPrefix) ? "!" : cfg.DiscordCommandPrefix.Trim();
                AdminRoleIds = ParseRoleIds(cfg.DiscordAdminRoleIdsCsv);

                if (ChatChannelId == 0 && AdminChannelId == 0)
                {
                    Printer.Warning("[Discord] DiscordChatChannelId and DiscordAdminChannelId are both missing/invalid.");
                    return;
                }

                var socketCfg = new DiscordSocketConfig
                {
                    GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMessages |
                        GatewayIntents.MessageContent,
                    AlwaysDownloadUsers = false,
                    MessageCacheSize = 0
                };

                Client = new DiscordSocketClient(socketCfg);
                Client.MessageReceived += OnMessageReceivedAsync;

                await Client.LoginAsync(TokenType.Bot, token);
                await Client.StartAsync();

                Started = true;
                StopRequested = false;

                OutboxWorkerTask = Task.Run(OutboxWorkerAsync);
                DiscordPresence.TryStart(Client);

                Printer.Title("[Discord] Bridge started");
            }
            catch (Exception e)
            {
                Printer.Error($"[Discord] Bridge failed to start: {e}");
            }
            finally
            {
                StartSemaphore.Release();
            }
        }

        public static void TryStop()
        {
            _ = Task.Run(StopAsync);
        }

        private static async Task StopAsync()
        {
            try
            {
                if (!Started) return;

                StopRequested = true;
                OutboxSignal.Release();
                DiscordPresence.TryStop();

                if (OutboxWorkerTask != null)
                {
                    try { await OutboxWorkerTask; }
                    catch { }
                }

                if (Client != null)
                {
                    try { await Client.StopAsync(); } catch { }
                    try { await Client.LogoutAsync(); } catch { }
                }
            }
            catch { }
            finally
            {
                Started = false;
                Client = null;
            }
        }

        private static async Task OnMessageReceivedAsync(SocketMessage raw)
        {
            try
            {
                if (!Started) return;
                if (raw.Author?.IsBot == true) return;

                var content = raw.Content?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(content)) return;

                if (ChatChannelId != 0 && raw.Channel.Id == ChatChannelId)
                {
                    var name = GetBestName(raw);
                    var msg = SanitizeGameTextFromDiscord(raw, content);
                    if (msg.Length > 5000) msg = msg.Substring(0, 5000);

                    ChatManager.BroadcastDiscordMessage(name, msg);
                    return;
                }

                if (AdminChannelId != 0 && raw.Channel.Id == AdminChannelId)
                {
                    if (!content.StartsWith(CommandPrefix, StringComparison.Ordinal)) return;
                    if (!IsAuthorizedAdmin(raw)) { await SafeReply(raw.Channel, "❌ Not authorized."); return; }

                    var cmd = content.Substring(CommandPrefix.Length).Trim();
                    if (string.IsNullOrWhiteSpace(cmd)) return;

                    BeginConsoleMirrorWindow();

                    ConsoleManager.ParseServerCommands(cmd, true);

                    await SafeReply(raw.Channel, "✅ Executed.");
                }
            }
            catch (Exception e)
            {
                Printer.Error($"[Discord] MessageReceived error: {e}");
            }
        }

        public static void TryRelayGameChatToDiscord(string username, string message)
        {
            if (!Started) return;
            if (Client == null) return;
            if (ChatChannelId == 0) return;
            if (string.IsNullOrWhiteSpace(message)) return;

            _ = Task.Run(() => RelayGameChatToDiscordAsync(username, message));
        }

        private static async Task RelayGameChatToDiscordAsync(string username, string message)
        {
            try
            {
                if (!Started) return;
                if (Client == null) return;
                if (ChatChannelId == 0) return;

                var safeUser = Escape(username);
                var safeMsg = SanitizeDiscordText(message);

                ulong[] mentionUserIds = Array.Empty<ulong>();
                var guild = GetPrimaryGuild();
                if (guild != null)
                {
                    var mentionResult = await ApplyUserMentionsAsync(guild, safeMsg);
                    safeMsg = mentionResult.Text;
                    mentionUserIds = mentionResult.UserIds;
                }

                Enqueue(ChatChannelId, $"**{safeUser}:** {safeMsg}", mentionUserIds);
            }
            catch (Exception e)
            {
                Printer.Error($"[Discord] RelayGameChatToDiscord error: {e}");
            }
        }

        private static void Enqueue(ulong channelId, string text, IReadOnlyCollection<ulong> mentionUserIds = null)
        {
            if (!Started) return;
            if (Client == null) return;
            if (channelId == 0) return;
            if (string.IsNullOrWhiteSpace(text)) return;

            Outbox.Enqueue(new OutboundMessage(channelId, text, mentionUserIds));
            OutboxSignal.Release();
        }

        private static async Task OutboxWorkerAsync()
        {
            DateTime burstStart = DateTime.UtcNow;
            int sentInBurst = 0;

            while (!StopRequested)
            {
                try
                {
                    await OutboxSignal.WaitAsync();

                    if (StopRequested) break;
                    if (Client == null) continue;

                    if (!Outbox.TryDequeue(out var first))
                        continue;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(first.Text);

                    HashSet<ulong> mentionIds = null;
                    if (first.UserMentionIds != null && first.UserMentionIds.Length > 0)
                        mentionIds = new HashSet<ulong>(first.UserMentionIds);

                    DateTime batchUntil = DateTime.UtcNow.AddMilliseconds(BatchWindowMs);
                    while (DateTime.UtcNow < batchUntil && sb.Length < MaxChunkLen)
                    {
                        if (!Outbox.TryPeek(out var next)) break;
                        if (next.ChannelId != first.ChannelId) break;

                        if (!Outbox.TryDequeue(out next)) break;

                        string toAdd = "\n" + next.Text;
                        if (sb.Length + toAdd.Length > MaxChunkLen) break;

                        sb.Append(toAdd);

                        if (next.UserMentionIds != null && next.UserMentionIds.Length > 0)
                        {
                            mentionIds ??= new HashSet<ulong>();
                            foreach (var id in next.UserMentionIds) mentionIds.Add(id);
                        }
                    }

                    double elapsed = (DateTime.UtcNow - burstStart).TotalMilliseconds;
                    if (elapsed >= BurstWindowMs)
                    {
                        burstStart = DateTime.UtcNow;
                        sentInBurst = 0;
                    }
                    else if (sentInBurst >= BurstCount)
                    {
                        int wait = Math.Max(0, BurstWindowMs - (int)elapsed);
                        if (wait > 0) await Task.Delay(wait);
                        burstStart = DateTime.UtcNow;
                        sentInBurst = 0;
                    }

                    await SendToChannelAsync(first.ChannelId, sb.ToString(), mentionIds);
                    sentInBurst++;
                }
                catch (Exception e)
                {
                    Printer.Error($"[Discord] Outbox worker error: {e}");
                }
            }
        }

        private static async Task SendToChannelAsync(ulong channelId, string text, IReadOnlyCollection<ulong> mentionUserIds)
        {
            try
            {
                if (Client == null) return;

                var channel = Client.GetChannel(channelId) as IMessageChannel;
                if (channel == null) return;

                AllowedMentions mentions = NoMentions;
                if (mentionUserIds != null && mentionUserIds.Count > 0)
                {
                    mentions = new AllowedMentions
                    {
                        UserIds = new List<ulong>(mentionUserIds)
                    };
                }

                foreach (var chunk in Chunk(text, MaxChunkLen))
                {
                    await SendSemaphore.WaitAsync();
                    try { await channel.SendMessageAsync(chunk, allowedMentions: mentions); }
                    finally { SendSemaphore.Release(); }
                }
            }
            catch (Exception e)
            {
                Printer.Error($"[Discord] SendToChannel error: {e}");
            }
        }

        private static async Task SafeReply(ISocketMessageChannel channel, string text)
        {
            if (channel == null) return;

            foreach (var chunk in Chunk(text, MaxChunkLen))
            {
                await SendSemaphore.WaitAsync();
                try { await channel.SendMessageAsync(chunk, allowedMentions: NoMentions); }
                finally { SendSemaphore.Release(); }
            }
        }

        private static IEnumerable<string> Chunk(string text, int maxLen)
        {
            if (string.IsNullOrEmpty(text)) yield break;
            if (text.Length <= maxLen) { yield return text; yield break; }

            var sb = new StringBuilder(maxLen);
            foreach (var c in text)
            {
                if (sb.Length >= maxLen)
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
                sb.Append(c);
            }
            if (sb.Length > 0) yield return sb.ToString();
        }

        private static string GetBestName(SocketMessage msg)
        {
            if (msg.Author is SocketGuildUser gu)
            {
                if (!string.IsNullOrWhiteSpace(gu.Nickname)) return gu.Nickname;
                if (!string.IsNullOrWhiteSpace(gu.DisplayName)) return gu.DisplayName;

                string global = TryGetGlobalName(msg.Author);
                if (!string.IsNullOrWhiteSpace(global)) return global;

                return gu.Username;
            }

            string global2 = TryGetGlobalName(msg.Author);
            if (!string.IsNullOrWhiteSpace(global2)) return global2;

            return msg.Author?.Username ?? "Discord";
        }

        private static string TryGetGlobalName(IUser user)
        {
            try
            {
                if (user == null) return string.Empty;
                PropertyInfo p = user.GetType().GetProperty("GlobalName", BindingFlags.Public | BindingFlags.Instance);
                if (p == null) return string.Empty;

                object v = p.GetValue(user, null);
                return v as string ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsAuthorizedAdmin(SocketMessage msg)
        {
            if (AdminRoleIds.Count == 0) return true;
            if (msg.Author is not SocketGuildUser gu) return false;

            foreach (var r in gu.Roles)
                if (AdminRoleIds.Contains(r.Id))
                    return true;

            return false;
        }

        private static HashSet<ulong> ParseRoleIds(string csv)
        {
            HashSet<ulong> set = new HashSet<ulong>();
            if (string.IsNullOrWhiteSpace(csv)) return set;

            foreach (var part in csv.Split(','))
            {
                if (ulong.TryParse(part.Trim(), out var id))
                    set.Add(id);
            }
            return set;
        }

        private static ulong ParseUlong(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            return ulong.TryParse(input.Trim(), out var v) ? v : 0;
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`");
        }

        private static string SanitizeDiscordText(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            s = s.Replace("@everyone", "@\u200beveryone").Replace("@here", "@\u200bhere");
            s = s.Replace("<@", "<\u200b@").Replace("<#", "<\u200b#");
            return s;
        }

        private static string SanitizeGameTextFromDiscord(SocketMessage raw, string content)
        {
            try
            {
                var result = content;

                if (raw.MentionedUsers != null)
                {
                    foreach (var u in raw.MentionedUsers)
                    {
                        var n = u.Username ?? "user";
                        result = result.Replace($"<@{u.Id}>", "@" + n).Replace($"<@!{u.Id}>", "@" + n);
                    }
                }

                if (raw.MentionedRoles != null)
                {
                    foreach (var r in raw.MentionedRoles)
                    {
                        var n = r.Name ?? "role";
                        result = result.Replace($"<@&{r.Id}>", "@" + n);
                    }
                }

                return result;
            }
            catch
            {
                return content;
            }
        }

        private static SocketGuild GetPrimaryGuild()
        {
            if (Client == null) return null;

            if (ChatChannelId != 0 && Client.GetChannel(ChatChannelId) is SocketGuildChannel c1)
                return c1.Guild;

            if (AdminChannelId != 0 && Client.GetChannel(AdminChannelId) is SocketGuildChannel c2)
                return c2.Guild;

            return null;
        }

        private static async Task<MentionApplyResult> ApplyUserMentionsAsync(SocketGuild guild, string input)
        {
            if (guild == null) return new MentionApplyResult(input, Array.Empty<ulong>());
            if (string.IsNullOrEmpty(input)) return new MentionApplyResult(string.Empty, Array.Empty<ulong>());

            var matches = MentionTokenRegex.Matches(input);
            if (matches.Count == 0) return new MentionApplyResult(input, Array.Empty<ulong>());

            Dictionary<string, ulong> resolvedInMessage = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
            HashSet<ulong> allowedIds = new HashSet<ulong>();
            StringBuilder sb = new StringBuilder(input.Length);

            int last = 0;
            int mentionCount = 0;

            await MentionResolveSemaphore.WaitAsync();
            try
            {
                foreach (Match m in matches)
                {
                    if (!m.Success) continue;
                    if (m.Index < last) continue;

                    sb.Append(input, last, m.Index - last);

                    string rawName = m.Groups[1].Success ? m.Groups[1].Value
                                   : m.Groups[2].Success ? m.Groups[2].Value
                                   : m.Groups[3].Value;

                    string name = NormalizeMentionName(rawName);
                    if (string.IsNullOrWhiteSpace(name) || IsBlockedMentionName(name) || mentionCount >= MaxMentionsPerMessage)
                    {
                        sb.Append(m.Value);
                        last = m.Index + m.Length;
                        continue;
                    }

                    if (!resolvedInMessage.TryGetValue(name, out var id))
                    {
                        id = await ResolveUserIdAsync(guild, name);
                        resolvedInMessage[name] = id;
                    }

                    if (id != 0)
                    {
                        sb.Append("<@").Append(id).Append('>');
                        allowedIds.Add(id);
                        mentionCount++;
                    }
                    else
                    {
                        sb.Append(m.Value);
                    }

                    last = m.Index + m.Length;
                }
            }
            finally
            {
                MentionResolveSemaphore.Release();
            }

            if (last < input.Length)
                sb.Append(input, last, input.Length - last);

            return new MentionApplyResult(sb.ToString(), allowedIds.Count == 0 ? Array.Empty<ulong>() : allowedIds.ToArray());
        }

        private static bool IsBlockedMentionName(string name)
        {
            return name.Equals("everyone", StringComparison.OrdinalIgnoreCase)
                || name.Equals("here", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeMentionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            name = name.Trim();

            if (name.Length > 32) name = name.Substring(0, 32);

            if (name.Contains('_'))
                name = name.Replace('_', ' ');

            return name.Trim();
        }

        private static async Task<ulong> ResolveUserIdAsync(SocketGuild guild, string name)
        {
            if (guild == null) return 0;

            var now = DateTime.UtcNow;
            if (MentionCache.TryGetValue(name, out var cached) && cached.ExpiresUtc > now)
                return cached.UserId;

            ulong id = 0;

            id = TryMatchUserInCache(guild, name);
            if (id == 0 && name.Contains(' '))
                id = TryMatchUserInCache(guild, name.Replace(" ", string.Empty));

            if (id == 0)
            {
                try
                {
                    var users = await guild.SearchUsersAsync(name, 25);
                    if (users != null)
                    {
                        IGuildUser best = null;
                        foreach (var u in users)
                        {
                            if (u == null) continue;

                            if (IsUserMatch(u, name))
                            {
                                best = u;
                                break;
                            }

                            best ??= u;
                        }
                        id = best?.Id ?? 0;
                    }
                }
                catch { }
            }

            var exp = now.AddMinutes(MentionCacheMinutes);
            MentionCache[name] = new MentionCacheEntry(id, exp);
            return id;
        }

        private static ulong TryMatchUserInCache(SocketGuild guild, string name)
        {
            try
            {
                foreach (var u in guild.Users)
                {
                    if (u == null) continue;
                    if (IsUserMatch(u, name)) return u.Id;
                }
            }
            catch { }
            return 0;
        }

        private static bool IsUserMatch(IGuildUser u, string name)
        {
            if (u == null) return false;
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (u is SocketGuildUser su)
            {
                if (string.Equals(su.Username, name, StringComparison.OrdinalIgnoreCase)) return true;
                if (!string.IsNullOrWhiteSpace(su.Nickname) && string.Equals(su.Nickname, name, StringComparison.OrdinalIgnoreCase)) return true;
                if (!string.IsNullOrWhiteSpace(su.DisplayName) && string.Equals(su.DisplayName, name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            else
            {
                if (string.Equals(u.Username, name, StringComparison.OrdinalIgnoreCase)) return true;
                if (!string.IsNullOrWhiteSpace(u.Nickname) && string.Equals(u.Nickname, name, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private readonly struct OutboundMessage
        {
            public readonly ulong ChannelId;
            public readonly string Text;
            public readonly ulong[] UserMentionIds;

            public OutboundMessage(ulong channelId, string text, IReadOnlyCollection<ulong> mentionUserIds)
            {
                ChannelId = channelId;
                Text = text;

                if (mentionUserIds == null || mentionUserIds.Count == 0)
                {
                    UserMentionIds = null;
                }
                else
                {
                    UserMentionIds = new ulong[mentionUserIds.Count];
                    int i = 0;
                    foreach (var id in mentionUserIds) UserMentionIds[i++] = id;
                }
            }
        }

        private readonly struct MentionApplyResult
        {
            public readonly string Text;
            public readonly ulong[] UserIds;

            public MentionApplyResult(string text, ulong[] userIds)
            {
                Text = text;
                UserIds = userIds ?? Array.Empty<ulong>();
            }
        }

        private readonly struct MentionCacheEntry
        {
            public readonly ulong UserId;
            public readonly DateTime ExpiresUtc;

            public MentionCacheEntry(ulong userId, DateTime expiresUtc)
            {
                UserId = userId;
                ExpiresUtc = expiresUtc;
            }
        }
    }
}