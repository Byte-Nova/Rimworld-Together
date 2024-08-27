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

            if (chatData.message.StartsWith("/")) ExecuteChatCommand(client, chatData.message.Split(' '));
            else BroadcastChatMessage(client, chatData.message);
        }

        private static void ExecuteChatCommand(ServerClient client, string[] command)
        {
            commandSemaphore.WaitOne();

            ChatCommand toFind = ChatManagerHelper.GetCommandFromName(command[0]);
            if (toFind == null) SendSystemMessage(client, "Command was not found.");
            else
            {
                ChatCommandManager.targetClient = client;
                ChatCommandManager.command = command;
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
            chatData.username = client.userFile.Username;
            chatData.message = message;
            chatData.userColor = client.userFile.IsAdmin ? UserColor.Admin : UserColor.Normal;
            chatData.messageColor = client.userFile.IsAdmin ? MessageColor.Admin : MessageColor.Normal;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
            NetworkHelper.SendPacketToAllClients(packet);

            WriteToLogs(client.userFile.Username, message);
            ChatManagerHelper.ShowChatInConsole(client.userFile.Username, message);

            if (Master.discordConfig.Enabled && Master.discordConfig.ChatChannelId != 0) DiscordManager.SendMessageToChatChannel(chatData.username, message);
        }

        public static void BroadcastDiscordMessage(string client, string message)
        {
            ChatData chatData = new ChatData();
            chatData.username = client;
            chatData.message = message;
            chatData.userColor = UserColor.Discord;
            chatData.messageColor = MessageColor.Discord;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
            NetworkHelper.SendPacketToAllClients(packet);

            WriteToLogs(client, message);
            ChatManagerHelper.ShowChatInConsole(client, message, true);
        }

        public static void BroadcastServerMessage(string message)
        {
            ChatData chatData = new ChatData();
            chatData.username = systemName;
            chatData.message = message;
            chatData.userColor = UserColor.Console;
            chatData.messageColor = MessageColor.Console;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
            NetworkHelper.SendPacketToAllClients(packet);

            if (Master.discordConfig.Enabled && Master.discordConfig.ChatChannelId != 0) DiscordManager.SendMessageToChatChannel(chatData.username, message);

            WriteToLogs(chatData.username, message);
            ChatManagerHelper.ShowChatInConsole(chatData.username, message);
        }

        public static void SendSystemMessage(ServerClient client, string message)
        {
            ChatData chatData = new ChatData();
            chatData.username = systemName;
            chatData.message = message;
            chatData.userColor = UserColor.Console;
            chatData.messageColor = MessageColor.Console;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
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
        
        private static readonly ChatCommand privateMessage = new ChatCommand("/w", 0,
            "Sends a private message to a specific user",
            ChatPrivateMessageCommandAction);

        public static readonly ChatCommand[] chatCommands = new ChatCommand[]
        {
            helpCommand,
            toolsCommand,
            pingCommand,
            disconnectCommand,
            stopOnlineActivityCommand,
            privateMessage
        };

        private static void ChatHelpCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                List<string> messagesToSend = new List<string> { "List of available commands:" };
                foreach (ChatCommand command in chatCommands) messagesToSend.Add($"{command.prefix} - {command.description}");

                foreach (string str in messagesToSend) ChatManager.SendSystemMessage(targetClient, str);
            }
        }

        private static void ChatToolsCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                foreach (string str in ChatManager.defaultTextTools)
                {
                    ChatManager.SendSystemMessage(targetClient, str);
                }
            }
        }

        private static void ChatPingCommandAction()
        {
            if (targetClient == null) return;
            else ChatManager.SendSystemMessage(targetClient, "Pong!");
        }

        private static void ChatDisconnectCommandAction()
        {
            if (targetClient == null) return;
            else targetClient.listener.disconnectFlag = true;
        }

        private static void ChatStopOnlineActivityCommandAction()
        {
            if (targetClient == null) return;
            else OnlineActivityManager.SendVisitStop(targetClient);
        }

        private static void ChatPrivateMessageCommandAction()
        {
            if (targetClient == null) return;
            else
            {
                string message = "";
                for (int i = 2; i < command.Length; i++) message += command[i] + " ";

                if (string.IsNullOrWhiteSpace(message)) ChatManager.SendSystemMessage(targetClient, "Message was empty.");
                else
                {
                    ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(command[1]));
                    if (toFind == null) ChatManager.SendSystemMessage(targetClient, "User was not found.");
                    else
                    {
                        //Don't allow players to send wispers to themselves
                        if (toFind == targetClient) ChatManager.SendSystemMessage(targetClient, "Can't send a whisper to yourself.");
                        else
                        {
                            ChatData chatData = new ChatData();
                            chatData.message = message;
                            chatData.userColor = UserColor.Private;
                            chatData.messageColor = MessageColor.Private;

                            //Send to sender
                            chatData.username = $">> {toFind.userFile.Username}";
                            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
                            targetClient.listener.EnqueuePacket(packet);

                            //Send to recipient
                            chatData.username = $"<< {targetClient.userFile.Username}";
                            packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ChatPacket), chatData);
                            toFind.listener.EnqueuePacket(packet);

                            ChatManagerHelper.ShowChatInConsole(chatData.username, message);
                        }
                    }
                }
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

