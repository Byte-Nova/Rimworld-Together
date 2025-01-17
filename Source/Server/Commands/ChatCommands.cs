using System.Reflection.Metadata.Ecma335;
using Shared;
using static Shared.CommonEnumerators;
using static GameServer.Commands.ChatCommandActions;
using static GameServer.Commands.ChatCommands;
using GameServer.Managers;
using GameServer.TCP;

namespace GameServer.Commands
{
    public class BaseChatCommand
    {
        public string prefix;

        public string description;

        public int parameters;

        public Action commandAction;
        
        public string arguments;

        public bool adminOnly;

        public BaseChatCommand(string prefix, int parameters, string description, Action commandAction, string arguments = "", bool adminOnly = false)
        {
            this.prefix = prefix;
            this.parameters = parameters;
            this.description = description;
            this.commandAction = commandAction;
            this.arguments = arguments;
            this.adminOnly = adminOnly;
        }
    }

    public static class ChatCommands
    {
        private static readonly BaseChatCommand listCommand = new BaseChatCommand("/list", 0,
            "Shows a list of all available commands", ListCommandAction);

        private static readonly BaseChatCommand helpCommand = new BaseChatCommand("/help", 1,
            "Shows a more detailed info about command", 
            HelpCommandAction, "{command}");

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
        
        private static readonly BaseChatCommand privateMessage = new BaseChatCommand("/w", -1,
            "Sends a private message to a specific user", 
            PrivateMessageCommandAction, "{username} {message}");
        
        private static readonly BaseChatCommand kickCommand = new BaseChatCommand("/kick", 1,
            "Kicks the selected player from the server", KickCommandAction, "{username}", true);
        
        private static readonly BaseChatCommand banCommand = new BaseChatCommand("/ban", 1,
            "Bans the selected player from the server", BanCommandAction, "{username}",true );
        
        private static readonly BaseChatCommand pardonCommand = new BaseChatCommand("/pardon", 1,
            "Pardons the selected player from the server", PardonCommandAction, "{username}", true);

        private static readonly BaseChatCommand doSiteRewardsCommand = new BaseChatCommand("/siterewards", 0,
            "Forces site rewards to run", DoSiteRewardsAction, null , true);

        private static readonly BaseChatCommand giveCommand = new BaseChatCommand("/give", 1,
                "Gives items to player", GiveCommandAction, "{username} {defName} {Quantity} {Quality}", true);
            
        public static readonly BaseChatCommand[] сommands = new BaseChatCommand[]
        {
            listCommand,
            helpCommand,
            toolsCommand,
            pingCommand,
            disconnectCommand,
            stopOnlineActivityCommand,
            privateMessage,
            kickCommand,
            banCommand,
            pardonCommand,
            doSiteRewardsCommand,
            giveCommand
        };
    }

    public static class ChatCommandActions
    {
        public static ServerClient? targetClient;
        public static string[]? command;
        
        public static void ListCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                List<string> messagesToSend = new List<string> { "List of available commands:" };
                foreach (BaseChatCommand command in commands)
                {
                    if (!command.adminOnly)
                        messagesToSend.Add($"{command.prefix} - {command.description}");
                    if (targetClient.userFile.IsAdmin && command.adminOnly)
                        messagesToSend.Add($"{command.prefix} - {command.description}");
                }
                foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(targetClient, str);
            }
        }

        public static void HelpCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                BaseChatCommand toGetCommand = ChatManagerHelper.GetCommandFromName("/" + command[1]);
                if (toGetCommand == null) ChatManager.SendConsoleMessage(targetClient, "Command was not found");
                else
                {
                    List<string> messagesToSend = new List<string> {$"{toGetCommand.prefix}", $"Description: {toGetCommand.description}", $"Syntax: {toGetCommand.prefix} {toGetCommand.arguments}" };
                    foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(targetClient, str);
                }
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
        public static void KickCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                if (toFind == null) ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                else
                {
                    toFind.listener.disconnectFlag = true;
                    ChatManager.SendConsoleMessage(targetClient, $"{toFind.userFile.Uid} has been kicked by force.");
                }
            }
        }

        public static void BanCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                if (toFind == null) ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                else
                {
                    UserManager.BanPlayerFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                    ChatManager.SendConsoleMessage(targetClient, $"{toFind.userFile.Uid} has been banned.");
                }
            }
        }
        public static void PardonCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                if (toFind == null) ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                else
                {
                    UserManager.PardonPlayerFromName(toFind.userFile.Uid);
                    ChatManager.SendConsoleMessage(targetClient, $"{toFind.userFile.Uid} has been pardoned.");
                }
            }
        }
        public static void DoSiteRewardsAction()
        {
            if (targetClient == null) return;
            else
            {
                SiteManager.SiteRewardTick();
                ChatManager.SendConsoleMessage(targetClient, "Forced Site Rewards.");
            }
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
                            chatData._username = $">> {toFind.userFile.Label}";
                            Packet packet = Packet.CreatePacketFromObject(nameof(ChatManager), chatData);
                            targetClient.listener.EnqueuePacket(packet);

                            //Send to recipient
                            chatData._username = $"<< {targetClient.userFile.Label}";
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