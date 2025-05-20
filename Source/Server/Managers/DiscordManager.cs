using System;
using System.IO;
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
    public static class DiscordManager
    {
        private static DiscordSocketClient _Client;
        private static DiscordConfigFile   _Config;

        //                      initialisation
        public static async Task InitializeAsync()
        {
            string ConfigPath = Path.Combine(Master.ConfigsPath, "DiscordConfig.json");
            _Config = Serializer.SerializeFromFile<DiscordConfigFile>(ConfigPath);

            if (_Config == null || !_Config.Enabled) return;

            var DiscordConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents =
                      GatewayIntents.Guilds
                    | GatewayIntents.GuildMessages
                    | GatewayIntents.DirectMessages
                    | GatewayIntents.MessageContent
            };

            _Client = new DiscordSocketClient(DiscordConfig);
            _Client.Log             += OnLogAsync;
            _Client.Ready           += OnReadyAsync;
            _Client.MessageReceived += OnMessageReceivedAsync;

            await _Client.LoginAsync(TokenType.Bot, _Config.BotToken);
            await _Client.StartAsync();
        }

        //                      logging / ready
        private static Task OnLogAsync(LogMessage msg)
        {
            Printer.Outsider($"[Discord] {msg.Severity}: {msg}");
            return Task.CompletedTask;
        }

        private static Task OnReadyAsync()
        {
            Printer.Outsider("[Discord] Info: Gateway: Ready");
            if (_Config.UseOnlineCount) _ = UpdatePresenceAsync();
            return Task.CompletedTask;
        }

        public static async Task UpdatePresenceAsync()
        {
            if (!_Config.UseOnlineCount) return;
            await _Client.SetGameAsync($"Players online: {Network.ConnectedClients.Count}");
        }

        //                      outgoing
        public static async Task SendChatMessageAsync(string username, string message)
        {
            if (!_Config.Enabled) return;

            var ChannelRaw = _Client.GetChannel(_Config.ChatChannelId);
            if (ChannelRaw is not IMessageChannel Channel)
            {
                Printer.Warning($"[Discord] ChatChannelId {_Config.ChatChannelId} invalid.");
                return;
            }

            await Channel.SendMessageAsync($"**{username}**: {message}");
            if (_Config.UseOnlineCount) await UpdatePresenceAsync();
        }

        public static async Task SendConsoleMessageAsync(string payload)
        {
            if (!_Config.Enabled) return;

            var ChannelRaw = _Client.GetChannel(_Config.ConsoleChannelId);
            if (ChannelRaw is not IMessageChannel Channel)
            {
                Printer.Warning($"[Discord] ConsoleChannelId {_Config.ConsoleChannelId} invalid.");
                return;
            }

            await Channel.SendMessageAsync($"```{payload}```");
            if (_Config.UseOnlineCount) await UpdatePresenceAsync();
        }

        //                             incoming
        private static Task OnMessageReceivedAsync(SocketMessage msg)
        {
            if (!_Config.Enabled || msg.Author.IsBot) return Task.CompletedTask;

            if (msg.Channel.Id == _Config.ConsoleChannelId)
                ConsoleManager.ProcessDiscordCommand(msg.Content, msg.Author.Username);
            else if (msg.Channel.Id == _Config.ChatChannelId)
                ChatManager.BroadcastDiscordMessage(msg.Author.Username, msg.Content);

            return Task.CompletedTask;
        }
    }
}