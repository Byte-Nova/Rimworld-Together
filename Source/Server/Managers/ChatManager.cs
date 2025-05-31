using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Commands;
using GameServer.Core;
using GameServer.Core.Configs;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    public static class ChatManager
    {
        private static readonly Semaphore _logLock     = new(1, 1);
        private static readonly Semaphore _cmdLock     = new(1, 1);
        private const string SystemName       = "CONSOLE";
        private const string NotificationName = "SERVER";
        private static ChatConfigFile?    ChatConfig    => Master.ChatConfig;
        private static DiscordConfigFile? DiscordConfig => Master.DiscordConfig;

        // Defaults messages
        public static readonly string[] DefaultJoinMessages =
        {
            "Welcome to the global chat!",
            "Please be considerate with others and have fun!",
            "Use '/help' to check all the available commands."
        };

        private static readonly string[] _defaultTextTools =
        {
            "List of available text tools:",
            "'b' inside brackets - Followed by the text you want to turn [b]bold",
            "'i' inside brackets - Followed by the text you want to turn [i]cursive",
            "HTML color inside brackets - Followed by the text you want to [ff0000]change color",
            "!'Name' - Ping a Discord user"
        };

        public static string[] DefaultTextTools => _defaultTextTools;

        public static string[] defaultTextTools => _defaultTextTools;

        // packets
        [HandlesPacket(PacketHeader.ChatManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            var data = Serializer.ConvertBytesToObject<ChatData>(bytes);

            if (data._message.StartsWith("/", StringComparison.Ordinal))
                ExecuteChatCommand(client, data._message.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            else
                BroadcastChatMessage(client, data._message);
        }

        private static void ExecuteChatCommand(ServerClient client, string[] cmd)
        {
            if (!_cmdLock.WaitOne(50))          // short wait avoids dead-lock
            {
                Printer.Warning("Chat command lock contention – dropped command.");
                return;
            }

            try
            {
                var found = ChatManagerHelper.GetCommandFromName(cmd[0]);
                if (found == null)
                {
                    SendConsoleMessage(client, "Command was not found.");
                }
                else
                {
                    ChatCommandActions.TargetClient = client;
                    ChatCommandActions.Command      = cmd;
                    found.CommandAction.Invoke();
                }

                ChatManagerHelper.ShowChatInConsole(client.UserFile.Label, string.Join(" ", cmd));
            }
            finally { _cmdLock.Release(); }
        }

        // broadcast helpers
        private static void BroadcastChatMessage(ServerClient client, string msg)
        {
            if (Master.ServerConfig == null) return;

            var data = new ChatData
            {
                _username      = client.UserFile.Label,
                _message       = msg,
                _usernameColor = client.UserFile.IsAdmin ? UserColor.Admin  : UserColor.Normal,
                _messageColor  = client.UserFile.IsAdmin ? MessageColor.Admin: MessageColor.Normal
            };

            NetworkHelper.SendPacketToAllClients(PacketHeader.ChatManager, data);
            _ = WriteToLogsAsync(client.UserFile.Label, msg);
            ChatManagerHelper.ShowChatInConsole(client.UserFile.Label, msg);

            // mirror to Discord chat channel (opt-in)
            if (DiscordConfig?.Enabled == true)
                _ = DiscordManager.SendChatMessageAsync(client.UserFile.Label, msg);
        }

        public static void BroadcastDiscordMessage(string user, string msg)
        {
            var data = new ChatData
            {
                _username      = user,
                _message       = msg,
                _usernameColor = UserColor.Discord,
                _messageColor  = MessageColor.Discord
            };

            NetworkHelper.SendPacketToAllClients(PacketHeader.ChatManager, data);
            _ = WriteToLogsAsync(user, msg);
            ChatManagerHelper.ShowChatInConsole(user, msg, true);
        }

        public static void BroadcastConsoleMessage(string msg)
            => BroadcastNamed(SystemName, UserColor.Console, MessageColor.Console, msg);
        public static void BroadcastServerNotification(string msg)
        {
            BroadcastNamed(NotificationName, UserColor.Server, MessageColor.Server, msg);

            if (DiscordConfig?.Enabled == true)
                _ = DiscordManager.SendChatMessageAsync(NotificationName, msg);
        }

        private static void BroadcastNamed(string who, UserColor uColor, MessageColor mColor, string msg)
        {
            var data = new ChatData
            {
                _username      = who,
                _message       = msg,
                _usernameColor = uColor,
                _messageColor  = mColor
            };

            NetworkHelper.SendPacketToAllClients(PacketHeader.ChatManager, data);
            _ = WriteToLogsAsync(who, msg);
            ChatManagerHelper.ShowChatInConsole(who, msg);
        }

        // console helpers
        public static void SendConsoleMessage(ServerClient client, string msg)
        {
            var data = new ChatData
            {
                _username      = SystemName,
                _message       = msg,
                _usernameColor = UserColor.Console,
                _messageColor  = MessageColor.Console
            };
            client.Listener.EnqueuePacket(PacketHeader.ChatManager, data);
        }

        public static void SendServerMessage(ServerClient client, string msg)
        {
            var data = new ChatData
            {
                _username      = NotificationName,
                _message       = msg,
                _usernameColor = UserColor.Server,
                _messageColor  = MessageColor.Server
            };
            client.Listener.EnqueuePacket(PacketHeader.ChatManager, data);
        }

        // async file logging
        private static Task WriteToLogsAsync(string user, string msg)
            => Task.Run(() =>
            {
                if (!_logLock.WaitOne(50)) return;     // skip if locked too long
                try
                {
                    string line  = $"[{DateTime.Now:HH:mm:ss}] | [{user}]: {msg}{Environment.NewLine}";
                    string path  = Path.Combine(Master.ChatLogsPath, $"{DateTime.Now:yyyy-MM-dd}.txt");
                    File.AppendAllText(path, line, Encoding.UTF8);
                }
                finally { _logLock.Release(); }
            });
    }

    public static class ChatManagerHelper
    {
        public static ServerClient? GetUserFromName(string name) =>
            NetworkHelper.GetConnectedClientFromUid(name);

        public static CommandBase? GetCommandFromName(string cmd) =>
            ChatCommands.commands.FirstOrDefault(c => c.Prefix.Equals(cmd, StringComparison.OrdinalIgnoreCase));

        public static string GetUsernameFromMention(string m) => m.Replace("@", "");

        public static void ShowChatInConsole(string user, string msg, bool fromDiscord = false)
        {
            if (!Master.ServerConfig.DisplayChatInConsole) return;

            if (fromDiscord)
                Printer.Message($"[Discord] > {user} > {msg}");
            else
                InformationDisplayer.DisplayChatMap(user, msg);
        }
    }
}