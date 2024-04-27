using Shared;
using System;
using System.Text;
using System.Threading;

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
            "Use '/help' to check available commands"
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
    
        public static void ParseClientMessages(ServerClient client, Packet packet)
        {
            ChatData chatData = (ChatData)Serializer.ConvertBytesToObject(packet.contents);
            
            for(int i = 0; i < chatData.messages.Count(); i++)
            {
                if (chatData.messages[i].StartsWith("/")) ExecuteChatCommand(client, chatData.messages[i]);
                else BroadcastChatMessage(client, chatData.messages[i]);
            }
        }

        private static void ExecuteChatCommand(ServerClient client, string command)
        {
            commandSemaphore.WaitOne();

            ChatCommand toFind = ChatCommandManager.chatCommands.ToList().Find(x => x.prefix == command);
            if (toFind == null) BroadcastSystemMessage(client, new string[] { "Command was not found" });
            else
            {
                ChatCommandManager.targetClient = client;
                toFind.commandAction.Invoke();
            }

            Logger.WriteToConsole($"[Chat command] > {client.username} > {command}");

            commandSemaphore.Release();
        }

        private static void BroadcastChatMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData.usernames.Add(client.username);
            chatData.messages.Add(message);

            if (client.isAdmin)
            {
                chatData.userColors.Add(((int)CommonEnumerators.MessageColor.Admin).ToString());
                chatData.messageColors.Add(((int)CommonEnumerators.MessageColor.Admin).ToString());
            }

            else
            {
                chatData.userColors.Add(((int)CommonEnumerators.MessageColor.Normal).ToString());
                chatData.messageColors.Add(((int)CommonEnumerators.MessageColor.Normal).ToString());
            }

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ChatPacket), chatData);
            foreach (ServerClient cClient in Network.connectedClients.ToArray()) cClient.listener.EnqueuePacket(packet);

            WriteToLogs(client.username, message);
            if (Master.serverConfig.DisplayChatInConsole) Logger.WriteToConsole($"[Chat] > {client.username} > {message}", Logger.LogMode.Normal, false);
        }

        public static void BroadcastServerMessage(string messageToSend)
        {
            ChatData chatData = new ChatData();
            chatData.usernames.Add("CONSOLE");
            chatData.messages.Add(messageToSend);
            chatData.userColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());
            chatData.messageColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ChatPacket), chatData);
            foreach (ServerClient client in Network.connectedClients.ToArray()) client.listener.EnqueuePacket(packet);

            WriteToLogs("CONSOLE", messageToSend);
            if (Master.serverConfig.DisplayChatInConsole) Logger.WriteToConsole($"[Chat] > CONSOLE > {messageToSend}", Logger.LogMode.Normal, false);
        }

        public static void BroadcastSystemMessage(ServerClient client, string[] messagesToSend)
        {
            ChatData chatData = new ChatData();
            for(int i = 0; i < messagesToSend.Count(); i++)
            {
                chatData.usernames.Add("CONSOLE");
                chatData.messages.Add(messagesToSend[i]);
                chatData.userColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());
                chatData.messageColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());
            }

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ChatPacket), chatData);
            client.listener.EnqueuePacket(packet);
        }
    }

    public static class ChatCommandManager
    {
        public static ServerClient targetClient;

        private static ChatCommand helpCommand = new ChatCommand("/help", 0,
            "Shows a list of all available commands",
            ChatHelpCommandAction);

        private static ChatCommand pingCommand = new ChatCommand("/ping", 0,
            "Checks if the connection to the server is working",
            ChatPingCommandAction);

        private static ChatCommand disconnectCommand = new ChatCommand("/dc", 0,
            "Forcefully disconnects you from the server",
            ChatDisconnectCommandAction);

        private static ChatCommand stopVisitCommand = new ChatCommand("/sv", 0,
            "Forcefully disconnects you from a visit",
            ChatStopVisitCommandAction);

        public static ChatCommand[] chatCommands = new ChatCommand[]
        {
            helpCommand,
            pingCommand,
            disconnectCommand,
            stopVisitCommand
        };

        private static void ChatHelpCommandAction()
        {
            List<string> messagesToSend = new List<string> { "List of available commands:" };
            foreach (ChatCommand command in chatCommands) messagesToSend.Add($"{command.prefix} - {command.description}");
            ChatManager.BroadcastSystemMessage(targetClient, messagesToSend.ToArray());
        }

        private static void ChatPingCommandAction()
        {
            List<string> messagesToSend = new List<string> { "Pong!" };
            ChatManager.BroadcastSystemMessage(targetClient, messagesToSend.ToArray());
        }

        private static void ChatDisconnectCommandAction()
        {
            targetClient.listener.disconnectFlag = true;
        }

        private static void ChatStopVisitCommandAction()
        {
            OnlineVisitManager.SendVisitStop(targetClient);
        }
    }
}
