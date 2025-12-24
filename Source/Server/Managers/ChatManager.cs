using GameServer.Commands;
using GameServer.Integrations.Discord;
using GameServer.Core;
using GameServer.Misc;
using Shared;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static Shared.CommonEnumerators;
using TCPNetwork.Packets;
using TCPNetwork.Files.Client;
using Shared.Misc;

namespace GameServer.Managers
{

    public static class ChatManager
    {
        private static Semaphore LogSemaphore = new Semaphore(1, 1);

        private static Semaphore CommandSemaphore { get; set; } = new Semaphore(1, 1);

        private static string SystemName { get; set; } = "CONSOLE";

        private static string NotificationName { get; set; } = "SERVER";

        public static string[] DefaultJoinMessages { get; set; } = new string[]
        {
            "Welcome to the global chat!",
            "Please be considerate with others and have fun!",
            "Use '/help' to check all the available commands."
        };

        public static string[] DefaultTextTools { get; set; } = new string[]
        {
            "List of available text tools:",
            "'b' inside brackets - Followed by the text you want to turn [b]bold",
            "'i' inside brackets - Followed by the text you want to turn [i]cursive",
            "HTML color inside brackets - Followed by the text you want to [ff0000]change color"
        };

        [HandlesPacket(PacketHeader.ChatManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            ChatData data = Serializer.ConvertBytesToObject<ChatData>(bytes);

            if (data._message.StartsWith("/")) ExecuteChatCommand(client, data._message.Split(' '));
            else BroadcastChatMessage(client, data._message);
        }

        private static void ExecuteChatCommand(ServerClient client, string[] command)
        {
            CommandSemaphore.WaitOne();

            CommandBase toFind = ChatManagerHelper.GetCommandFromName(command[0]);
            if (toFind == null) SendConsoleMessage(client, "Command was not found.");
            else
            {
                ChatCommandActions.TargetClient = client;
                ChatCommandActions.Command = command;
                toFind.CommandAction.Invoke();
            }

            // idiotic string was concatenated with no spaces (like bruh what, ocd hit hard on this one)
            string chatCommand = "";
            for (int i = 0; i < command.Length; i++) chatCommand += command[i] + " ";
            chatCommand = chatCommand.TrimEnd();

            ChatManagerHelper.ShowChatInConsole(client.UserFile.Username, chatCommand);

            CommandSemaphore.Release();
        }

        private static void BroadcastChatMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = client.UserFile.Username;
            chatData._message = message;
            chatData._usernameColor = client.UserFile.IsAdmin ? ChatColor.Admin : ChatColor.Normal;
            chatData._messageColor = ChatColor.Normal;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(client.UserFile.Username, message);
            ChatManagerHelper.ShowChatInConsole(client.UserFile.Username, message);

            DiscordBridge.TryRelayGameChatToDiscord(chatData._username, chatData._message);
        }

        public static void BroadcastDiscordMessage(string client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = client;
            chatData._message = message;
            chatData._usernameColor = ChatColor.Discord;
            chatData._messageColor = ChatColor.Discord;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(client, message);
            ChatManagerHelper.ShowChatInConsole(client, message, true);
        }

        public static void BroadcastConsoleMessage(string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = SystemName;
            chatData._message = message;
            chatData._usernameColor = ChatColor.Console;
            chatData._messageColor = ChatColor.Console;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(chatData._username, message);
            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
        }

        public static void BroadcastServerNotification(string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = NotificationName;
            chatData._message = message;
            chatData._usernameColor = ChatColor.Server;
            chatData._messageColor = ChatColor.Server;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ChatManager, chatData);

            WriteToLogs(chatData._username, message);
            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
        }

        public static void SendConsoleMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = SystemName;
            chatData._message = message;
            chatData._usernameColor = ChatColor.Console;
            chatData._messageColor = ChatColor.Console;

            client.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);
        }

        public static void SendServerMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = NotificationName;
            chatData._message = message;
            chatData._usernameColor = ChatColor.Server;
            chatData._messageColor = ChatColor.Server;

            client.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);
        }

        private static void WriteToLogs(string username, string message)
        {
            LogSemaphore.WaitOne();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{DateTime.Now:HH:mm:ss}] | [" + username + "]: " + message);
            stringBuilder.Append(Environment.NewLine);

            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = (dateTime.Year + "-" + dateTime.Month.ToString("D2") + "-" + dateTime.Day.ToString("D2")).ToString();
            string nowFullPath = Master.ChatLogsPath + Path.DirectorySeparatorChar + nowFileName + ".txt";

            File.AppendAllText(nowFullPath, stringBuilder.ToString());
            stringBuilder.Clear();

            LogSemaphore.Release();
        }
    }

    public static class ChatManagerHelper
    {
        public static ServerClient GetUserFromName(string username)
        {
            return ServerNetwork.Instance.GetConnectedClientFromUsername(username);
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