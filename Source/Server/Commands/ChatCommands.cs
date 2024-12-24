using Shared;
using static Shared.CommonEnumerators;
using static GameServer.ChatCommandActions;
using static GameServer.ChatCommands;

namespace GameServer
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

        private static readonly BaseChatCommand stopOnlineActivityCommand = new BaseChatCommand("/stopactivity", 0,
            "Forcefully disconnects you from an activity",
            StopOnlineActivityCommandAction);

        private static readonly BaseChatCommand privateMessage = new BaseChatCommand("/w", 0,
            "Sends a private message to a specific user",
            PrivateMessageCommandAction);

        public static readonly BaseChatCommand[] commands = new BaseChatCommand[]
        {
            helpCommand,
            toolsCommand,
            pingCommand,
            disconnectCommand,
            stopOnlineActivityCommand,
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
            else
            {
                List<string> messagesToSend = new List<string> { "List of available commands:" };
                foreach (BaseChatCommand command in commands) messagesToSend.Add($"{command.prefix} - {command.description}");

                foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(targetClient, str);
            }
        }

        public static void ToolsCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                foreach (string str in ChatManager.defaultTextTools)
                {
                    ChatManager.SendConsoleMessage(targetClient, str);
                }
            }
        }

        public static void PingCommandAction()
        {
            if (targetClient == null) return;
            else ChatManager.SendConsoleMessage(targetClient, "Pong!");
        }

        public static void DisconnectCommandAction()
        {
            if (targetClient == null) return;
            else targetClient.listener.disconnectFlag = true;
        }

        public static void StopOnlineActivityCommandAction()
        {
            if (targetClient == null) return;
            else OnlineActivityManager.StopActivity(targetClient);
        }

        public static void PrivateMessageCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                string message = "";
                for (int i = 2; i < command.Length; i++) message += command[i] + " ";

                if (string.IsNullOrWhiteSpace(message)) ChatManager.SendConsoleMessage(targetClient, "Message was empty.");
                else
                {
                    ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                    if (toFind == null) ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                    else
                    {
                        //Don't allow players to send wispers to themselves
                        if (toFind == targetClient) ChatManager.SendConsoleMessage(targetClient, "Can't send a whisper to yourself.");
                        else
                        {
                            ChatData chatData = new ChatData();
                            chatData._message = message;
                            chatData._usernameColor = UserColor.Private;
                            chatData._messageColor = MessageColor.Private;

                            //Send to sender
                            chatData._username = $">> {toFind.userFile.Username}";
                            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
                            targetClient.listener.EnqueuePacket(packet);

                            //Send to recipient
                            chatData._username = $"<< {targetClient.userFile.Username}";
                            packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
                            toFind.listener.EnqueuePacket(packet);

                            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
                        }
                    }
                }
            }
        }
    }
}