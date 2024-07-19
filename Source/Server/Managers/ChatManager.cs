using Shared;
using System.Text;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ChatManager
    {
        private static readonly Semaphore logSemaphore = new Semaphore(1, 1);
        private static readonly Semaphore commandSemaphore = new Semaphore(1, 1);

        public static readonly string[] defaultJoinMessages = new string[]
        {
            "Welcome to the global chat!",
            "Please be considerate with others and have fun!",
            "Use '/help' to check all the available commands."
        };

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
    
        public static void ParseClientMessage(ServerClient client, Packet packet)
        {
            ChatData chatData = Serializer.ConvertBytesToObject<ChatData>(packet.contents);

            if (chatData.message.StartsWith("/")) ExecuteChatCommand(client, chatData.message);
            else BroadcastChatMessage(client, chatData.message);
        }

        private static void ExecuteChatCommand(ServerClient client, string command)
        {
            commandSemaphore.WaitOne();

            ChatCommand toFind = ChatCommandManager.chatCommands.ToList().Find(x => x.prefix == command);
            if (toFind == null) BroadcastSystemMessages(client, new string[] { "Command was not found" });
            else
            {
                ChatCommandManager.targetClient = client;
                toFind.commandAction.Invoke();
            }

            Logger.Message($"[Chat command] > {client.userFile.Username} > {command}");

            commandSemaphore.Release();
        }

        private static void BroadcastChatMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData.username = client.userFile.Username;
            chatData.message = message;

            if (client.userFile.IsAdmin)
            {
                chatData.userColor = UserColor.Admin;
                chatData.messageColor = MessageColor.Admin;
            }

            else
            {
                chatData.userColor = UserColor.Normal;
                chatData.messageColor = MessageColor.Normal;
            }

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
            foreach (ServerClient cClient in Network.connectedClients.ToArray()) cClient.listener.EnqueuePacket(packet);

            WriteToLogs(client.userFile.Username, message);
            if (Master.serverConfig.DisplayChatInConsole) Logger.Message($"[Chat] > {client.userFile.Username} > {message}");
        }

        public static void BroadcastServerMessage(string messageToSend)
        {
            ChatData chatData = new ChatData();
            chatData.username = "CONSOLE";
            chatData.message = messageToSend;
            chatData.userColor = UserColor.Console;
            chatData.messageColor = MessageColor.Console;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
            foreach (ServerClient client in Network.connectedClients.ToArray()) client.listener.EnqueuePacket(packet);

            WriteToLogs("CONSOLE", messageToSend);
            if (Master.serverConfig.DisplayChatInConsole) Logger.Message($"[Chat] > CONSOLE > {messageToSend}");
        }

        public static void BroadcastSystemMessages(ServerClient client, string[] messagesToSend)
        {
            ChatData chatData = new ChatData();
            for (int i = 0; i < messagesToSend.Length; i++)
            {
                chatData.username = "CONSOLE";
                chatData.message = messagesToSend[i];
                chatData.userColor = UserColor.Console;
                chatData.messageColor = MessageColor.Console;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
                client.listener.EnqueuePacket(packet);
            }
        }
    }

    public static class ChatCommandManager
    {
        public static ServerClient targetClient;

        private static readonly ChatCommand helpCommand = new ChatCommand("/help", 0,
            "Shows a list of all available commands",
            ChatHelpCommandAction);

        private static readonly ChatCommand toolsCommand = new ChatCommand("/tools", 0,
            "Shows a list of all available chat tools",
            ChatToolsCommandAction);

        private static readonly ChatCommand pingCommand = new ChatCommand("/ping", 0,
            "Checks if the connection to the server is working",
            ChatPingCommandAction);

        private static readonly ChatCommand disconnectCommand = new ChatCommand("/dc", 0,
            "Forcefully disconnects you from the server",
            ChatDisconnectCommandAction);

        private static readonly ChatCommand stopOnlineActivityCommand = new ChatCommand("/sv", 0,
            "Forcefully disconnects you from a visit",
            ChatStopOnlineActivityCommandAction);

        public static readonly ChatCommand[] chatCommands = new ChatCommand[]
        {
            helpCommand,
            toolsCommand,
            pingCommand,
            disconnectCommand,
            stopOnlineActivityCommand
        };

        private static void ChatHelpCommandAction()
        {
            List<string> messagesToSend = new List<string> { "List of available commands:" };
            foreach (ChatCommand command in chatCommands) messagesToSend.Add($"{command.prefix} - {command.description}");
            ChatManager.BroadcastSystemMessages(targetClient, messagesToSend.ToArray());
        }

        private static void ChatToolsCommandAction()
        {
            List<string> messagesToSend = new List<string>
            {
                "List of available text tools:",
                "'b' inside brackets - Followed by the text you want to turn [b]bold",
                "'i' inside brackets - Followed by the text you want to turn [i]cursive",
                "HTML color inside brackets - Followed by the text you want to [ff0000]change color"
            };

            ChatManager.BroadcastSystemMessages(targetClient, messagesToSend.ToArray());
        }

        private static void ChatPingCommandAction()
        {
            List<string> messagesToSend = new List<string> { "Pong!" };
            ChatManager.BroadcastSystemMessages(targetClient, messagesToSend.ToArray());
        }

        private static void ChatDisconnectCommandAction()
        {
            targetClient.listener.disconnectFlag = true;
        }

        private static void ChatStopOnlineActivityCommandAction()
        {
            OnlineActivityManager.SendVisitStop(targetClient);
        }
    }
}
