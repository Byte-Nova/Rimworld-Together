using Shared;
using static Shared.CommonEnumerators;
using static GameServer.Commands.ChatCommandActions;
using static GameServer.Commands.ChatCommands;
using GameServer.Managers;
using TCPNetwork.Server;
using TCPNetwork.Packets;

namespace GameServer.Commands
{
    public static class ChatCommands
    {
        private static readonly CommandBase HelpCommand = new CommandBase("/help", 0,
            "Shows a list of all available commands",
            HelpCommandAction);

        private static readonly CommandBase ToolsCommand = new CommandBase("/tools", 0,
            "Shows a list of all available chat tools",
            ToolsCommandAction);

        private static readonly CommandBase PingCommand = new CommandBase("/ping", 0,
            "Checks if the connection to the server is working",
            PingCommandAction);

        private static readonly CommandBase DisconnectCommand = new CommandBase("/dc", 0,
            "Forcefully disconnects you from the server",
            DisconnectCommandAction);

        private static readonly CommandBase PMCommand = new CommandBase("/w", 0,
            "Sends a private message to a specific user",
            PrivateMessageCommandAction);

        public static readonly CommandBase[] commands = new CommandBase[]
        {
            HelpCommand,
            ToolsCommand,
            PingCommand,
            DisconnectCommand,
            PMCommand
        };
    }

    public static class ChatCommandActions
    {
        public static ServerClient TargetClient { get; set; }

        public static string[] Command { get; set; }

        public static void HelpCommandAction()
        {
            if (TargetClient == null) return;
            else
            {
                List<string> messagesToSend = new List<string> { "List of available commands:" };
                foreach (CommandBase command in commands) messagesToSend.Add($"{command.Prefix} - {command.Description}");

                foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(TargetClient, str);
            }
        }

        public static void ToolsCommandAction()
        {
            if (TargetClient == null) return;
            else
            {
                foreach (string str in ChatManager.defaultTextTools)
                {
                    ChatManager.SendConsoleMessage(TargetClient, str);
                }
            }
        }

        public static void PingCommandAction()
        {
            if (TargetClient == null) return;
            else ChatManager.SendConsoleMessage(TargetClient, "Pong!");
        }

        public static void DisconnectCommandAction()
        {
            if (TargetClient == null) return;
            else TargetClient.Listener.DisconnectFlag = true;
        }

        public static void PrivateMessageCommandAction()
        {
            if (TargetClient == null) return;
            else
            {
                string message = "";
                for (int i = 2; i < Command.Length; i++) message += Command[i] + " ";

                if (string.IsNullOrWhiteSpace(message)) ChatManager.SendConsoleMessage(TargetClient, "Message was empty.");
                else
                {
                    ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(Command[1]));
                    if (toFind == null) ChatManager.SendConsoleMessage(TargetClient, "User was not found.");
                    else
                    {
                        //Don't allow players to send wispers to themselves
                        if (toFind == TargetClient) ChatManager.SendConsoleMessage(TargetClient, "Can't send a whisper to yourself.");
                        else
                        {
                            ChatData chatData = new ChatData();
                            chatData._message = message;
                            chatData._usernameColor = UserColor.Private;
                            chatData._messageColor = MessageColor.Private;

                            //Send to sender
                            chatData._username = $">> {toFind.UserFile.Label}";
                            TargetClient.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);

                            //Send to recipient
                            chatData._username = $"<< {TargetClient.UserFile.Label}";
                            toFind.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);

                            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
                        }
                    }
                }
            }
        }
    }
}