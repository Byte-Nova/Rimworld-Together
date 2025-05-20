using GameServer.Commands;
using GameServer.Core;
using GameServer.Core.Configs;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    public static class ChatManager
    {
        private static readonly Semaphore LogSemaphore     = new Semaphore(1, 1);
        private static readonly Semaphore CommandSemaphore = new Semaphore(1, 1);

        private static readonly string SystemName       = "CONSOLE";
        private static readonly string NotificationName = "SERVER";

        // pull server / discord configs
        private static ChatConfigFile    ChatConfig    => Master.ChatConfig;
        private static DiscordConfigFile DiscordConfig => Master.DiscordConfig;

        public static readonly string[] DefaultJoinMessages =
        {
            "Welcome to the global chat!",
            "Please be considerate with others and have fun!",
            "Use '/help' to check all the available commands."
        };

        public static readonly string[] defaultTextTools =
        {
            "List of available text tools:",
            "'b' inside brackets - Followed by the text you want to turn [b]bold",
            "'i' inside brackets - Followed by the text you want to turn [i]cursive",
            "HTML color inside brackets - Followed by the text you want to [ff0000]change color"
        };

        [HandlesPacket(PacketHeader.ChatManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            ChatData data = Serializer.ConvertBytesToObject<ChatData>(bytes);

            if (data._message.StartsWith("/"))
                ExecuteChatCommand(client, data._message.Split(' '));
            else
                BroadcastChatMessage(client, data._message);
        }

        // ───────────────────────────── commands ────────────────────────────
        private static void ExecuteChatCommand(ServerClient client, string[] cmd)
        {
            CommandSemaphore.WaitOne();

            CommandBase found = ChatManagerHelper.GetCommandFromName(cmd[0]);
            if (found == null)
                SendConsoleMessage(client, "Command was not found.");
            else
            {
                ChatCommandActions.TargetClient = client;
                ChatCommandActions.Command      = cmd;
                found.CommandAction.Invoke();
            }

            ChatManagerHelper.ShowChatInConsole(client.UserFile.Label, string.Join(" ", cmd));
            CommandSemaphore.Release();
        }

        // ───────────────────────────── broadcast ───────────────────────────
        private static void BroadcastChatMessage(ServerClient client, string msg)
        {
            if (Master.ServerConfig == null) return;

            ChatData data = new ChatData
            {
                _username      = client.UserFile.Label,
                _message       = msg,
                _usernameColor = client.UserFile.IsAdmin ? UserColor.Admin  : UserColor.Normal,
                _messageColor  = client.UserFile.IsAdmin ? MessageColor.Admin: MessageColor.Normal
            };

            NetworkHelper.SendPacketToAllClients(PacketHeader.ChatManager, data);
            WriteToLogs(client.UserFile.Label, msg);
            ChatManagerHelper.ShowChatInConsole(client.UserFile.Label, msg);

            // mirror to Discord
            if (DiscordConfig != null && DiscordConfig.Enabled)
                _ = DiscordManager.SendChatMessageAsync(client.UserFile.Label, msg);
        }

        public static void BroadcastDiscordMessage(string user, string msg)
        {
            ChatData data = new ChatData
            {
                _username      = user,
                _message       = msg,
                _usernameColor = UserColor.Discord,
                _messageColor  = MessageColor.Discord
            };

            NetworkHelper.SendPacketToAllClients(PacketHeader.ChatManager, data);
            WriteToLogs(user, msg);
            ChatManagerHelper.ShowChatInConsole(user, msg, true);
        }

        public static void BroadcastConsoleMessage(string msg)
        {
            ChatData data = new ChatData
            {
                _username      = SystemName,
                _message       = msg,
                _usernameColor = UserColor.Console,
                _messageColor  = MessageColor.Console
            };

            NetworkHelper.SendPacketToAllClients(PacketHeader.ChatManager, data);
            WriteToLogs(SystemName, msg);
            ChatManagerHelper.ShowChatInConsole(SystemName, msg);
        }

        public static void BroadcastServerNotification(string msg)
        {
            ChatData data = new ChatData
            {
                _username      = NotificationName,
                _message       = msg,
                _usernameColor = UserColor.Server,
                _messageColor  = MessageColor.Server
            };

            NetworkHelper.SendPacketToAllClients(PacketHeader.ChatManager, data);
            WriteToLogs(NotificationName, msg);
            ChatManagerHelper.ShowChatInConsole(NotificationName, msg);

            // also post to Discord
            if (DiscordConfig != null && DiscordConfig.Enabled)
                _ = DiscordManager.SendChatMessageAsync(NotificationName, msg);
        }

        // ───────────────────────────── helpers ────────────────────────────
        public static void SendConsoleMessage(ServerClient client, string msg)
        {
            ChatData data = new ChatData
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
            ChatData data = new ChatData
            {
                _username      = NotificationName,
                _message       = msg,
                _usernameColor = UserColor.Server,
                _messageColor  = MessageColor.Server
            };
            client.Listener.EnqueuePacket(PacketHeader.ChatManager, data);
        }

        private static void WriteToLogs(string user, string msg)
        {
            LogSemaphore.WaitOne();

            var sb = new StringBuilder();
            sb.Append($"[{DateTime.Now:HH:mm:ss}] | [{user}]: {msg}{Environment.NewLine}");
            string path = Path.Combine(Master.ChatLogsPath, $"{DateTime.Now:yyyy-MM-dd}.txt");
            File.AppendAllText(path, sb.ToString());

            LogSemaphore.Release();
        }
    }

    public static class ChatManagerHelper
    {
        public static ServerClient GetUserFromName(string name) =>
            NetworkHelper.GetConnectedClientFromUid(name);

        public static CommandBase GetCommandFromName(string cmd) =>
            ChatCommands.commands.FirstOrDefault(c => c.Prefix == cmd);

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