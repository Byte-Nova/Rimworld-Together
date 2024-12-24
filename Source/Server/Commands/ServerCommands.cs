using Shared;
using static GameServer.ConsoleCommands;
using static Shared.CommonEnumerators;
using static GameServer.ConsoleCommandActions;

namespace GameServer
{
    public class BaseServerCommand
    {
        public string prefix;

        public string description;

        public int parameters;

        public Action commandAction;

        public BaseServerCommand(string prefix, int parameters, string description, Action commandAction)
        {
            this.prefix = prefix;
            this.parameters = parameters;
            this.description = description;
            this.commandAction = commandAction;
        }
    }

    public static class ConsoleCommands
    {
        private static readonly BaseServerCommand helpCommand = new BaseServerCommand("help", 0,
            "Shows a list of all available commands to use",
            HelpCommandAction);

        public static readonly BaseServerCommand backupCommand = new BaseServerCommand("backup", 0,
            "Backup the server.",
            BackupCommandAction);

        public static readonly BaseServerCommand backupUserCommand = new BaseServerCommand("backupuser", 1,
            "Backup the data of a specific user",
            BackupUserCommandAction);

        public static readonly BaseServerCommand listCommand = new BaseServerCommand("list", 0,
            "Shows all connected players",
            ListCommandAction);

        public static readonly BaseServerCommand opCommand = new BaseServerCommand("op", 1,
            "Gives admin privileges to the selected player",
            OpCommandAction);

        public static readonly BaseServerCommand deopCommand = new BaseServerCommand("deop", 1,
            "Removes admin privileges from the selected player",
            DeopCommandAction);

        public static readonly BaseServerCommand kickCommand = new BaseServerCommand("kick", 1,
            "Kicks the selected player from the server",
            KickCommandAction);

        public static readonly BaseServerCommand banCommand = new BaseServerCommand("ban", 1,
            "Bans the selected player from the server",
            BanCommandAction);

        public static readonly BaseServerCommand pardonCommand = new BaseServerCommand("pardon", 1,
            "Pardons the selected player from the server",
            PardonCommandAction);

        public static readonly BaseServerCommand deepListCommand = new BaseServerCommand("deeplist", 0,
            "Shows a list of all server players",
            DeepListCommandAction);

        public static readonly BaseServerCommand banListCommand = new BaseServerCommand("banlist", 0,
            "Shows a list of all banned server players",
            BanListCommandAction);

        public static readonly BaseServerCommand reloadCommand = new BaseServerCommand("reload", 0,
            "Reloads all server resources",
            ReloadCommandAction);

        public static readonly BaseServerCommand modListCommand = new BaseServerCommand("modlist", 0,
            "Shows all currently loaded mods",
            ModListCommandAction);

        public static readonly BaseServerCommand doSiteRewards = new BaseServerCommand("dositerewards", 0,
            "Forces site rewards to run",
            DoSiteRewardsCommandAction);

        public static readonly BaseServerCommand eventCommand = new BaseServerCommand("event", 2,
            "Sends a command to the selecter players",
            EventCommandAction);

        public static readonly BaseServerCommand eventAllCommand = new BaseServerCommand("eventall", 1,
            "Sends a command to all connected players",
            EventAllCommandAction);

        public static readonly BaseServerCommand eventListCommand = new BaseServerCommand("eventlist", 0,
            "Shows a list of all available events to use",
            EventListCommandAction);

        public static readonly BaseServerCommand broadcastCommand = new BaseServerCommand("broadcast", -1,
            "Broadcast a message to all connected players",
            BroadcastCommandAction);

        public static readonly BaseServerCommand serverMessageCommand = new BaseServerCommand("chat", -1,
            "Send a message in chat from the Server",
            ServerMessageCommandAction);

        public static readonly BaseServerCommand whitelistCommand = new BaseServerCommand("whitelist", 0,
            "Shows all whitelisted players",
            WhitelistCommandAction);

        public static readonly BaseServerCommand whitelistAddCommand = new BaseServerCommand("whitelistadd", 1,
            "Adds a player to the whitelist",
            WhitelistAddCommandAction);

        public static readonly BaseServerCommand whitelistRemoveCommand = new BaseServerCommand("whitelistremove", 1,
            "Removes a player from the whitelist",
            WhitelistRemoveCommandAction);

        public static readonly BaseServerCommand forceSaveCommand = new BaseServerCommand("forcesave", 1,
            "Forces a player to sync their save",
            ForceSaveCommandAction);

        public static readonly BaseServerCommand resetPlayerCommand = new BaseServerCommand("resetplayer", 1,
            "Resets a player profile from the server",
            ResetPlayerCommandAction);

        public static readonly BaseServerCommand portforwardCommand = new BaseServerCommand("portforward", 0,
            "will use UPnP to portforward the server",
            PortForwardCommandAction);

        public static readonly BaseServerCommand setGameSpeedCommand = new BaseServerCommand("setgamespeed", 1,
            "Changes the enforced game speed for all players",
            SetGameSpeedCommandAction);

        public static readonly BaseServerCommand resetWorldCommand = new BaseServerCommand("resetworld", 0,
            "Resets all the world related data and stores a backup of it",
            ResetWorldCommandAction);

        public static readonly BaseServerCommand quitCommand = new BaseServerCommand("quit", 0,
            "Saves all player data and then closes the server",
            QuitCommandAction);

        public static readonly BaseServerCommand forceQuitCommand = new BaseServerCommand("forcequit", 0,
            "Closes the server without saving player data",
            ForceQuitCommandAction);

        public static readonly BaseServerCommand clearCommand = new BaseServerCommand("clear", 0,
            "Clears the console output",
            ClearCommandAction);

        public static readonly BaseServerCommand showModManagerCommand = new BaseServerCommand("showmodmanager", 1,
            "Allows a player to change mod configuration for the server",
            ShowModManagerCommandAction);

        public static readonly BaseServerCommand updateCommand = new BaseServerCommand("update", 0,
            "Updates your server to the newest version. Do not use if you aren't told directly to do so, as it can very well BREAK things",
            UpdateCommandAction);

        public static List<BaseServerCommand> commands = new List<BaseServerCommand>
        {
            backupCommand,
            backupUserCommand,
            banCommand,
            banListCommand,
            broadcastCommand,
            clearCommand,
            deepListCommand,
            deopCommand,
            doSiteRewards,
            eventAllCommand,
            eventCommand,
            setGameSpeedCommand,
            eventListCommand,
            forceQuitCommand,
            forceSaveCommand,
            helpCommand,
            kickCommand,
            listCommand,
            modListCommand,
            opCommand,
            pardonCommand,
            portforwardCommand,
            quitCommand,
            reloadCommand,
            resetPlayerCommand,
            resetWorldCommand,
            serverMessageCommand,
            whitelistAddCommand,
            whitelistCommand,
            whitelistRemoveCommand,
            showModManagerCommand,
            updateCommand
        };
    }

    public static class ConsoleCommandActions
    {
        public static void HelpCommandAction()
        {
            Logger.Title($"List of available commands: [{commands.Count()}]");
            Logger.Title("----------------------------------------");

            foreach (BaseServerCommand command in commands.ToList().OrderBy(fetch => fetch.prefix))
            {
                Logger.Warning($"{command.prefix} - {command.description}");
            }
            Logger.Title("----------------------------------------");
        }

        public static void BackupCommandAction()
        {
            BackupManager.BackupServer();
        }

        public static void BackupUserCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleManager.commandParameters[0]);

            if (userFile == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                Logger.Warning("Do you want this backup to be persistent? (Will not be automatically deleted)");
            DeleteUser:
                Logger.Warning("Please type 'YES' or 'NO'");
                string response = Console.ReadLine();

                if (response == "NO") BackupManager.BackupUser(userFile.Username);
                else if (response == "YES") BackupManager.BackupUser(userFile.Username, true);
                else
                {
                    Logger.Error($"{response} is not a valid option; The options must be capitalized");
                    goto DeleteUser;
                }

            }
        }
        public static void ListCommandAction()
        {
            Logger.Title($"Connected players: [{NetworkHelper.GetConnectedClientsSafe().Count()}]");
            Logger.Title("----------------------------------------");
            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                Logger.Warning($"{client.userFile.Username} - {client.userFile.SavedIP}");
            }
            Logger.Title("----------------------------------------");
        }

        public static void DeepListCommandAction()
        {
            UserFile[] userFiles = UserManagerHelper.GetAllUserFiles();

            Logger.Title($"Server players: [{userFiles.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (UserFile user in userFiles)
            {
                Logger.Warning($"{user.Username} - {user.SavedIP}");
            }
            Logger.Title("----------------------------------------");
        }

        public static void OpCommandAction()
        {
            ServerClient toFind = NetworkHelper.GetConnectedClientFromUsername(ConsoleManager.commandParameters[0]);

            if (toFind == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                if (CheckIfIsAlready(toFind)) return;
                else
                {
                    toFind.userFile.UpdateAdmin(true);

                    CommandData commandData = new CommandData();
                    commandData._commandMode = CommandMode.Op;

                    Packet packet = Packet.CreatePacketFromObject(nameof(ConsoleManager), commandData);
                    toFind.listener.EnqueuePacket(packet);

                    Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' has now admin privileges");
                }
            }

            bool CheckIfIsAlready(ServerClient client)
            {
                if (client.userFile.IsAdmin)
                {
                    Logger.Warning($"User '{client.userFile.Username}' was already an admin");
                    return true;
                }

                else return false;
            }
        }

        public static void DeopCommandAction()
        {
            ServerClient toFind = NetworkHelper.GetConnectedClientFromUsername(ConsoleManager.commandParameters[0]);

            if (toFind == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                if (CheckIfIsAlready(toFind)) return;
                else
                {
                    toFind.userFile.UpdateAdmin(false);

                    CommandData commandData = new CommandData();
                    commandData._commandMode = CommandMode.Deop;

                    Packet packet = Packet.CreatePacketFromObject(nameof(ConsoleManager), commandData);
                    toFind.listener.EnqueuePacket(packet);

                    Logger.Warning($"User '{toFind.userFile.Username}' is no longer an admin");
                }
            }

            bool CheckIfIsAlready(ServerClient client)
            {
                if (!client.userFile.IsAdmin)
                {
                    Logger.Warning($"User '{client.userFile.Username}' was not an admin");
                    return true;
                }

                else return false;
            }
        }

        public static void KickCommandAction()
        {
            ServerClient toFind = NetworkHelper.GetConnectedClientFromUsername(ConsoleManager.commandParameters[0]);

            if (toFind == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                toFind.listener.disconnectFlag = true;

                Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' has been kicked from the server");
            }
        }

        public static void BanListCommandAction()
        {
            List<UserFile> userFiles = UserManagerHelper.GetAllUserFiles().ToList().FindAll(x => x.IsBanned);

            Logger.Title($"Banned players: [{userFiles.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (UserFile user in userFiles) Logger.Warning($"{user.Username} - {user.SavedIP}");
            Logger.Title("----------------------------------------");
        }

        public static void BanCommandAction() { UserManager.BanPlayerFromName(ConsoleManager.commandParameters[0]); }

        public static void PardonCommandAction() { UserManager.PardonPlayerFromName(ConsoleManager.commandParameters[0]); }

        public static void ReloadCommandAction() { Main_.LoadResources(); }

        public static void ModListCommandAction()
        {
            Logger.Title($"Required Mods: [{Master.modConfig.RequiredMods.Length}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.modConfig.RequiredMods) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");

            Logger.Title($"Optional Mods: [{Master.modConfig.OptionalMods.Length}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.modConfig.OptionalMods) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");

            Logger.Title($"Forbidden Mods: [{Master.modConfig.ForbiddenMods.Length}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.modConfig.ForbiddenMods) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");
        }

        public static void SetGameSpeedCommandAction()
        {
            int desiredSpeed = int.Parse(ConsoleManager.commandParameters[0]);
            if (desiredSpeed < 0 || desiredSpeed > 4) Logger.Error("Tried to set invalid game speed, specify 0-4");
            else
            {
                Master.actionValues.EnforcedGameSpeed = int.Parse(ConsoleManager.commandParameters[0]);
                Main_.SaveValueFile(ServerFileMode.Actions);

                Logger.Warning($"Enforced game speed to '{Master.actionValues.EnforcedGameSpeed}'");
            }
        }

        public static void DoSiteRewardsCommandAction()
        {
            Logger.Title($"Forced site rewards");
            SiteManager.SiteRewardTick();
        }

        public static void EventCommandAction()
        {
            ServerClient client = NetworkHelper.GetConnectedClientFromUsername(ConsoleManager.commandParameters[0]);

            if (client == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                EventFile toFind = EventManagerHelper.loadedEvents.FirstOrDefault(fetch => fetch.DefName == ConsoleManager.commandParameters[1]);
                if (toFind == null) Logger.Warning($"Event '{ConsoleManager.commandParameters[1]}' was not found");
                else
                {
                    EventData eventData = new EventData();
                    eventData._stepMode = EventStepMode.Receive;
                    eventData._eventFile = toFind;

                    //We set it to -1 to let the client know it will fall at any settlement
                    eventData._toTile = -1;

                    Packet packet = Packet.CreatePacketFromObject(nameof(EventManager), eventData);
                    client.listener.EnqueuePacket(packet);

                    Logger.Title($"Sent event '{ConsoleManager.commandParameters[1]}' to '{ConsoleManager.commandParameters[0]}'");
                }
            }
        }

        public static void EventAllCommandAction()
        {
            EventFile toFind = EventManagerHelper.loadedEvents.FirstOrDefault(fetch => fetch.DefName == ConsoleManager.commandParameters[0]);
            if (toFind == null) Logger.Warning($"Event '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
                {
                    EventData eventData = new EventData();
                    eventData._stepMode = EventStepMode.Receive;
                    eventData._eventFile = toFind;

                    //We set it to -1 to let the client know it will fall at any settlement
                    eventData._toTile = -1;

                    Packet packet = Packet.CreatePacketFromObject(nameof(EventManager), eventData);
                    client.listener.EnqueuePacket(packet);
                }

                Logger.Title($"Sent event '{ConsoleManager.commandParameters[0]}' to every connected player");
            }
        }

        public static void EventListCommandAction()
        {
            Logger.Title($"Available events: [{EventManagerHelper.loadedEvents.Length}]");
            Logger.Title("----------------------------------------");
            foreach (EventFile eventFile in EventManagerHelper.loadedEvents) Logger.Warning($"{eventFile.DefName}");
            Logger.Title("----------------------------------------");
        }

        public static void BroadcastCommandAction()
        {
            string fullText = "";
            foreach (string str in ConsoleManager.commandParameters) fullText += $"{str} ";
            fullText = fullText.Remove(fullText.Length - 1, 1);

            CommandData commandData = new CommandData();
            commandData._commandMode = CommandMode.Broadcast;
            commandData._details = fullText;

            Packet packet = Packet.CreatePacketFromObject(nameof(ConsoleManager), commandData);
            NetworkHelper.SendPacketToAllClients(packet);

            Logger.Title($"Sent broadcast: '{fullText}'");
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

            Logger.Title($"Sent chat: '{fullText}'");
        }

        public static void WhitelistCommandAction()
        {
            Logger.Title($"Whitelisted usernames: [{Master.whitelist.WhitelistedUsers.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.whitelist.WhitelistedUsers) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");
        }

        public static void WhitelistAddCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleManager.commandParameters[0]);
            if (userFile == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.AddUserToWhitelist(ConsoleManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (Master.whitelist.WhitelistedUsers.Contains(userFile.Username))
                {
                    Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was already whitelisted");
                    return true;
                }

                else return false;
            }
        }

        public static void WhitelistRemoveCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleManager.commandParameters[0]);
            if (userFile == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.RemoveUserFromWhitelist(ConsoleManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!Master.whitelist.WhitelistedUsers.Contains(userFile.Username))
                {
                    Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not whitelisted");
                    return true;
                }

                else return false;
            }
        }

        public static void ForceSaveCommandAction()
        {
            ServerClient toFind = NetworkHelper.GetConnectedClientFromUsername(ConsoleManager.commandParameters[0]);
            if (toFind == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                CommandData commandData = new CommandData();
                commandData._commandMode = CommandMode.ForceSave;

                Packet packet = Packet.CreatePacketFromObject(nameof(ConsoleManager), commandData);
                toFind.listener.EnqueuePacket(packet);

                Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' has been forced to save");
            }
        }

        public static void ResetPlayerCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleManager.commandParameters[0]);
            ServerClient toFind = NetworkHelper.GetConnectedClientFromUsername(userFile.Username);

            if (userFile == null) Logger.Warning($"User '{ConsoleManager.commandParameters[0]}' was not found");
            else SaveManager.ResetPlayerData(toFind, userFile.Username);
        }

        public static void PortForwardCommandAction()
        {
            if (!Master.serverConfig.UseUPnP) Logger.Error("Cannot portforward because UPnP is disabled on the server");
            else _ = new UPnP();
        }

        public static void ResetWorldCommandAction()
        {
            //Make sure the user wants to reset the world
            Logger.Warning("Are you sure you want to reset the world?");
            Logger.Warning("Please type 'YES' or 'NO'");

        DeleteWorldQuestion:
            string response = Console.ReadLine();

            if (response == "NO") return;
            else if (response != "YES")
            {
                Logger.Error($"{response} is not a valid option. The answer must be capitalized");
                goto DeleteWorldQuestion;
            }

            BackupManager.BackupServer();

            Directory.Delete($"{Master.caravansPath}", true);
            Directory.Delete($"{Master.corePath}", true);
            Directory.Delete($"{Master.eventsPath}", true);
            Directory.Delete($"{Master.factionsPath}", true);
            Directory.Delete($"{Master.logsPath}", true);
            Directory.Delete($"{Master.mapsPath}", true);
            Directory.Delete($"{Master.savesPath}", true);
            Directory.Delete($"{Master.settlementsPath}", true);
            Directory.Delete($"{Master.sitesPath}", true);
            Directory.Delete($"{Master.usersPath}", true);

            Environment.Exit(0);
        }

        public static void QuitCommandAction()
        {
            Master.isClosing = true;

            Logger.Warning($"Waiting for all saves to quit");

            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                CommandData commandData = new CommandData();
                commandData._commandMode = CommandMode.ForceSave;

                Packet packet = Packet.CreatePacketFromObject(nameof(ConsoleManager), commandData);
                client.listener.EnqueuePacket(packet);
            }

            while (NetworkHelper.GetConnectedClientsSafe().Length > 0) Thread.Sleep(1);

            Environment.Exit(0);
        }

        public static void ForceQuitCommandAction() { Environment.Exit(0); }

        public static void ClearCommandAction()
        {
            Console.Clear();

            Logger.Title("[Cleared console]");
        }

        public static void ShowModManagerCommandAction()
        {
            ServerClient toFind = NetworkHelper.GetConnectedClientFromUsername(ConsoleManager.commandParameters[0]);
            if (toFind == null) Logger.Error($"Player '{ConsoleManager.commandParameters[0]}' was not found");
            else
            {
                if (!toFind.userFile.IsAdmin) Logger.Error($"Player '{ConsoleManager.commandParameters[0]}' needs to be an operator");
                else
                {
                    ModConfigData data = new ModConfigData();
                    data._stepMode = ModConfigStepMode.Ask;
                    data._configFile = Master.modConfig;

                    Packet packet = Packet.CreatePacketFromObject(nameof(ModManager), data);
                    toFind.listener.EnqueuePacket(packet);

                    Logger.Warning("Command sent sucessfully");
                }
            }
        }

        public static void UpdateCommandAction()
        {
            Logger.Warning("Are you sure you want to run the update command? You should only do so if you are told to, as this may break things.");
            Logger.Warning("Please type 'YES' or 'NO'");

        UpdateCommandQuestion:
            string response = Console.ReadLine();

            if (response == "NO") return;
            else if (response == "YES") Updater.UpdateManager.UpdateServer();
            else
            {
                Logger.Error($"{response} is not a valid option; The options must be capitalized");
                goto UpdateCommandQuestion;
            }
        }
    }
}