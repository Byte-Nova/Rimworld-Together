using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;

namespace GameServer.Managers.External
{
    public static class DiscordManager
    {
        private static DiscordSocketClient _client = null!;
        private static bool _running = false;

        private static IMessageChannel? chatChannel;
        private static IMessageChannel? consoleChannel;

        private const int presenceDelayMs = 60000;
        private static readonly ConcurrentQueue<string> _consoleQueue = new ConcurrentQueue<string>();
        private static Timer? _consoleBufferTimer;
        private const int BufferIntervalMs = 200;
        private const int MaxMessageLength = 1900;

        public static async Task StartAsync()
        {
            if (Master.discordConfig == null || !Master.discordConfig.Enabled || string.IsNullOrWhiteSpace(Master.discordConfig.BotToken))
            {
                Printer.Warning("[Discord] Integration disabled or missing config.");
                return;
            }

            if (_running)
            {
                Printer.Warning("[Discord] Already running.");
                return;
            }
            _running = true;

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _client.Log += OnLogAsync;
            _client.MessageReceived += OnMessageReceivedAsync;
            _client.Ready += OnBotReadyAsync;

            try
            {
                await _client.LoginAsync(TokenType.Bot, Master.discordConfig.BotToken);
                await _client.StartAsync();
                Printer.Message("[Discord] Bot connected.");
            }
            catch (Exception ex)
            {
                Printer.Error($"[Discord] Failed to start: {ex}");
                _running = false;
            }
        }

        public static async Task StopAsync()
        {
            if (!_running) return;
            _running = false;
            try
            {
                _consoleBufferTimer?.Dispose();
                await _client.StopAsync();
                await _client.LogoutAsync();
                _client.Dispose();
                Printer.Message("[Discord] Bot disconnected.");
            }
            catch (Exception ex)
            {
                Printer.Error($"[Discord] Stop error: {ex}");
            }
        }

        private static Task OnLogAsync(LogMessage msg)
        {
            Printer.Message($"[Discord Integration] > {msg.Message}");
            return Task.CompletedTask;
        }

        private static async Task OnBotReadyAsync()
        {
            if (!_running) return;
            if (Master.discordConfig.ChatChannelId != 0)
                chatChannel = _client.GetChannel(Master.discordConfig.ChatChannelId) as IMessageChannel;
            if (Master.discordConfig.ConsoleChannelId != 0)
                consoleChannel = _client.GetChannel(Master.discordConfig.ConsoleChannelId) as IMessageChannel;
            StartConsoleBuffer();
            await AnnounceServerOnline();
            if (Master.discordConfig.UseOnlineCount)
                _ = Task.Run(UpdatePlayerCountLoop);
        }

        private static async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!_running) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;
            if (message.Author.IsBot) return;

            if (Master.discordConfig.ChatChannelId != 0 && message.Channel.Id == Master.discordConfig.ChatChannelId)
            {
                ChatManager.BroadcastDiscordMessage(message.Author.Username, message.Content);
            }
            else if (Master.discordConfig.ConsoleChannelId != 0 && message.Channel.Id == Master.discordConfig.ConsoleChannelId)
            {
                Printer.Outsider($"[Discord Console] {message.Content}");
                ConsoleManager.ParseServerCommands(message.Content);
            }
            await Task.CompletedTask;
        }

        public static async Task SendMessageToDiscordChat(string username, string text)
        {
            if (!_running || _client == null) return;
            if (Master.discordConfig.ChatChannelId == 0) return;
            if (chatChannel == null)
            {
                chatChannel = _client.GetChannel(Master.discordConfig.ChatChannelId) as IMessageChannel;
                if (chatChannel == null) return;
            }
            await chatChannel.SendMessageAsync($"**{username}**: {text}");
        }

        public static void EnqueueConsoleLine(string line)
        {
            if (!_running) return;
            if (Master.discordConfig.ConsoleChannelId == 0) return;
            _consoleQueue.Enqueue(line);
            _consoleBufferTimer?.Change(BufferIntervalMs, Timeout.Infinite);
            if (_consoleBufferTimer == null)
            {
                _consoleBufferTimer = new Timer(async state => await FlushConsoleBuffer(), null, BufferIntervalMs, Timeout.Infinite);
            }
        }

        private static async Task FlushConsoleBuffer()
        {
            if (!_running) return;
            if (consoleChannel == null)
            {
                if (Master.discordConfig.ConsoleChannelId != 0)
                    consoleChannel = _client.GetChannel(Master.discordConfig.ConsoleChannelId) as IMessageChannel;
                if (consoleChannel == null) return;
            }
            StringBuilder sb = new StringBuilder();
            while (_consoleQueue.TryDequeue(out string line))
            {
                if (sb.Length + line.Length > MaxMessageLength)
                {
                    await TrySendMessageAsync(sb.ToString());
                    sb.Clear();
                }
                sb.AppendLine(line);
            }
            if (sb.Length > 0)
            {
                await TrySendMessageAsync(sb.ToString());
            }
            _consoleBufferTimer?.Change(BufferIntervalMs, Timeout.Infinite);
        }

        private static async Task TrySendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            try
            {
                await consoleChannel.SendMessageAsync(message);
            }
            catch (Discord.Net.HttpException httpEx)
            {
                Printer.Warning($"[Discord Integration] Rate limit triggered: {httpEx.Message}");
                foreach (var line in message.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        _consoleQueue.Enqueue(line);
                }
                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                Printer.Error($"[Discord Integration] Failed to send message: {ex}");
            }
        }

        private static void StartConsoleBuffer()
        {
            if (_consoleBufferTimer == null)
            {
                _consoleBufferTimer = new Timer(async state => await FlushConsoleBuffer(), null, BufferIntervalMs, Timeout.Infinite);
            }
        }

        public static async Task BroadcastAll(string message)
        {
            ChatManager.BroadcastServerNotification(message);
            await SendMessageToDiscordChat("SERVER", message);
        }

        private static async Task AnnounceServerOnline()
        {
            if (!_running) return;
            if (Master.discordConfig.ChatChannelId == 0) return;
            if (_client.ConnectionState != ConnectionState.Connected) return;
            if (chatChannel == null) return;
            await chatChannel.SendMessageAsync(":white_check_mark: **Server is now online!**");
        }

        private static async Task UpdatePlayerCountLoop()
        {
            while (_running && Master.discordConfig != null && Master.discordConfig.UseOnlineCount)
            {
                int count = NetworkHelper.GetConnectedClientsSafe().Length;
                string text = (count == 1) ? "1 Player Online" : $"{count} Players Online";
                await _client.SetCustomStatusAsync(text);
                await Task.Delay(presenceDelayMs);
            }
        }
    }
}