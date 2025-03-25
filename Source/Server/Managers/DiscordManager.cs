using System;
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
        // --- Rate limit merge functionality removed ---

        public static async Task StartAsync()
        {
            if (Master.discordConfig == null ||
                !Master.discordConfig.Enabled ||
                string.IsNullOrWhiteSpace(Master.discordConfig.BotToken))
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
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.MessageContent
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

            await AnnounceServerOnline();
            if (Master.discordConfig.UseOnlineCount)
                _ = Task.Run(UpdatePlayerCountLoop);
        }

        private static async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!_running) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;
            if (message.Author.IsBot) return;

            if (Master.discordConfig.ChatChannelId != 0 &&
                message.Channel.Id == Master.discordConfig.ChatChannelId)
            {
                ChatManager.BroadcastDiscordMessage(message.Author.Username, message.Content);
            }
            else if (Master.discordConfig.ConsoleChannelId != 0 &&
                     message.Channel.Id == Master.discordConfig.ConsoleChannelId)
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

        // --- Instead of queuing messages to avoid rate limits, send console lines immediately ---
        public static void EnqueueConsoleLine(string line)
        {
            if (!_running) return;
            if (Master.discordConfig.ConsoleChannelId == 0) return;
            Task.Run(async () =>
            {
                if (consoleChannel == null)
                {
                    consoleChannel = _client.GetChannel(Master.discordConfig.ConsoleChannelId) as IMessageChannel;
                    if (consoleChannel == null) return;
                }
                try
                {
                    await consoleChannel.SendMessageAsync(line);
                }
                catch (Exception ex)
                {
                    Printer.Error($"[Discord] Failed to send console line: {ex}");
                }
            });
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