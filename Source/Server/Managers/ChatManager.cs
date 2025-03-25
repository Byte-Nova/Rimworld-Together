using GameServer.Commands;
using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using System.Text;
using static Shared.CommonEnumerators;
using System.Linq;
using System.Threading.Tasks; // for Task.Run

namespace GameServer.Managers
{
    [RTManager]
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
            "Use '/help' to check all the available commands."
        };

        public static readonly string[] defaultTextTools = new string[]
        {
            "List of available text tools:",
            "'b' inside brackets - Followed by the text you want to turn [b]bold",
            "'i' inside brackets - Followed by the text you want to turn [i]cursive",
            "HTML color inside brackets - Followed by the text you want to [ff0000]change color"
        };

        private static void ParsePacket(ServerClient client, Packet packet)
        {
            ChatData chatData = Serializer.ConvertBytesToObject<ChatData>(packet.Contents);
            if (chatData._message.StartsWith("/"))
            {
                ExecuteChatCommand(client, chatData._message.Split(' '));
            }
            else
            {
                BroadcastChatMessage(client, chatData._message);
            }
        }

        private static void ExecuteChatCommand(ServerClient client, string[] command)
        {
            commandSemaphore.WaitOne();

            BaseChatCommand toFind = ChatManagerHelper.GetCommandFromName(command[0]);
            if (toFind == null)
            {
                SendConsoleMessage(client, "Command was not found.");
            }
            else
            {
                ChatCommandActions.targetClient = client;
                ChatCommandActions.command = command;
                toFind.commandAction.Invoke();
            }
            string chatCommand = string.Join(" ", command);
            ChatManagerHelper.ShowChatInConsole(client.userFile.Label, chatCommand);
            commandSemaphore.Release();
        }

        private static void BroadcastChatMessage(ServerClient client, string message)
        {
            if (Master.serverConfig == null) return;
            var chatData = new ChatData
            {
                _username = client.userFile.Label,
                _message = message,
                _usernameColor = client.userFile.IsAdmin ? UserColor.Admin : UserColor.Normal,
                _messageColor = client.userFile.IsAdmin ? MessageColor.Admin : MessageColor.Normal
            };
            Packet packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);
            WriteToLogs(client.userFile.Label, message);
            ChatManagerHelper.ShowChatInConsole(client.userFile.Label, message);
            if (Master.discordConfig != null && Master.discordConfig.Enabled)
            {
                Task.Run(async () =>
                {
                    await Managers.External.DiscordManager.SendMessageToDiscordChat(chatData._username, message);
                });
            }
        }

        public static void BroadcastDiscordMessage(string client, string message)
        {
            var chatData = new ChatData
            {
                _username = client,
                _message = message,
                _usernameColor = UserColor.Discord,
                _messageColor = MessageColor.Discord
            };
            Packet packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);
            WriteToLogs(client, message);
            ChatManagerHelper.ShowChatInConsole(client, message, true);
        }

        public static void BroadcastConsoleMessage(string message)
        {
            var chatData = new ChatData
            {
                _username = systemName,
                _message = message,
                _usernameColor = UserColor.Console,
                _messageColor = MessageColor.Console
            };
            Packet packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);
            WriteToLogs(chatData._username, message);
            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
        }

        public static void BroadcastServerNotification(string message)
        {
            var chatData = new ChatData
            {
                _username = notificationName,
                _message = message,
                _usernameColor = UserColor.Server,
                _messageColor = MessageColor.Server
            };
            Packet packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            NetworkHelper.SendPacketToAllClients(packet);
            WriteToLogs(chatData._username, message);
            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
        }

        public static void SendConsoleMessage(ServerClient client, string message)
        {
            var chatData = new ChatData
            {
                _username = systemName,
                _message = message,
                _usernameColor = UserColor.Console,
                _messageColor = MessageColor.Console
            };
            Packet packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendServerMessage(ServerClient client, string message)
        {
            var chatData = new ChatData
            {
                _username = notificationName,
                _message = message,
                _usernameColor = UserColor.Server,
                _messageColor = MessageColor.Server
            };
            Packet packet = Packet.CreateFromObject(nameof(ChatManager), chatData);
            client.listener.EnqueuePacket(packet);
        }

        private static void WriteToLogs(string username, string message)
        {
            logSemaphore.WaitOne();
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{DateTime.Now:HH:mm:ss}] | [{username}]: {message}");
            sb.Append(Environment.NewLine);
            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = $"{dateTime.Year}-{dateTime.Month:D2}-{dateTime.Day:D2}";
            string nowFullPath = Master.chatLogsPath + System.IO.Path.DirectorySeparatorChar + nowFileName + ".txt";
            File.AppendAllText(nowFullPath, sb.ToString());
            sb.Clear();
            logSemaphore.Release();
        }
    }

    public static class ChatManagerHelper
    {
        public static ServerClient GetUserFromName(string username)
        {
            return NetworkHelper.GetConnectedClientFromUid(username);
        }

        public static BaseChatCommand GetCommandFromName(string commandName)
        {
            return ChatCommands.commands.FirstOrDefault(x => x.prefix == commandName);
        }

        public static string GetUsernameFromMention(string mention)
        {
            return mention.Replace("@", "");
        }

        public static void ShowChatInConsole(string username, string message, bool fromDiscord = false)
        {
            if (!Master.serverConfig.DisplayChatInConsole) return;
            if (fromDiscord)
                Printer.Message($"[Discord] > {username} > {message}");
            else
                InformationDisplayer.DisplayChatMap(username, message);
        }
    }
}