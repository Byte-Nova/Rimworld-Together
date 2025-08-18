using GameServer.Commands;
using GameServer.Core;
using GameServer.Misc;
using Shared;
using System.Text;
using static Shared.CommonEnumerators;
using TCPNetwork.Server;
using TCPNetwork.Packets;

namespace GameServer.Managers
{

    public static class ChatManager
    {
        private static readonly Semaphore logSemaphore = new Semaphore(1, 1);

        private static readonly Semaphore commandSemaphore = new Semaphore(1, 1);

        private static readonly string systemName = "CONSOLE";

        private static readonly string notificationName = "SERVER";

        public static readonly string[] defaultJoinMessages = new string[]
        {
            "Welcome to the global chat!",
            "Please be considerate with others and have fun!",
            "Use '/help' to check all the available commands."
        };

        public static readonly string[] defaultTextTools = new string[]
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

            Printer.Warning(data, LogImportanceMode.Extreme);

            if (data._message.StartsWith("/")) ExecuteChatCommand(client, data._message.Split(' '));
            else BroadcastChatMessage(client, data._message);
        }

        private static void ExecuteChatCommand(ServerClient client, string[] command)
        {
            commandSemaphore.WaitOne();

            CommandBase toFind = ChatManagerHelper.GetCommandFromName(command[0]);
            if (toFind == null) SendConsoleMessage(client, "Command was not found.");
            else
            {
                ChatCommandActions.TargetClient = client;
                ChatCommandActions.Command = command;
                toFind.CommandAction.Invoke();
            }

            string chatCommand = "";
            for (int i = 0; i < command.Length; i++) chatCommand += command[i] + "";

            ChatManagerHelper.ShowChatInConsole(client.UserFile.Label, chatCommand);

            commandSemaphore.Release();
        }

        private static void BroadcastChatMessage(ServerClient client, string message)
        {
            if (Master.ServerConfig == null) return;

            ChatData chatData = new ChatData();
            chatData._username = client.UserFile.Label;
            chatData._message = message;
            chatData._usernameColor = client.UserFile.IsAdmin ? UserColor.Admin : UserColor.Normal;
            chatData._messageColor = client.UserFile.IsAdmin ? MessageColor.Admin : MessageColor.Normal;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(client.UserFile.Label, message);
            ChatManagerHelper.ShowChatInConsole(client.UserFile.Label, message);
        }

        public static void BroadcastDiscordMessage(string client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = client;
            chatData._message = message;
            chatData._usernameColor = UserColor.Discord;
            chatData._messageColor = MessageColor.Discord;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(client, message);
            ChatManagerHelper.ShowChatInConsole(client, message, true);
        }

        public static void BroadcastConsoleMessage(string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = systemName;
            chatData._message = message;
            chatData._usernameColor = UserColor.Console;
            chatData._messageColor = MessageColor.Console;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(chatData._username, message);
            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
        }

        public static void BroadcastServerNotification(string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = notificationName;
            chatData._message = message;
            chatData._usernameColor = UserColor.Server;
            chatData._messageColor = MessageColor.Server;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(chatData._username, message);
            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
        }

        public static void SendConsoleMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = systemName;
            chatData._message = message;
            chatData._usernameColor = UserColor.Console;
            chatData._messageColor = MessageColor.Console;

            client.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);
        }

        public static void SendServerMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = notificationName;
            chatData._message = message;
            chatData._usernameColor = UserColor.Server;
            chatData._messageColor = MessageColor.Server;

            client.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);
        }

        private static void WriteToLogs(string username, string message)
        {
            logSemaphore.WaitOne();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{DateTime.Now:HH:mm:ss}] | [" + username + "]: " + message);
            stringBuilder.Append(Environment.NewLine);

            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = (dateTime.Year + "-" + dateTime.Month.ToString("D2") + "-" + dateTime.Day.ToString("D2")).ToString();
            string nowFullPath = Master.ChatLogsPath + Path.DirectorySeparatorChar + nowFileName + ".txt";

            File.AppendAllText(nowFullPath, stringBuilder.ToString());
            stringBuilder.Clear();

            logSemaphore.Release();
        }
    }

    public static class ChatManagerHelper
    {
        public static ServerClient GetUserFromName(string username)
        {
            return ServerNetwork.Instance.GetConnectedClientFromUid(username);
        }

        public static CommandBase GetCommandFromName(string commandName)
        {
            return ChatCommands.commands.ToArray().FirstOrDefault(x => x.Prefix == commandName);
        }

        public static string GetUsernameFromMention(string mention)
        {
            return mention.Replace("@", "");
        }

        public static void ShowChatInConsole(string username, string message, bool fromDiscord = false)
        {
            if (!Master.ServerConfig.DisplayChatInConsole) return;
            else
            {
                if (fromDiscord) Printer.Message($"[Discord] > {username} > {message}");
                else InformationDisplayer.DisplayChatMap(username, message);
            }
        }
    }
}

