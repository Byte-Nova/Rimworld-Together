using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Misc.Commands;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers
{
    public static class ChatManager
    {
        public static string[] defaultJoinMessages = new string[]
        {
            "Welcome to the global chat!", "Please be considerate with others and have fun!", "Use '/help' to check available commands"
        };

        public static void ParseClientMessages(ServerClient client, Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = (ChatMessagesJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            
            for(int i = 0; i < chatMessagesJSON.messages.Count(); i++)
            {
                if (chatMessagesJSON.messages[i].StartsWith("/")) ExecuteCommand(client, packet);
                else BroadcastClientMessages(client, packet);
            }
        }

        public static void ExecuteCommand(ServerClient client, Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = (ChatMessagesJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            ChatCommand toFind = ChatCommandManager.chatCommands.ToList().Find(x => x.prefix == chatMessagesJSON.messages[0]);
            if (toFind == null) SendMessagesToClient(client, new string[] { "Command was not found" });
            else
            {
                ChatCommandManager.invoker = client;

                toFind.commandAction.Invoke();
            }

            Logger.WriteToConsole($"[Chat command] > {client.username} > {chatMessagesJSON.messages[0]}");
        }

        public static void BroadcastClientMessages(ServerClient client, Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = (ChatMessagesJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            for(int i = 0; i < chatMessagesJSON.messages.Count(); i++)
            {
                if (client.isAdmin)
                {
                    chatMessagesJSON.userColors.Add(((int)CommonEnumerators.MessageColor.Admin).ToString());
                    chatMessagesJSON.messageColors.Add(((int)CommonEnumerators.MessageColor.Admin).ToString());
                }

                else
                {
                    chatMessagesJSON.userColors.Add(((int)CommonEnumerators.MessageColor.Normal).ToString());
                    chatMessagesJSON.messageColors.Add(((int)CommonEnumerators.MessageColor.Normal).ToString());
                }
            }

            Packet rPacket = Packet.CreatePacketFromJSON("ChatPacket", chatMessagesJSON);
            foreach (ServerClient cClient in Network.Network.connectedClients.ToArray()) cClient.clientListener.SendData(rPacket);

            Logger.WriteToConsole($"[Chat] > {client.username} > {chatMessagesJSON.messages[0]}");
        }

        public static void BroadcastServerMessages(string messageToSend)
        {
            ChatMessagesJSON chatMessagesJSON = new ChatMessagesJSON();
            chatMessagesJSON.usernames.Add("CONSOLE");
            chatMessagesJSON.messages.Add(messageToSend);
            chatMessagesJSON.userColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());
            chatMessagesJSON.messageColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());

            Packet packet = Packet.CreatePacketFromJSON("ChatPacket", chatMessagesJSON);

            foreach (ServerClient client in Network.Network.connectedClients.ToArray())
            {
                client.clientListener.SendData(packet);
            }

            Logger.WriteToConsole($"[Chat] > {"CONSOLE"} > {"127.0.0.1"} > {chatMessagesJSON.messages[0]}");
        }

        public static void SendMessagesToClient(ServerClient client, string[] messagesToSend)
        {
            ChatMessagesJSON chatMessagesJSON = new ChatMessagesJSON();
            for(int i = 0; i < messagesToSend.Count(); i++)
            {
                chatMessagesJSON.usernames.Add("CONSOLE");
                chatMessagesJSON.messages.Add(messagesToSend[i]);
                chatMessagesJSON.userColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());
                chatMessagesJSON.messageColors.Add(((int)CommonEnumerators.MessageColor.Console).ToString());
            }

            Packet packet = Packet.CreatePacketFromJSON("ChatPacket", chatMessagesJSON);
            client.clientListener.SendData(packet);
        }
    }

    public static class ChatCommandManager
    {
        public static ServerClient invoker;

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
            List<string> messagesToSend = new List<string>();
            messagesToSend.Add("List of available commands: ");
            foreach (ChatCommand command in chatCommands) messagesToSend.Add($"{command.prefix} - {command.description}");
            ChatManager.SendMessagesToClient(invoker, messagesToSend.ToArray());
        }

        private static void ChatPingCommandAction()
        {
            List<string> messagesToSend = new List<string>();
            messagesToSend.Add("Pong!");
            ChatManager.SendMessagesToClient(invoker, messagesToSend.ToArray());
        }

        private static void ChatDisconnectCommandAction()
        {
            invoker.disconnectFlag = true;
        }

        private static void ChatStopVisitCommandAction()
        {
            VisitDetailsJSON visitDetailsJSON = new VisitDetailsJSON();
            visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Stop).ToString();

            VisitManager.SendVisitStop(invoker, visitDetailsJSON);
        }
    }
}
