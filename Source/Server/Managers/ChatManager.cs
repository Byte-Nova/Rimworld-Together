using Shared;
using System.Text;
using static Shared.CommonEnumerators;

namespace GameServer
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

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            ChatData chatData = Serializer.ConvertBytesToObject<ChatData>(packet.contents);

            if (chatData._message.StartsWith("/")) ExecuteChatCommand(client, chatData._message.Split(' '));
            else BroadcastChatMessage(client, chatData._message);
        }

        private static void ExecuteChatCommand(ServerClient client, string[] command)
        {
            commandSemaphore.WaitOne();

            BaseChatCommand toFind = ChatManagerHelper.GetCommandFromName(command[0]);
            if (toFind == null) SendConsoleMessage(client, "Command was not found.");
            else
            {
                ChatCommandActions.targetClient = client;
                ChatCommandActions.command = command;
                toFind.commandAction.Invoke();
            }

            string chatCommand = "";
            for (int i = 0; i < command.Length; i++) chatCommand += command[i] + "";

            ChatManagerHelper.ShowChatInConsole(client.userFile.Username, chatCommand);

            commandSemaphore.Release();
        }

        private static void BroadcastChatMessage(ServerClient client, string message)
        {
            if (Master.serverConfig == null) return;

            ChatData chatData = new ChatData();
            chatData._username = client.userFile.Username;
            chatData._message = message;
            chatData._usernameColor = client.userFile.IsAdmin ? UserColor.Admin : UserColor.Normal;
            chatData._messageColor = client.userFile.IsAdmin ? MessageColor.Admin : MessageColor.Normal;

            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);

            WriteToLogs(client.userFile.Username, message);
            ChatManagerHelper.ShowChatInConsole(client.userFile.Username, message);

            if (Master.discordConfig.Enabled && Master.discordConfig.ChatChannelId != 0) DiscordManager.SendMessageToChatChannel(chatData._username, message);
        }

        public static void BroadcastDiscordMessage(string client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = client;
            chatData._message = message;
            chatData._usernameColor = UserColor.Discord;
            chatData._messageColor = MessageColor.Discord;

            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);

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

            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);

            if (Master.discordConfig.Enabled && Master.discordConfig.ChatChannelId != 0) DiscordManager.SendMessageToChatChannel(chatData._username, message);

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

            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);

            if (Master.discordConfig.Enabled && Master.discordConfig.ChatChannelId != 0) DiscordManager.SendMessageToChatChannel(chatData._username, message);

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

            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendServerMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData._username = notificationName;
            chatData._message = message;
            chatData._usernameColor = UserColor.Server;
            chatData._messageColor = MessageColor.Server;

            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
            client.listener.EnqueuePacket(packet);
        }

        private static void WriteToLogs(string username, string message)
        {
            logSemaphore.WaitOne();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{DateTime.Now:HH:mm:ss}] | [" + username + "]: " + message);
            stringBuilder.Append(Environment.NewLine);
    
            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = (dateTime.Year + "-" + dateTime.Month.ToString("D2") + "-" + dateTime.Day.ToString("D2")).ToString();
            string nowFullPath = Master.chatLogsPath + Path.DirectorySeparatorChar + nowFileName + ".txt";
    
            File.AppendAllText(nowFullPath, stringBuilder.ToString());
            stringBuilder.Clear();

            logSemaphore.Release();
        }
    }

    public static class ChatManagerHelper
    {
        public static ServerClient GetUserFromName(string username)
        {
            return NetworkHelper.GetConnectedClientFromUsername(username);
        }

        public static BaseChatCommand GetCommandFromName(string commandName)
        {
            return ChatCommands.commands.ToArray().FirstOrDefault(x => x.prefix == commandName);
        }

        public static string GetUsernameFromMention(string mention)
        {
            return mention.Replace("@", "");
        }

        public static void ShowChatInConsole(string username, string message, bool fromDiscord = false)
        {
            if (!Master.serverConfig.DisplayChatInConsole) return;
            else 
            {
                if (fromDiscord) Logger.Message($"[Discord] > {username} > {message}");
                else Logger.Message($"[Chat] > {username} > {message}");
            }
        }
    }
}

