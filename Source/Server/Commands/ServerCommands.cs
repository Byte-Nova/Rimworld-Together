using Shared;
using static Shared.CommonEnumerators;
using static GameServer.Commands.ConsoleCommandActions;
using GameServer.Core;
using GameServer.Files;
using GameServer.Managers;
using GameServer.Misc;
using Shared.Files;
using TCPNetwork.Server;
using TCPNetwork.Packets;

namespace GameServer.Commands
{
    public static class ConsoleCommands
    {
        private static readonly CommandBase HelpCommand = new CommandBase("help", 0,
            "Shows a list of all available commands to use",
            HelpCommandAction);

        public static readonly CommandBase BackupCommand = new CommandBase("backup", 0,
            "Backup the server.",
            BackupCommandAction);

        public static readonly CommandBase BackupUserCommand = new CommandBase("backupuser", 1,
            "Backup the data of a specific user",
            BackupUserCommandAction);

        public static readonly CommandBase ListCommand = new CommandBase("list", 0,
            "Shows all connected players",
            ListCommandAction);

        public static readonly CommandBase OpCommand = new CommandBase("op", 1,
            "Gives admin privileges to the selected player",
            OpCommandAction);

        public static readonly CommandBase DeopCommand = new CommandBase("deop", 1,
            "Removes admin privileges from the selected player",
            DeopCommandAction);

        public static readonly CommandBase KickCommand = new CommandBase("kick", 1,
            "Kicks the selected player from the server",
            KickCommandAction);

        public static readonly CommandBase BanCommand = new CommandBase("ban", 1,
            "Bans the selected player from the server",
            BanCommandAction);

        public static readonly CommandBase PardonCommand = new CommandBase("pardon", 1,
            "Pardons the selected player from the server",
            PardonCommandAction);

        public static readonly CommandBase DeepListCommand = new CommandBase("deeplist", 0,
            "Shows a list of all server players",
            DeepListCommandAction);

        public static readonly CommandBase BanListCommand = new CommandBase("banlist", 0,
            "Shows a list of all banned server players",
            BanListCommandAction);

        public static readonly CommandBase ReloadCommand = new CommandBase("reload", 0,
            "Reloads all server resources",
            ReloadCommandAction);

        public static readonly CommandBase ModListCommand = new CommandBase("modlist", 0,
            "Shows all currently loaded mods",
            ModListCommandAction);

        public static readonly CommandBase DoSiteRewards = new CommandBase("dositerewards", 0,
            "Forces site rewards to run",
            DoSiteRewardsCommandAction);

        public static readonly CommandBase EventCommand = new CommandBase("event", 2,
            "Sends a command to the selecter players",
            EventCommandAction);

        public static readonly CommandBase EventAllCommand = new CommandBase("eventall", 1,
            "Sends a command to all connected players",
            EventAllCommandAction);

        public static readonly CommandBase EventListCommand = new CommandBase("eventlist", 0,
            "Shows a list of all available events to use",
            EventListCommandAction);

        public static readonly CommandBase BroadcastCommand = new CommandBase("broadcast", -1,
            "Broadcast a message to all connected players",
            BroadcastCommandAction);

        public static readonly CommandBase ServerMessageCommand = new CommandBase("chat", -1,
            "Send a message in chat from the Server",
            ServerMessageCommandAction);

        public static readonly CommandBase WhitelistCommand = new CommandBase("whitelist", 0,
            "Shows all whitelisted players",
            WhitelistCommandAction);

        public static readonly CommandBase WhitelistAddCommand = new CommandBase("whitelistadd", 1,
            "Adds a player to the whitelist",
            WhitelistAddCommandAction);

        public static readonly CommandBase WhitelistRemoveCommand = new CommandBase("whitelistremove", 1,
            "Removes a player from the whitelist",
            WhitelistRemoveCommandAction);

        public static readonly CommandBase ForceSaveCommand = new CommandBase("forcesave", 1,
            "Forces a player to sync their save",
            ForceSaveCommandAction);

        public static readonly CommandBase ResetPlayerCommand = new CommandBase("resetplayer", 1,
            "Resets a player profile from the server",
            ResetPlayerCommandAction);

        public static readonly CommandBase PortforwardCommand = new CommandBase("portforward", 0,
            "will use UPnP to portforward the server",
            PortForwardCommandAction);

        public static readonly CommandBase ResetWorldCommand = new CommandBase("resetworld", 0,
            "Resets all the world related data and stores a backup of it",
            ResetWorldCommandAction);

        public static readonly CommandBase QuitCommand = new CommandBase("quit", 0,
            "Saves all player data and then closes the server",
            QuitCommandAction);

        public static readonly CommandBase ForceQuitCommand = new CommandBase("forcequit", 0,
            "Closes the server without saving player data",
            ForceQuitCommandAction);

        public static readonly CommandBase ClearCommand = new CommandBase("clear", 0,
            "Clears the console output",
            ClearCommandAction);

        public static readonly CommandBase DebugGCClear = new CommandBase("debuggcclear", 0,
            "Forces the garbage collector to collect",
            ForceGCClearCommandAction);
        public static List<CommandBase> Commands = new List<CommandBase>
        {
            BackupCommand,
            BackupUserCommand,
            BanCommand,
            BanListCommand,
            BroadcastCommand,
            ClearCommand,
            DeepListCommand,
            DeopCommand,
            DoSiteRewards,
            EventAllCommand,
            EventCommand,
            EventListCommand,
            ForceQuitCommand,
            ForceSaveCommand,
            HelpCommand,
            KickCommand,
            ListCommand,
            ModListCommand,
            OpCommand,
            PardonCommand,
            PortforwardCommand,
            QuitCommand,
            ReloadCommand,
            ResetPlayerCommand,
            ResetWorldCommand,
            ServerMessageCommand,
            WhitelistAddCommand,
            WhitelistCommand,
            WhitelistRemoveCommand,
            DebugGCClear
        };
    }

    public static class ConsoleCommandActions
    {
        public static void HelpCommandAction()
        {
            Printer.Title($"List of available commands: [{ConsoleCommands.Commands.Count()}]");
            Printer.Title("----------------------------------------");

            foreach (CommandBase command in ConsoleCommands.Commands.ToList().OrderBy(fetch => fetch.Prefix))
            {
                Printer.Warning($"{command.Prefix} - {command.Description}");
            }
            Printer.Title("----------------------------------------");
        }

        public static void BackupCommandAction()
        {
            BackupManager.BackupServer();
        }

        public static void BackupUserCommandAction()
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(ConsoleManager.commandParameters[0]);

            if (userFile == null) ThrowUserNotFoundError();
            else
            {
                Printer.Warning("Do you want this backup to be persistent? (Will not be automatically deleted)");
            DeleteUser:
                Printer.Warning("Please type 'YES' or 'NO'");
                string response = Console.ReadLine();

                if (response == "NO") BackupManager.BackupUser(userFile.Uid);
                else if (response == "YES") BackupManager.BackupUser(userFile.Uid, true);
                else
                {
                    Printer.Error($"{response} is not a valid option; The options must be capitalized");
                    goto DeleteUser;
                }

            }
        }
        public static void ListCommandAction()
        {
            Printer.Title($"Connected players: [{ServerNetwork.Instance.GetConnectedClientsSafe().Count()}]");
            Printer.Title("----------------------------------------");
            foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe())
            {
                Printer.Warning($"{client.UserFile.SavedIP} - {client.UserFile.Label} - {client.UserFile.Uid}");
            }
            Printer.Title("----------------------------------------");
        }

        public static void DeepListCommandAction()
        {
            UserFile[] userFiles = UserManagerH.GetAllUserFiles();

            Printer.Title($"Server players: [{userFiles.Count()}]");
            Printer.Title("----------------------------------------");
            foreach (UserFile user in userFiles)
            {
                Printer.Warning($"{user.Label} - {user.Uid}");
            }
            Printer.Title("----------------------------------------");
        }

        public static void OpCommandAction()
        {
            UserFile toFind = UserManagerH.GetAllUserFiles().Where(x => x.Uid == ConsoleManager.commandParameters[0]).FirstOrDefault();
            if (toFind == null) 
            {
                ThrowUserNotFoundError();
                return;
            }

            if (CheckIfIsAlready(toFind)) return;

            toFind.UpdateAdmin(true);

            ServerClient client = ServerNetwork.Instance.GetConnectedClientFromUid(toFind.Uid);
            if (client != null)
            {
                CommandData commandData = new CommandData();
                commandData._commandMode = CommandMode.Op;

                client.UserFile.UpdateAdmin(true);
                client.Listener.EnqueuePacket(PacketHeader.ConsoleManager, commandData);
            }
            UserManagerH.SaveUserFile(toFind);

            Printer.Warning($"User '{toFind.Label}' has now admin privileges");
            bool CheckIfIsAlready(UserFile userFile)
            {
                if (userFile.IsAdmin)
                {
                    Printer.Warning($"User '{userFile.Label}' was already an admin");
                    return true;
                }

                else return false;
            }
        }

        public static void DeopCommandAction()
        {
            UserFile toFind = UserManagerH.GetAllUserFiles().Where(x => x.Uid == ConsoleManager.commandParameters[0]).FirstOrDefault();

            if (toFind == null)
            {
                ThrowUserNotFoundError();
                return;
            }

            if (CheckIfIsAlready(toFind)) return;

            toFind.UpdateAdmin(false);
            ServerClient client = ServerNetwork.Instance.GetConnectedClientFromUid(toFind.Uid);
            if (client != null)
            {
                CommandData commandData = new CommandData();
                commandData._commandMode = CommandMode.Deop;

                client.UserFile.UpdateAdmin(false);
                client.Listener.EnqueuePacket(PacketHeader.ConsoleManager, commandData);
            }
            UserManagerH.SaveUserFile(toFind);

            Printer.Warning($"User '{toFind.Label}' is no longer an admin");

            bool CheckIfIsAlready(UserFile client)
            {
                if (!client.IsAdmin)
                {
                    Printer.Warning($"User '{client.Label}' was not an admin");
                    return true;
                }

                else return false;
            }
        }

        public static void KickCommandAction()
        {
            ServerClient toFind = ServerNetwork.Instance.GetConnectedClientFromUid(ConsoleManager.commandParameters[0]);

            if (toFind == null)
            {
                ThrowUserNotFoundError();
                return;
            }
            toFind.Listener.DisconnectFlag = true;

            Printer.Warning($"User '{toFind.UserFile.Label}' has been kicked from the server");
        }

        public static void BanListCommandAction()
        {
            List<UserFile> userFiles = UserManagerH.GetAllUserFiles().ToList().FindAll(x => x.IsBanned);

            Printer.Title($"Banned players: [{userFiles.Count()}]");
            Printer.Title("----------------------------------------");
            foreach (UserFile user in userFiles) Printer.Warning($"{user.Label} - {user.SavedIP}");
            Printer.Title("----------------------------------------");
        }

        public static void BanCommandAction() { UserManager.BanPlayerFromName(ConsoleManager.commandParameters[0]); }

        public static void PardonCommandAction() { UserManager.PardonPlayerFromName(ConsoleManager.commandParameters[0]); }

        public static void ReloadCommandAction() { Main_.LoadResources(); }

        public static void ModListCommandAction()
        {
            Printer.Title($"Required Mods: [{Master.ModConfig.RequiredMods.Length}]");
            Printer.Title("----------------------------------------");
            foreach (string str in Master.ModConfig.RequiredMods) Printer.Warning($"{str}");
            Printer.Title("----------------------------------------");

            Printer.Title($"Optional Mods: [{Master.ModConfig.OptionalMods.Length}]");
            Printer.Title("----------------------------------------");
            foreach (string str in Master.ModConfig.OptionalMods) Printer.Warning($"{str}");
            Printer.Title("----------------------------------------");

            Printer.Title($"Forbidden Mods: [{Master.ModConfig.ForbiddenMods.Length}]");
            Printer.Title("----------------------------------------");
            foreach (string str in Master.ModConfig.ForbiddenMods) Printer.Warning($"{str}");
            Printer.Title("----------------------------------------");
        }

        public static void DoSiteRewardsCommandAction()
        {
            Printer.Title($"Forced site rewards");
            SiteManager.SiteRewardTick();
        }

        public static void EventCommandAction()
        {
            ServerClient client = ServerNetwork.Instance.GetConnectedClientFromUid(ConsoleManager.commandParameters[0]);

            if (client == null) Printer.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                EventFile toFind = EventManagerH.LoadedEvents.FirstOrDefault(fetch => fetch.DefName == ConsoleManager.commandParameters[1]);
                if (toFind == null) Printer.Warning($"Event '{ConsoleManager.commandParameters[1]}' was not found");
                else
                {
                    EventData eventData = new EventData();
                    eventData._stepMode = EventStepMode.Receive;
                    eventData._eventFile = toFind;

                    //We set it to -1 to let the client know it will fall at any settlement
                    eventData._toTile = -1;

                    client.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);

                    Printer.Title($"Sent event '{ConsoleManager.commandParameters[1]}' to '{ConsoleManager.commandParameters[0]}'");
                }
            }
        }

        public static void EventAllCommandAction()
        {
            EventFile toFind = EventManagerH.LoadedEvents.FirstOrDefault(fetch => fetch.DefName == ConsoleManager.commandParameters[0]);
            if (toFind == null) Printer.Warning($"Event '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe())
                {
                    EventData eventData = new EventData();
                    eventData._stepMode = EventStepMode.Receive;
                    eventData._eventFile = toFind;

                    //We set it to -1 to let the client know it will fall at any settlement
                    eventData._toTile = -1;

                    client.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);
                }

                Printer.Title($"Sent event '{ConsoleManager.commandParameters[0]}' to every connected player");
            }
        }

        public static void EventListCommandAction()
        {
            Printer.Title($"Available events: [{EventManagerH.LoadedEvents.Length}]");
            Printer.Title("----------------------------------------");
            foreach (EventFile eventFile in EventManagerH.LoadedEvents) Printer.Warning($"{eventFile.DefName}");
            Printer.Title("----------------------------------------");
        }

        public static void BroadcastCommandAction()
        {
            string fullText = "";
            foreach (string str in ConsoleManager.commandParameters) fullText += $"{str} ";
            fullText = fullText.Remove(fullText.Length - 1, 1);

            CommandData commandData = new CommandData();
            commandData._commandMode = CommandMode.Broadcast;
            commandData._details = fullText;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.ConsoleManager, commandData);

            Printer.Title($"Sent broadcast: '{fullText}'");
        }

        public static void ServerMessageCommandAction()
        {
            string fullText = "";
            foreach (string str in ConsoleManager.commandParameters)
            {
                fullText += $"{str} ";
            }
            fullText = fullText.Remove(fullText.Length - 1, 1);

            ChatManager.BroadcastConsoleMessage(fullText);

            Printer.Title($"Sent chat: '{fullText}'");
        }

        public static void WhitelistCommandAction()
        {
            Printer.Title($"Whitelisted usernames: [{Master.Whitelist.WhitelistedUsers.Count()}]");
            Printer.Title("----------------------------------------");
            foreach (string str in Master.Whitelist.WhitelistedUsers) Printer.Warning($"{str}");
            Printer.Title("----------------------------------------");
        }

        public static void WhitelistAddCommandAction()
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(ConsoleManager.commandParameters[0]);
            if (userFile == null) ThrowUserNotFoundError();
            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.AddUserToWhitelist(ConsoleManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (Master.Whitelist.WhitelistedUsers.Contains(userFile.Uid))
                {
                    Printer.Warning($"User '{ConsoleManager.commandParameters[0]}' was already whitelisted");
                    return true;
                }

                else return false;
            }
        }

        public static void WhitelistRemoveCommandAction()
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(ConsoleManager.commandParameters[0]);
            if (userFile == null) ThrowUserNotFoundError();

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.RemoveUserFromWhitelist(ConsoleManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!Master.Whitelist.WhitelistedUsers.Contains(userFile.Uid))
                {
                    Printer.Warning($"User '{ConsoleManager.commandParameters[0]}' was not whitelisted");
                    return true;
                }

                else return false;
            }
        }

        public static void ForceSaveCommandAction()
        {
            ServerClient toFind = ServerNetwork.Instance.GetConnectedClientFromUid(ConsoleManager.commandParameters[0]);
            if (toFind == null) ThrowUserNotFoundError();
            else
            {
                CommandData commandData = new CommandData();
                commandData._commandMode = CommandMode.ForceSave;

                toFind.Listener.EnqueuePacket(PacketHeader.ConsoleManager, commandData);

                Printer.Warning($"User '{ConsoleManager.commandParameters[0]}' has been forced to save");
            }
        }

        public static void ResetPlayerCommandAction()
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(ConsoleManager.commandParameters[0]);
            if (userFile == null) ThrowUserNotFoundError();
            else
            {
                ServerClient toFind = ServerNetwork.Instance.GetConnectedClientFromUid(userFile.Uid);
                SaveManager.ResetPlayerData(toFind, userFile.Uid);
            }
        }

        public static void PortForwardCommandAction()
        {
            if (!Master.ServerConfig.UseUPnP) Printer.Error("Cannot portforward because UPnP is disabled on the server");
            else _ = new UPnP();
        }

        public static void ResetWorldCommandAction()
        {
            //Make sure the user wants to reset the world
            Printer.Warning("Are you sure you want to reset the world?");
            Printer.Warning("Please type 'YES' or 'NO'");

        DeleteWorldQuestion:
            string response = Console.ReadLine();

            if (response == "NO") return;
            else if (response != "YES")
            {
                Printer.Error($"{response} is not a valid option. The answer must be capitalized");
                goto DeleteWorldQuestion;
            }

            BackupManager.BackupServer();

            Directory.Delete(Master.AssetsPath, true);
            Directory.Delete(Master.ConfigsPath, true);
            Directory.Delete(Master.TempPath, true);

            Environment.Exit(0);
        }

        public static void QuitCommandAction()
        {
            Master.IsClosing = true;

            Printer.Warning($"Waiting for all saves to quit");

            foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe())
            {
                CommandData commandData = new CommandData();
                commandData._commandMode = CommandMode.ForceSave;

                client.Listener.EnqueuePacket(PacketHeader.ConsoleManager, commandData);
            }

            while (ServerNetwork.Instance.GetConnectedClientsSafe().Length > 0) Thread.Sleep(1);

            Environment.Exit(0);
        }

        public static void ForceQuitCommandAction() { Environment.Exit(0); }

        public static void ClearCommandAction()
        {
            Console.Clear();

            Printer.Title("[Cleared console]");
        }

        public static void ForceGCClearCommandAction()
        {
            GC.Collect();
            Printer.Warning($"Currently reporting {GC.GetTotalMemory(false)}");
        }
        public static void ThrowUserNotFoundError()
        {
            Printer.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            UserFile[] allUsers = UserManagerH.GetAllUserFiles();
            if (allUsers.Any(u => u.Label == ConsoleManager.commandParameters[0]))
                Printer.Warning($"Username detected. You can only use UIDs for user commands. " +
                    $"Use the command `deeplist` to get the UID of {ConsoleManager.commandParameters[0]}.");
            UserFile[] usersWithMatchingUsername = allUsers.Where(u => u.Label == ConsoleManager.commandParameters[0]).ToArray();
            if (usersWithMatchingUsername.Length == 1)
            {
                Printer.Warning($"Since only one person with the username {ConsoleManager.commandParameters[0]} exists, " +
                    $"we were able to fetch his UID automatically: {usersWithMatchingUsername.First().Uid}");
            }
        }
    }
}