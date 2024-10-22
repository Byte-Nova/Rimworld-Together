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
            "Use '/list' to check all the available commands."
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

            if (chatData._message.StartsWith("/"))
            {
                SendConsoleMessage(client, chatData._message);
                ExecuteChatCommand(client, chatData._message.Split(' '));
            }
            else BroadcastChatMessage(client, chatData._message);
        }

        private static void ExecuteChatCommand(ServerClient client, string[] command)
        {
            commandSemaphore.WaitOne();

            ChatCommand toFind = ChatManagerHelper.GetCommandFromName(command[0]);
            if (toFind == null) SendConsoleMessage(client, "Command was not found.");
            else
            {
                ChatCommandManager.targetClient = client;
                ChatCommandManager.command = command;
                if (toFind.parameters >= command.Length && toFind.parameters >= 0)
                {
                    SendConsoleMessage(client, "Invalid arguments.");
                }
                else
                if (toFind.adminOnly && !client.userFile.IsAdmin)
                {
                    SendConsoleMessage(client, "You should be a admin to execute this command.");
                }
                else 
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
            ChatManagerHelper.ShowChatInConsole("", message);
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

    public static class ChatCommandManager
    {
        public static ServerClient? targetClient;
        public static string[]? command;

        private static readonly ChatCommand listCommand = new ChatCommand("/list", 0,
            "Shows a list of all available commands", false, ListCommandAction);

        private static readonly ChatCommand helpCommand = new ChatCommand("/help", 1,
            "Shows a list of all available commands", false,
            HelpCommandAction, "{command}");

        private static readonly ChatCommand toolsCommand = new ChatCommand("/tools", 0,
            "Shows a list of all available chat tools", false,
            ToolsCommandAction);

        private static readonly ChatCommand pingCommand = new ChatCommand("/ping", 0,
            "Checks if the connection to the server is working", false,
            PingCommandAction);

        private static readonly ChatCommand disconnectCommand = new ChatCommand("/dc", 0,
            "Forcefully disconnects you from the server", false,
            DisconnectCommandAction);

        private static readonly ChatCommand stopOnlineActivityCommand = new ChatCommand("/stopactivity", 0,
            "Forcefully disconnects you from an activity", false,
            StopOnlineActivityCommandAction);
        
        private static readonly ChatCommand privateMessage = new ChatCommand("/w", -1,
            "Sends a private message to a specific user", false,
            PrivateMessageCommandAction, "{username} {message}");
        
        private static readonly ChatCommand kickCommand = new ChatCommand("/kick", 1,
            "Kicks the selected player from the server", true, KickCommandAction, "{username}");
        
        private static readonly ChatCommand banCommand = new ChatCommand("/ban", 1,
            "Bans the selected player from the server", true, BanCommandAction, "{username}");
        
        private static readonly ChatCommand pardonCommand = new ChatCommand("/pardon", 1,
            "Pardons the selected player from the server", true, PardonCommandAction, "{username}");

        private static readonly ChatCommand doSiteRewardsCommand = new ChatCommand("/siterewards", 0,
            "Forces site rewards to run", true, DoSiteRewardsAction);

        private static readonly ChatCommand giveCommand = new ChatCommand("/give", 1,
                "Gives items to player", true, GiveCommandAction);
            
        public static readonly ChatCommand[] chatCommands = new ChatCommand[]
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

        private static void ListCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                List<string> messagesToSend = new List<string> { "List of available commands:" };
                foreach (ChatCommand command in chatCommands)
                {
                    if (!command.adminOnly)
                        messagesToSend.Add($"{command.prefix} - {command.description}");
                    if (targetClient.userFile.IsAdmin && command.adminOnly)
                        messagesToSend.Add($"{command.prefix} - {command.description}");
                }
                foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(targetClient, str);
            }
        }

        private static void HelpCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ChatCommand toGetCommand = ChatManagerHelper.GetCommandFromName(command[1]);
                if (toGetCommand == null) ChatManager.SendConsoleMessage(targetClient, "Command was not found");
                else
                {
                    List<string> messagesToSend = new List<string> {$"{toGetCommand.prefix}", $"Description: {toGetCommand.description}" };
                    if (toGetCommand.arguments.Length > 1)
                        messagesToSend.Add($"Syntax: {toGetCommand.prefix} {toGetCommand.arguments}");
                    foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(targetClient, str);
                }
            }
        }

        private static void ToolsCommandAction()
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

        private static void PingCommandAction()
        {
            if (targetClient == null) return;
            else ChatManager.SendConsoleMessage(targetClient, "Pong!");
        }

        private static void DisconnectCommandAction()
        {
            if (targetClient == null) return;
            else targetClient.listener.disconnectFlag = true;
        }

        private static void StopOnlineActivityCommandAction()
        {
            if (targetClient == null) return;
            else OnlineActivityManager.StopActivity(targetClient);
        }

        private static void PrivateMessageCommandAction()
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

        private static void KickCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                if (toFind == null) ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                else
                {
                    toFind.listener.disconnectFlag = true;
                    ChatManager.SendConsoleMessage(targetClient, $"{toFind.userFile.Username} has been kicked.");
                }
            }
        }

        private static void BanCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                if (toFind == null) ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                else
                {
                    UserManager.BanPlayerFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                    ChatManager.SendConsoleMessage(targetClient, $"{toFind.userFile.Username} has been banned.");
                }
            }
        }
        private static void PardonCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                if (toFind == null) ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                else
                {
                    UserManager.PardonPlayerFromName(toFind.userFile.Username);
                    ChatManager.SendConsoleMessage(targetClient, $"{toFind.userFile.Username} has been pardoned.");
                }
            }
        }
        private static void DoSiteRewardsAction()
        {
            if (targetClient == null) return;
            else
            {
                SiteManager.SiteRewardTick();
                ChatManager.SendConsoleMessage(targetClient, "Forced Site Rewards.");
            }
        }

        private static void GiveCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                ThingDataFile sendedThing = new ThingDataFile(); 
                ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                if (toFind == null) {ChatManager.SendConsoleMessage(targetClient, "User was not found.");
                    return;
                }
                if (command.Length <= 3) {ChatManager.SendConsoleMessage(targetClient, $"Def of thing isn't specified");
                    return;
                }
                else
                {
                    sendedThing.DefName = command[2];
                    if (command.Length <= 4) sendedThing.Quantity = 1;
                    else sendedThing.Quantity = int.Parse(command[3]);
                    if (command.Length <= 5) sendedThing.Quality = 2;
                    else sendedThing.Quality = int.Parse(command[4]);
                }
                    Packet packet = Packet.CreatePacketFromObject(nameof(GiveCommandManager), sendedThing);
                    toFind.listener.EnqueuePacket(packet);
            }
        }
    }

    public static class ChatManagerHelper
    {
        public static ServerClient GetUserFromName(string username)
        {
            return NetworkHelper.GetConnectedClientsSafe().FirstOrDefault(fetch => fetch.userFile.Username == username);
        }

        public static ChatCommand GetCommandFromName(string commandName)
        {
            return ChatCommandManager.chatCommands.ToArray().FirstOrDefault(x => x.prefix == commandName);
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

