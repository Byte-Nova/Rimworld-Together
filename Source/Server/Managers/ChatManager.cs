using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimworldTogether;
using Shared.JSON;
using Shared.JSON.Actions;
using Shared.Misc;

namespace GameServer
{
    public static class ChatManager
    {
        public enum UserColor { Normal, Admin, Console }

        public enum MessageColor { Normal, Admin, Console }

        public static string[] defaultJoinMessages = new string[]
        {
            "Welcome to the global chat!", "Please be considerate with others and have fun!", "Use '/help' to check available commands"
        };

        public static void ParseClientMessages(Client client, Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = Serializer.SerializeFromString<ChatMessagesJSON>(packet.contents[0]);
            
            for(int i = 0; i < chatMessagesJSON.messages.Count(); i++)
            {
                if (chatMessagesJSON.messages[i].StartsWith("/")) ExecuteCommand(client, packet);
                else BroadcastClientMessages(client, packet);
            }
        }

        public static void ExecuteCommand(Client client, Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = Serializer.SerializeFromString<ChatMessagesJSON>(packet.contents[0]);

            ChatCommand toFind = ChatCommandManager.chatCommands.ToList().Find(x => x.prefix == chatMessagesJSON.messages[0]);
            if (toFind == null) SendMessagesToClient(client, new string[] { "Command was not found" });
            else
            {
                ChatCommandManager.invoker = client;

                toFind.commandAction.Invoke();
            }
        }

        public static void BroadcastClientMessages(Client client, Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = Serializer.SerializeFromString<ChatMessagesJSON>(packet.contents[0]);
            for(int i = 0; i < chatMessagesJSON.messages.Count(); i++)
            {
                if (client.isAdmin)
                {
                    chatMessagesJSON.userColors.Add(((int)MessageColor.Admin).ToString());
                    chatMessagesJSON.messageColors.Add(((int)MessageColor.Admin).ToString());
                }

                else
                {
                    chatMessagesJSON.userColors.Add(((int)MessageColor.Normal).ToString());
                    chatMessagesJSON.messageColors.Add(((int)MessageColor.Normal).ToString());
                }
            }

            string[] contents = new string[] { Serializer.SerializeToString(chatMessagesJSON) };
            Packet rPacket = new Packet("ChatPacket", contents);
            foreach (Client cClient in Network.connectedClients.ToArray()) Network.SendData(cClient, rPacket);

            Logger.WriteToConsole($"[Chat] > {client.username} > {chatMessagesJSON.messages[0]}");
        }

        public static void BroadcastServerMessages(string messageToSend)
        {
            ChatMessagesJSON chatMessagesJSON = new ChatMessagesJSON();
            chatMessagesJSON.usernames.Add("CONSOLE");
            chatMessagesJSON.messages.Add(messageToSend);
            chatMessagesJSON.userColors.Add(((int)MessageColor.Console).ToString());
            chatMessagesJSON.messageColors.Add(((int)MessageColor.Console).ToString());

            string[] contents = new string[] { Serializer.SerializeToString(chatMessagesJSON) };
            Packet packet = new Packet("ChatPacket", contents);

            foreach (Client client in Network.connectedClients.ToArray())
            {
                Network.SendData(client, packet);
            }

            Logger.WriteToConsole($"[Chat] > {"CONSOLE"} > {"127.0.0.1"} > {chatMessagesJSON.messages[0]}");
        }

        public static void SendMessagesToClient(Client client, string[] messagesToSend)
        {
            ChatMessagesJSON chatMessagesJSON = new ChatMessagesJSON();
            for(int i = 0; i < messagesToSend.Count(); i++)
            {
                chatMessagesJSON.usernames.Add("CONSOLE");
                chatMessagesJSON.messages.Add(messagesToSend[i]);
                chatMessagesJSON.userColors.Add(((int)MessageColor.Console).ToString());
                chatMessagesJSON.messageColors.Add(((int)MessageColor.Console).ToString());
            }

            string[] contents = new string[] { Serializer.SerializeToString(chatMessagesJSON) };
            Packet packet = new Packet("ChatPacket", contents);
            Network.SendData(client, packet);
        }
    }

    public static class ChatCommandManager
    {
        public static Client invoker;

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
            visitDetailsJSON.visitStepMode = ((int)VisitManager.VisitStepMode.Stop).ToString();

            VisitManager.SendVisitStop(invoker, visitDetailsJSON);
        }
    }
}
