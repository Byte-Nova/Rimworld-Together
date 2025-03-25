using Shared;
using static Shared.CommonEnumerators;
using static GameServer.Commands.ChatCommandActions;
using static GameServer.Commands.ChatCommands;
using GameServer.Managers;
using GameServer.TCP;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Commands
{
    public class BaseChatCommand
    {
        public string prefix;
        public string description;
        public int parameters;
        public Action commandAction;

        public BaseChatCommand(string prefix, int parameters, string description, Action commandAction)
        {
            this.prefix = prefix;
            this.parameters = parameters;
            this.description = description;
            this.commandAction = commandAction;
        }
    }

    public static class ChatCommands
    {
        private static readonly BaseChatCommand helpCommand = new BaseChatCommand("/help", 0,
            "Shows a list of all available commands",
            HelpCommandAction);

        private static readonly BaseChatCommand toolsCommand = new BaseChatCommand("/tools", 0,
            "Shows a list of all available chat tools",
            ToolsCommandAction);

        private static readonly BaseChatCommand pingCommand = new BaseChatCommand("/ping", 0,
            "Checks if the connection to the server is working",
            PingCommandAction);

        private static readonly BaseChatCommand disconnectCommand = new BaseChatCommand("/dc", 0,
            "Forcefully disconnects you from the server",
            DisconnectCommandAction);

        private static readonly BaseChatCommand privateMessage = new BaseChatCommand("/w", 0,
            "Sends a private message to a specific user",
            PrivateMessageCommandAction);

        public static readonly BaseChatCommand[] commands = new BaseChatCommand[]
        {
            helpCommand,
            toolsCommand,
            pingCommand,
            disconnectCommand,
            privateMessage
        };
    }

    public static class ChatCommandActions
    {
        public static ServerClient? targetClient;
        public static string[]? command;

        public static void HelpCommandAction()
        {
            if (targetClient == null) return;
            List<string> messagesToSend = new List<string> { "List of available commands:" };
            foreach (BaseChatCommand cmd in ChatCommands.commands)
            {
                messagesToSend.Add($"{cmd.prefix} - {cmd.description}");
            }
            foreach (string str in messagesToSend)
            {
                ChatManager.SendConsoleMessage(targetClient, str);
            }
        }

        public static void ToolsCommandAction()
        {
            if (targetClient == null) return;
            foreach (string str in ChatManager.defaultTextTools)
            {
                ChatManager.SendConsoleMessage(targetClient, str);
            }
        }

        public static void PingCommandAction()
        {
            if (targetClient == null) return;
            ChatManager.SendConsoleMessage(targetClient, "Pong!");
        }

        public static void DisconnectCommandAction()
        {
            if (targetClient == null) return;
            targetClient.listener.DisconnectFlag = true;
        }

        public static void PrivateMessageCommandAction()
        {
            if (targetClient == null || command == null) return;

            // Concatenate the message from the parameters, starting at index 2.
            string message = string.Join(" ", command.Skip(2)).Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                ChatManager.SendConsoleMessage(targetClient, "Message was empty.");
                return;
            }

            // Get the username from the second parameter (after removing any '@').
            string username = ChatManagerHelper.GetUsernameFromMention(command[1]);
            ServerClient? toFind = ChatManagerHelper.GetUserFromName(username);
            if (toFind == null)
            {
                ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                return;
            }
            if (toFind == targetClient)
            {
                ChatManager.SendConsoleMessage(targetClient, "Can't send a whisper to yourself.");
                return;
            }

            // Prepare and send the private message to both sender and recipient.
            ChatData chatData = new ChatData();
            chatData._message = message;
            chatData._usernameColor = UserColor.Private;
            chatData._messageColor = MessageColor.Private;

            // Send to sender
            chatData._username = $">> {toFind.userFile.Label}";
            Packet packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            targetClient.listener.EnqueuePacket(packet);

            // Send to recipient
            chatData._username = $"<< {targetClient.userFile.Label}";
            packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            toFind.listener.EnqueuePacket(packet);

            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
        }
    }
}