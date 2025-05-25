using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GameServer.Core;
using GameServer.Core.Configs;
using GameServer.Managers;
using GameServer.Misc;
using GameServer.TCP;
using Shared;

namespace GameServer.Misc
{
    ///   Handles all Discord I/O (console bridge + in-game chat mirror).
    ///   Public surface kept 100% identical for comptibility.
    public static class DiscordManager
    {
        private static DiscordSocketClient _Client;
        private static DiscordConfigFile   _Config;

        // simple rate-gate so presence updates aren’t spammed every packet
        private static readonly SemaphoreSlim _presenceGate = new(1, 1);

        // initialisation
        public static async Task InitializeAsync()
        {
            string cfgPath = Path.Combine(Master.ConfigsPath, "DiscordConfig.json");
            _Config = Serializer.SerializeFromFile<DiscordConfigFile>(cfgPath);

            if (_Config == null || !_Config.Enabled)
            {
                Printer.Outsider("[Discord] Integration disabled in config.");
                return;
            }

            var dcCfg = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents =
                      GatewayIntents.Guilds
                    | GatewayIntents.GuildMessages
                    | GatewayIntents.DirectMessages
                    | GatewayIntents.MessageContent
            };

            _Client = new DiscordSocketClient(dcCfg);
            _Client.Log             += OnLogAsync;
            _Client.Ready           += OnReadyAsync;
            _Client.MessageReceived += OnMessageReceivedAsync;

            try
            {
                await _Client.LoginAsync(TokenType.Bot, _Config.BotToken).ConfigureAwait(false);
                await _Client.StartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Printer.Error($"[Discord] Failed to start bot: {ex.Message}");
                // fail-open – the game server can still run without Discord
            }
        }

        // logging / ready
        private static Task OnLogAsync(LogMessage m)
        {
            Printer.Outsider($"[Discord] {m.Severity}: {m}");
            return Task.CompletedTask;
        }

        private static Task OnReadyAsync()
        {
            Printer.Outsider("[Discord] Info: Gateway: Ready");
            if (_Config.UseOnlineCount)
                _ = UpdatePresenceAsync();
            return Task.CompletedTask;
        }

        public static async Task UpdatePresenceAsync()
        {
            if (!_Config.UseOnlineCount || _Client?.LoginState != LoginState.LoggedIn) return;

            // gate: only one presence update at a time
            if (!await _presenceGate.WaitAsync(0)) return;

            try
            {
                int count = Network.ConnectedClients.Count;
                await _Client.SetGameAsync($"Players online: {count}").ConfigureAwait(false);
            }
            finally
            {
                _presenceGate.Release();
            }
        }

        // outgoing helpers
        public static async Task SendChatMessageAsync(string username, string message)
        {
            if (!_Config.Enabled) return;

            if (_Client.GetChannel(_Config.ChatChannelId) is not IMessageChannel ch)
            {
                Printer.Warning($"[Discord] ChatChannelId {_Config.ChatChannelId} invalid.");
                return;
            }

            await ch.SendMessageAsync($"**{username}**: {message}").ConfigureAwait(false);
            if (_Config.UseOnlineCount) await UpdatePresenceAsync().ConfigureAwait(false);
        }

        public static async Task SendConsoleMessageAsync(string payload)
        {
            if (!_Config.Enabled) return;

            if (_Client.GetChannel(_Config.ConsoleChannelId) is not IMessageChannel ch)
            {
                Printer.Warning($"[Discord] ConsoleChannelId {_Config.ConsoleChannelId} invalid.");
                return;
            }

            await ch.SendMessageAsync($"```{payload}```").ConfigureAwait(false);
            if (_Config.UseOnlineCount) await UpdatePresenceAsync().ConfigureAwait(false);
        }

        // incoming → game-side
        private static Task OnMessageReceivedAsync(SocketMessage msg)
        {
            if (!_Config.Enabled || msg.Author.IsBot) return Task.CompletedTask;

            if (msg.Channel.Id == _Config.ConsoleChannelId)
                ConsoleManager.ProcessDiscordCommand(msg.Content, msg.Author.Username);
            else if (msg.Channel.Id == _Config.ChatChannelId)
                ChatManager.BroadcastDiscordMessage(msg.Author.Username, msg.Content);

            return Task.CompletedTask;
        }

        // graceful shutdown helper (call from Main_ when server stops)
        public static async Task ShutdownAsync()
        {
            if (_Client == null) return;
            await _Client.LogoutAsync().ConfigureAwait(false);
            await _Client.StopAsync().ConfigureAwait(false);
            await _Client.DisposeAsync().ConfigureAwait(false);
            Printer.Outsider("[Discord] Gateway: Disconnected");
        }
    }
}