using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CommandStorage
    {
        private static readonly ServerCommand helpCommand = new ServerCommand("help", 0,
            "Shows a list of all available commands to use",
            HelpCommandAction);

        private static readonly ServerCommand listCommand = new ServerCommand("list", 0,
            "Shows all connected players",
            ListCommandAction);

        private static readonly ServerCommand opCommand = new ServerCommand("op", 1,
            "Gives admin privileges to the selected player",
            OpCommandAction);

        private static readonly ServerCommand deopCommand = new ServerCommand("deop", 1,
            "Removes admin privileges from the selected player",
            DeopCommandAction);

        private static readonly ServerCommand kickCommand = new ServerCommand("kick", 1,
            "Kicks the selected player from the server",
            KickCommandAction);

        private static readonly ServerCommand banCommand = new ServerCommand("ban", 1,
            "Bans the selected player from the server",
            BanCommandAction);

        private static readonly ServerCommand pardonCommand = new ServerCommand("pardon", 1,
            "Pardons the selected player from the server",
            PardonCommandAction);

        private static readonly ServerCommand deepListCommand = new ServerCommand("deeplist", 0,
            "Shows a list of all server players",
            DeepListCommandAction);

        private static readonly ServerCommand banListCommand = new ServerCommand("banlist", 0,
            "Shows a list of all banned server players",
            BanListCommandAction);

        private static readonly ServerCommand reloadCommand = new ServerCommand("reload", 0,
            "Reloads all server resources",
            ReloadCommandAction);

        private static readonly ServerCommand modListCommand = new ServerCommand("modlist", 0,
            "Shows all currently loaded mods",
            ModListCommandAction);

        private static  readonly ServerCommand doSiteRewards = new ServerCommand("dositerewards", 0,
            "Forces site rewards to run",
            DoSiteRewardsCommandAction);

        private static readonly ServerCommand eventCommand = new ServerCommand("event", 2,
            "Sends a command to the selecter players",
            EventCommandAction);

        private static readonly ServerCommand eventAllCommand = new ServerCommand("eventall", 1,
            "Sends a command to all connected players",
            EventAllCommandAction);

        private static readonly ServerCommand eventListCommand = new ServerCommand("eventlist", 0,
            "Shows a list of all available events to use",
            EventListCommandAction);

        private static readonly ServerCommand broadcastCommand = new ServerCommand("broadcast", -1,
            "Broadcast a message to all connected players",
            BroadcastCommandAction);

        private static readonly ServerCommand serverMessageCommand = new ServerCommand("chat", -1,
            "Send a message in chat from the Server",
            ServerMessageCommandAction);

        private static readonly ServerCommand whitelistCommand = new ServerCommand("whitelist", 0,
            "Shows all whitelisted players",
            WhitelistCommandAction);

        private static readonly ServerCommand whitelistAddCommand = new ServerCommand("whitelistadd", 1,
            "Adds a player to the whitelist",
            WhitelistAddCommandAction);

        private static readonly ServerCommand whitelistRemoveCommand = new ServerCommand("whitelistremove", 1,
            "Removes a player from the whitelist",
            WhitelistRemoveCommandAction);

        private static readonly ServerCommand whitelistToggleCommand = new ServerCommand("togglewhitelist", 0,
            "Toggles the whitelist ON or OFF",
            WhitelistToggleCommandAction);

        private static readonly ServerCommand forceSaveCommand = new ServerCommand("forcesave", 1,
            "Forces a player to sync their save",
            ForceSaveCommandAction);

        private static readonly ServerCommand resetPlayerCommand = new ServerCommand("resetplayer", 1,
            "Resets a player profile from the server",
            ResetPlayerCommandAction);

        private static readonly ServerCommand toggleDifficultyCommand = new ServerCommand("toggledifficulty", 0,
            "Enables custom difficulty in the server",
            ToggleDifficultyCommandAction);

        private static readonly ServerCommand toggleCustomScenariosCommand = new ServerCommand("togglecustomscenarios", 0,
            "enables/disables custom scenarios on the server",
            ToggleCustomScenariosCommandAction);

        private static readonly ServerCommand toggleDiscordPresenceCommand = new ServerCommand("togglediscordpresence", 0,
            "enables/disables Discord pressence on the server",
            ToggleDiscordPressenceCommandAction);

        private static readonly ServerCommand toggleUPnPCommand = new ServerCommand("toggleupnp", 0,
            "enables/disables UPnP port mapping (auto-portforwarding)",
            ToggleUPnPCommandAction);

        private static readonly ServerCommand portforwardCommand = new ServerCommand("portforward", 0,
            "will use UPnP to portforward the server",
            PortForwardCommandAction);

        private static readonly ServerCommand toggleVerboseLogsCommand = new ServerCommand("toggleverboselogs", 0,
            "toggles verbose logs to be true or false",
            ToggleVerboseLogsCommandAction);

        private static readonly ServerCommand toggleSyncLocalSaveCommand = new ServerCommand("togglesynclocalsave", 0,
            "toggles allowing local saves to sync with server to be true or false",
            ToggleSyncLocalSaveCommandAction);

        private static readonly ServerCommand resetWorldCommand = new ServerCommand("resetworld", 0,
            "Resets all the world related data and stores a backup of it",
            ResetWorldCommandAction);

        private static readonly ServerCommand quitCommand = new ServerCommand("quit", 0,
            "Saves all player data and then closes the server",
            QuitCommandAction);

        private static readonly ServerCommand forceQuitCommand = new ServerCommand("forcequit", 0,
            "Closes the server without saving player data",
            ForceQuitCommandAction);

        private static readonly ServerCommand clearCommand = new ServerCommand("clear", 0,
            "Clears the console output",
            ClearCommandAction);

        public static readonly ServerCommand[] serverCommands = new ServerCommand[]
        {
            helpCommand,
            listCommand,
            deepListCommand,
            opCommand,
            deopCommand,
            kickCommand,
            banCommand,
            banListCommand,
            pardonCommand,
            reloadCommand,
            modListCommand,
            eventCommand,
            eventAllCommand,
            eventListCommand,
            doSiteRewards,
            broadcastCommand,
            serverMessageCommand,
            whitelistCommand,
            whitelistAddCommand,
            whitelistRemoveCommand,
            whitelistToggleCommand,
            forceSaveCommand,
            resetPlayerCommand,
            toggleDifficultyCommand,
            toggleCustomScenariosCommand,
            toggleDiscordPresenceCommand,
            toggleUPnPCommand,
            portforwardCommand,
            toggleVerboseLogsCommand,
            toggleSyncLocalSaveCommand,
            resetWorldCommand,
            quitCommand,
            forceQuitCommand,
            clearCommand
        };

        private static void HelpCommandAction()
        {
            Logger.Title($"List of available commands: [{serverCommands.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (ServerCommand command in serverCommands)
            {
                Logger.Warning($"{command.prefix} - {command.description}");
            }
            Logger.Title("----------------------------------------");
        }

        private static void ListCommandAction()
        {
            Logger.Title($"Connected players: [{NetworkHelper.GetConnectedClientsSafe().Count()}]");
            Logger.Title("----------------------------------------");
            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                Logger.Warning($"{client.userFile.Username} - {client.userFile.SavedIP}");
            }
            Logger.Title("----------------------------------------");
        }

        private static void DeepListCommandAction()
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

        private static void OpCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.userFile.Username == ConsoleCommandManager.commandParameters[0]);
            if (toFind == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

            else
            {
                if (CheckIfIsAlready(toFind)) return;
                else
                {
                    toFind.userFile.UpdateAdmin(true);

                    CommandManager.SendOpCommand(toFind);

                    Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' has now admin privileges");
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

        private static void DeopCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.userFile.Username == ConsoleCommandManager.commandParameters[0]);
            if (toFind == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

            else
            {
                if (CheckIfIsAlready(toFind)) return;
                else
                {
                    toFind.userFile.UpdateAdmin(false);

                    CommandManager.SendDeOpCommand(toFind);

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

        private static void KickCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.userFile.Username == ConsoleCommandManager.commandParameters[0]);
            if (toFind == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

            else
            {
                toFind.listener.disconnectFlag = true;

                Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' has been kicked from the server");
            }
        }

        private static void BanCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.userFile.Username == ConsoleCommandManager.commandParameters[0]);
            if (toFind == null)
            {
                UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleCommandManager.commandParameters[0]);
                if (userFile == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

                else
                {
                    if (CheckIfIsAlready(userFile)) return;
                    else
                    {
                        toFind.userFile.UpdateBan(true);

                        Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' has been banned from the server");
                    }
                }
            }

            else
            {
                toFind.listener.disconnectFlag = true;

                toFind.userFile.UpdateBan(true);

                Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' has been banned from the server");
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (userFile.IsBanned)
                {
                    Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was already banned from the server");
                    return true;
                }

                else return false;
            }
        }

        private static void BanListCommandAction()
        {
            List<UserFile> userFiles = UserManagerHelper.GetAllUserFiles().ToList().FindAll(x => x.IsBanned);

            Logger.Title($"Banned players: [{userFiles.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (UserFile user in userFiles) Logger.Warning($"{user.Username} - {user.SavedIP}");
            Logger.Title("----------------------------------------");
        }

        private static void PardonCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleCommandManager.commandParameters[0]);
            if (userFile == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else
                {
                    userFile.UpdateBan(false);

                    Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' is no longer banned from the server");
                }
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!userFile.IsBanned)
                {
                    Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not banned from the server");
                    return true;
                }

                else return false;
            }
        }

        private static void ReloadCommandAction() { Main_.LoadResources(); }
        
        private static void ModListCommandAction()
        {
            Logger.Title($"Required Mods: [{Master.loadedRequiredMods.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.loadedRequiredMods) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");

            Logger.Title($"Optional Mods: [{Master.loadedOptionalMods.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.loadedOptionalMods) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");

            Logger.Title($"Forbidden Mods: [{Master.loadedForbiddenMods.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.loadedForbiddenMods) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");
        }

        private static void DoSiteRewardsCommandAction()
        {
            Logger.Title($"Forced site rewards");
            SiteManager.SiteRewardTick();
        }

        private static void EventCommandAction()
        {
            ServerClient client = Network.connectedClients.ToList().Find(x => x.userFile.Username == ConsoleCommandManager.commandParameters[0]);
            if (client == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");
            else
            {
                EventFile toFind = EventManagerHelper.loadedEvents.FirstOrDefault(fetch => fetch.DefName == ConsoleCommandManager.commandParameters[1]);
                if (toFind == null) Logger.Warning($"Event '{ConsoleCommandManager.commandParameters[1]}' was not found");
                else
                {
                    CommandManager.SendEventCommand(client, toFind);

                    Logger.Title($"Sent event '{ConsoleCommandManager.commandParameters[1]}' to '{ConsoleCommandManager.commandParameters[0]}'");
                }
            }
        }

        private static void EventAllCommandAction()
        {
            EventFile toFind = EventManagerHelper.loadedEvents.FirstOrDefault(fetch => fetch.DefName == ConsoleCommandManager.commandParameters[0]);
            if (toFind == null) Logger.Warning($"Event '{ConsoleCommandManager.commandParameters[0]}' was not found");
            else
            {
                foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
                {
                    CommandManager.SendEventCommand(client, toFind);
                }

                Logger.Title($"Sent event '{ConsoleCommandManager.commandParameters[0]}' to every connected player");
            }
        }

        private static void EventListCommandAction()
        {
            Logger.Title($"Available events: [{EventManagerHelper.loadedEvents.Length}]");
            Logger.Title("----------------------------------------");
            foreach (EventFile eventFile in EventManagerHelper.loadedEvents) Logger.Warning($"{eventFile.DefName}");
            Logger.Title("----------------------------------------");
        }

        private static void BroadcastCommandAction()
        {
            string fullText = "";
            foreach (string str in ConsoleCommandManager.commandParameters) fullText += $"{str} ";
            fullText = fullText.Remove(fullText.Length - 1, 1);

            CommandManager.SendBroadcastCommand(fullText);

            Logger.Title($"Sent broadcast: '{fullText}'");
        }

        private static void ServerMessageCommandAction()
        {
            string fullText = "";
            foreach (string str in ConsoleCommandManager.commandParameters)
            {
                fullText += $"{str} ";
            }
            fullText = fullText.Remove(fullText.Length - 1, 1);

            ChatManager.BroadcastServerMessage(fullText);

            Logger.Title($"Sent chat: '{fullText}'");
        }

        private static void WhitelistCommandAction()
        {
            Logger.Title($"Whitelisted usernames: [{Master.whitelist.WhitelistedUsers.Count()}]");
            Logger.Title("----------------------------------------");
            foreach (string str in Master.whitelist.WhitelistedUsers) Logger.Warning($"{str}");
            Logger.Title("----------------------------------------");
        }

        private static void WhitelistAddCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleCommandManager.commandParameters[0]);
            if (userFile == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.AddUserToWhitelist(ConsoleCommandManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (Master.whitelist.WhitelistedUsers.Contains(userFile.Username))
                {
                    Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was already whitelisted");
                    return true;
                }

                else return false;
            }
        }

        private static void WhitelistRemoveCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleCommandManager.commandParameters[0]);
            if (userFile == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.RemoveUserFromWhitelist(ConsoleCommandManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!Master.whitelist.WhitelistedUsers.Contains(userFile.Username))
                {
                    Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not whitelisted");
                    return true;
                }

                else return false;
            }
        }

        private static void WhitelistToggleCommandAction() { WhitelistManager.ToggleWhitelist(); }

        private static void ForceSaveCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.userFile.Username == ConsoleCommandManager.commandParameters[0]);
            if (toFind == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");

            else
            {
                CommandManager.SendForceSaveCommand(toFind);
                Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' has been forced to save");
            }
        }

        private static void ResetPlayerCommandAction()
        {
            UserFile userFile = UserManagerHelper.GetUserFileFromName(ConsoleCommandManager.commandParameters[0]);
            ServerClient toFind = UserManagerHelper.GetConnectedClientFromUsername(userFile.Username);

            if (userFile == null) Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' was not found");
            else SaveManager.ResetPlayerData(toFind, userFile.Username);
        }

        private static void ToggleDifficultyCommandAction()
        {
            Master.difficultyValues.UseCustomDifficulty = !Master.difficultyValues.UseCustomDifficulty;
            Logger.Warning($"Custom difficulty is now {(Master.difficultyValues.UseCustomDifficulty ? ("Enabled") : ("Disabled"))}");
            Main_.SaveValueFile(ServerFileMode.Difficulty);
        }

        private static void ToggleCustomScenariosCommandAction()
        {
            Master.actionValues.EnableCustomScenarios = !Master.actionValues.EnableCustomScenarios;
            Logger.Warning($"Custom scenarios are now {(Master.actionValues.EnableCustomScenarios ? ("Enabled") : ("Disabled"))}");
            Main_.SaveValueFile(ServerFileMode.Configs);
        }

        private static void ToggleDiscordPressenceCommandAction()
        {
            Master.discordConfig.Enabled = !Master.discordConfig.Enabled;
            Logger.Warning($"Discord pressence is now {(Master.discordConfig.Enabled ? ("Enabled") : ("Disabled"))}");
            Logger.Warning("Please restart the server to start the service");
            Main_.SaveValueFile(ServerFileMode.Discord);
        }

        private static void ToggleUPnPCommandAction()
        {
            Master.serverConfig.UseUPnP = !Master.serverConfig.UseUPnP;
            Logger.Warning($"UPnP port mapping is now {(Master.serverConfig.UseUPnP ? ("Enabled") : ("Disabled"))}");
            Main_.SaveValueFile(ServerFileMode.Configs);

            if (Master.serverConfig.UseUPnP)
            {
                portforwardQuestion:
                    Logger.Warning("You have enabled UPnP on the server. Would you like to portforward?");
                    Logger.Warning("Please type 'YES' or 'NO'");

                    string response = Console.ReadLine();

                    if (response == "YES") _ = new UPnP();

                    else if (response == "NO")
                    {
                        Logger.Warning("You can use the command 'portforward' in the future to portforward the server");
                    }

                    else
                    {
                        Logger.Error("The response you have entered is not a valid option. Please make sure your response is capitalized");
                        goto portforwardQuestion;
                    }
            }
            else Logger.Warning("If a port has already been forwarded using UPnP, it will continute to be active until the server is restarted");
        }

        private static void PortForwardCommandAction()
        {
            if (!Master.serverConfig.UseUPnP)
            {
                Logger.Error("Cannot portforward because UPnP is disabled on the server. You can use the command 'toggleupnp' to enable it.");
            }
            else _ = new UPnP();
        }

        private static void ToggleVerboseLogsCommandAction()
        {
            Master.serverConfig.VerboseLogs = !Master.serverConfig.VerboseLogs;
            Logger.Warning($"Verbose Logs set to {Master.serverConfig.VerboseLogs}");
            Main_.SaveValueFile(ServerFileMode.Configs);
        }

        private static void ToggleSyncLocalSaveCommandAction()
        {
            Master.serverConfig.SyncLocalSave = !Master.serverConfig.SyncLocalSave;
            Logger.Warning($"Sync Local Save set to {Master.serverConfig.SyncLocalSave}");
            Main_.SaveValueFile(ServerFileMode.Configs);
        }

        private static void ResetWorldCommandAction()
        {
            //Make sure the user wants to reset the world
            Logger.Warning("Are you sure you want to reset the world?");
            Logger.Warning("Please type 'YES' or 'NO'");

            DeleteWorldQuestion:
                string response = Console.ReadLine();

                if (response == "NO") return;
                else if (response != "YES")
                {
                    Logger.Error($"{response} is not a valid option; The options must be capitalized");
                    goto DeleteWorldQuestion;
                }
    
                string newWorldFolderName = $"World-{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
                string newWorldFolderPath = $"{Master.backupWorldPath + Path.DirectorySeparatorChar}{newWorldFolderName}";
                if (!Directory.Exists($"{newWorldFolderPath}")) Directory.CreateDirectory($"{newWorldFolderPath}");

                //Make the new folder and move all the current world folders to it
                Logger.Warning($"The archived world will be saved as: {newWorldFolderPath}");
                Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Core");

                //The core directory is special because we want to copy the files, not just move them.
                foreach (string file in Directory.GetFiles(Master.corePath))
                {
                    if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Core{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
                }

                //Remove the old world file
                File.Delete($"{Master.corePath + Path.DirectorySeparatorChar}WorldValues.json");

                //Move the rest of the directories
                if (Directory.Exists(Master.factionsPath)) Directory.Move(Master.factionsPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Factions");
                if (Directory.Exists(Master.mapsPath)) Directory.Move(Master.mapsPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Maps");
                if (Directory.Exists(Master.savesPath)) Directory.Move(Master.savesPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Saves");
                if (Directory.Exists(Master.settlementsPath)) Directory.Move(Master.settlementsPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Settlements");
                if (Directory.Exists(Master.sitesPath)) Directory.Move(Master.sitesPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Sites");
                if (Directory.Exists(Master.usersPath)) Directory.Move(Master.usersPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Users");
                if (Directory.Exists(Master.caravansPath)) Directory.Move(Master.caravansPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Caravans");

                Main_.SetPaths();
                Logger.Warning("World has been successfully reset and archived");
                foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe()) client.listener.disconnectFlag = true;
        }

        private static void QuitCommandAction()
        {
            Master.isClosing = true;

            Logger.Warning($"Waiting for all saves to quit");

            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe()) CommandManager.SendForceSaveCommand(client);

            while (NetworkHelper.GetConnectedClientsSafe().Length > 0) Thread.Sleep(1);

            Environment.Exit(0);
        }

        private static void ForceQuitCommandAction() { Environment.Exit(0); }

        private static void ClearCommandAction()
        {
            Console.Clear();

            Logger.Title("[Cleared console]");
        }
    }
}