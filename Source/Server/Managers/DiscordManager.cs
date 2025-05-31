using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GameServer.Core;
using GameServer.Core.Configs;
using GameServer.Managers;
using GameServer.TCP;
using Shared;

namespace GameServer.Misc
{
    // Handles all Discord I/O (console bridge + in-game chat mirror).
    public static class DiscordManager
    {
        private static DiscordSocketClient _client;
        private static DiscordConfigFile   _cfg;
        private static readonly SemaphoreSlim _presenceGate = new(1,1);

        // buffered “tap” → discord-console 
        private const int  TapBatchMax = 15;   // when >= 15 lines → flush immediately
        private const int  TapDelayMs  = 4_000;// or after 4 s since last line
        private static readonly List<string> _tapBuf  = new();
        private static readonly object       _tapLock = new();
        private static CancellationTokenSource? _tapCts;
        public static async Task InitializeAsync()
        {
            string cfgPath = Path.Combine(Master.ConfigsPath, "DiscordConfig.json");
            _cfg = Serializer.SerializeFromFile<DiscordConfigFile>(cfgPath);

            if (_cfg is null || !_cfg.Enabled)
            {
                Printer.Discord("[Discord] disabled in config.");
                return;
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel       = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.DirectMessages |
                                 GatewayIntents.MessageContent
            });

            _client.Log             += m => { Printer.Discord($"[Discord] {m}"); return Task.CompletedTask; };
            _client.Ready           += () => { Printer.Discord("[Discord] Ready"); return UpdatePresenceAsync(); };
            _client.MessageReceived += OnMessageReceivedAsync;

            // tap the three Printer streams (Title / Warning / Error)
            Printer.ConsoleTap += OnConsoleTap;

            try
            {
                await _client.LoginAsync(TokenType.Bot, _cfg.BotToken);
                await _client.StartAsync();
            }
            catch (Exception ex) { Printer.Error($"[Discord] start-up failed: {ex.Message}"); }
        }
        public static async Task UpdatePresenceAsync()
        {
            if (!_cfg.UseOnlineCount || _client?.LoginState != LoginState.LoggedIn) return;
            if (!await _presenceGate.WaitAsync(0)) return;

            try
            {
                var names = NetworkHelper.GetConnectedClientsSafe()
                                         .Select(c => c.UserFile.Label)
                                         .ToList();

                string status = names.Count switch
                {
                    0      => "No players online",
                    <= 5   => $"Players online: {names.Count}: {string.Join(", ", names)}",
                    _      => $"Players online: {names.Count}: {string.Join(", ", names.Take(5))} …"
                };

                await _client.SetGameAsync(status);
            }
            finally { _presenceGate.Release(); }
        }

        //──────────────────── helpers / regexes ────────────────────
        private static readonly Regex _guildEmoji  = new(@"<a?:([A-Za-z0-9_]+):\d+>", RegexOptions.Compiled);
        private static readonly Regex _bangMention = new(@"!\w+",                     RegexOptions.Compiled); //* !Name
        private static readonly Regex _userMention = new(@"<@!?(\d+)>",               RegexOptions.Compiled); //* <@ID>

        //──────────────────── game → Discord ────────────────────
        public static async Task SendChatMessageAsync(string username, string message)
        {
            if (!_cfg.Enabled) return;
            if (_client.GetChannel(_cfg.ChatChannelId) is not IMessageChannel ch) return;

            var allow = new AllowedMentions { AllowedTypes = AllowedMentionTypes.None };
            var guild = (ch as SocketGuildChannel)?.Guild;

            string final = _bangMention.Replace(message, m =>
            {
                string wanted = m.Value[1..];                         // strip '!'
                var user = guild?.Users.FirstOrDefault(u =>
                           string.Equals(u.DisplayName, wanted, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(u.Username,    wanted, StringComparison.OrdinalIgnoreCase));

                if (user is null) return m.Value;
                allow.UserIds.Add(user.Id);
                return user.Mention;
            });

            final = _guildEmoji.Replace(final, e => $":{e.Groups[1].Value}:");

            await ch.SendMessageAsync($"**{username}**: {final}", allowedMentions: allow);
            if (_cfg.UseOnlineCount) await UpdatePresenceAsync();
        }

        //──────────────────── console → Discord ────────────────────
        public static async Task SendConsoleMessageAsync(string payload)
        {
            if (!_cfg.Enabled) return;
            if (_client.GetChannel(_cfg.ConsoleChannelId) is not IMessageChannel ch) return;
            await ch.SendMessageAsync($"```{payload}```", allowedMentions: AllowedMentions.None);
        }

        //──────────────────── Discord → server ────────────────────
        private static Task OnMessageReceivedAsync(SocketMessage msg)
        {
            if (!_cfg.Enabled || msg.Author.IsBot) return Task.CompletedTask;

            string name = (msg.Author as SocketGuildUser)?.DisplayName ?? msg.Author.Username;

            // Console channel (requires @Bot prefix)
            if (msg.Channel.Id == _cfg.ConsoleChannelId)
            {
                string raw  = msg.Content.Trim();
                string tag1 = $"<@{_client.CurrentUser.Id}>",
                       tag2 = $"<@!{_client.CurrentUser.Id}>";

                if (raw.StartsWith(tag1) || raw.StartsWith(tag2))
                {
                    string cmd = raw.Replace(tag1, "").Replace(tag2, "").TrimStart();
                    if (!string.IsNullOrWhiteSpace(cmd))
                        ConsoleManager.ProcessDiscordCommand(cmd, name);
                }
                return Task.CompletedTask;
            }

            // Chat channel
            if (msg.Channel.Id == _cfg.ChatChannelId)
            {
                var guild = (msg.Channel as SocketGuildChannel)?.Guild;

                string text = msg.Content;
                text = _bangMention.Replace(text, m => $"@{m.Value[1..]}");      //* !Name → @Name
                text = _userMention.Replace(text, m =>
                {
                    if (!ulong.TryParse(m.Groups[1].Value, out ulong id)) return m.Value;
                    var u = guild?.GetUser(id);
                    return u is not null ? $"@{u.DisplayName ?? u.Username}" : m.Value;
                });
                text = _guildEmoji.Replace(text, e => $":{e.Groups[1].Value}:");

                ChatManager.BroadcastDiscordMessage(name, text);
            }

            return Task.CompletedTask;
        }

        /*──────── buffered tap logic ────────*/
        private static void OnConsoleTap(Printer.LogKind kind, string txt)
        {
            // ignore while a console command is actively capturing
            if (!string.IsNullOrEmpty(Printer.DiscordConsoleUser)) return;

            string icon = kind switch
            {
                Printer.LogKind.Error   => ":x:",
                Printer.LogKind.Warning => ":warning:",
                _                       => ":information_source:"
            };

            lock (_tapLock)
            {
                _tapBuf.Add($"{icon} {txt}");

                // flush immediately on hard limit
                if (_tapBuf.Count >= TapBatchMax)
                {
                    CancelDebounce();
                    _ = FlushTapAsync();
                    return;
                }

                // reset debounce timer
                CancelDebounce();
                _tapCts = new CancellationTokenSource();
                _ = DebounceFlushAsync(_tapCts.Token);
            }
        }

        private static async Task DebounceFlushAsync(CancellationToken tok)
        {
            try { await Task.Delay(TapDelayMs, tok); }
            catch (TaskCanceledException) { return; }
            await FlushTapAsync();
        }

        private static async Task FlushTapAsync()
        {
            string payload;
            lock (_tapLock)
            {
                if (_tapBuf.Count == 0) return;
                payload = string.Join('\n', _tapBuf);
                _tapBuf.Clear();
            }
            await SendConsoleMessageAsync(payload);
        }

        private static void CancelDebounce()
        {
            try { _tapCts?.Cancel(); } catch { /* ignore */ }
            _tapCts = null;
        }

        public static async Task ShutdownAsync()
        {
            if (_client == null) return;
            await _client.LogoutAsync();
            await _client.StopAsync();
            await _client.DisposeAsync();
            Printer.Discord("[Discord] Disconnected");
        }
    }
}