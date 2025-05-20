using Shared;
using static Shared.CommonEnumerators;
using static GameServer.Commands.ChatCommandActions;
using static GameServer.Commands.ChatCommands;
using GameServer.Managers;
using GameServer.TCP;
using System;
using System.Collections.Generic;

namespace GameServer.Commands
{
    public static class ChatCommands
    {
        private static readonly CommandBase HelpCommand = new(
            "/help", 0, "Shows a list of all available commands", HelpCommandAction);

        private static readonly CommandBase ToolsCommand = new(
            "/tools", 0, "Shows a list of all available chat tools", ToolsCommandAction);

        private static readonly CommandBase PingCommand = new(
            "/ping", 0, "Checks if the connection to the server is working", PingCommandAction);

        private static readonly CommandBase DisconnectCommand = new(
            "/dc", 0, "Forcefully disconnects you from the server", DisconnectCommandAction);

        private static readonly CommandBase PMCommand = new(
            "/w", 0, "Sends a private message to a specific user", PrivateMessageCommandAction);

        // identifier expected by the rest of the codebase
        public static readonly CommandBase[] commands =
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
        public static ServerClient? TargetClient { get; set; }
        public static string[]?     Command      { get; set; }

        public static void HelpCommandAction()
        {
            if (TargetClient == null) return;

            var list = new List<string> { "List of available commands:" };
            foreach (var cmd in ChatCommands.commands)
                list.Add($"{cmd.Prefix} - {cmd.Description}");

            foreach (string line in list)
                ChatManager.SendConsoleMessage(TargetClient, line);
        }

        public static void ToolsCommandAction()
        {
            if (TargetClient == null) return;
            foreach (string s in ChatManager.defaultTextTools)
                ChatManager.SendConsoleMessage(TargetClient, s);
        }

        public static void PingCommandAction()
        {
            if (TargetClient == null) return;
            ChatManager.SendConsoleMessage(TargetClient, "Pong!");
        }

        public static void DisconnectCommandAction()
        {
            if (TargetClient == null) return;
            TargetClient.Listener.DisconnectFlag = true;
        }

        public static void PrivateMessageCommandAction()
        {
            if (TargetClient == null || Command == null || Command.Length < 3) return;

            // build whisper text
            string msg = string.Join(' ', Command, 2, Command.Length - 2);
            if (string.IsNullOrWhiteSpace(msg))
            {
                ChatManager.SendConsoleMessage(TargetClient, "Message was empty.");
                return;
            }

            var recipient = ChatManagerHelper.GetUserFromName(
                            ChatManagerHelper.GetUsernameFromMention(Command[1]));
            if (recipient == null)
            {
                ChatManager.SendConsoleMessage(TargetClient, "User was not found.");
                return;
            }

            if (recipient == TargetClient)
            {
                ChatManager.SendConsoleMessage(TargetClient, "Can't send a whisper to yourself.");
                return;
            }

            var data = new ChatData
            {
                _message       = msg,
                _usernameColor = UserColor.Private,
                _messageColor  = MessageColor.Private
            };

            // to sender
            data._username = $">> {recipient.UserFile.Label}";
            TargetClient.Listener.EnqueuePacket(PacketHeader.ChatManager, data);

            // to recipient
            data._username = $"<< {TargetClient.UserFile.Label}";
            recipient.Listener.EnqueuePacket(PacketHeader.ChatManager, data);

            ChatManagerHelper.ShowChatInConsole(data._username, msg);
        }
    }
}