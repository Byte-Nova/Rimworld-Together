namespace GameServer
{
    public static class ServerCommandManager
    {
        public static string[] eventTypes = new string[]
        {
            "Raid",
            "Infestation",
            "MechCluster",
            "ToxicFallout",
            "Manhunter",
            "Wanderer",
            "FarmAnimals",
            "ShipChunks",
            "TraderCaravan"
        };

        public static string[] commandParameters;

        public static void ParseServerCommands(string parsedString)
        {
            string parsedPrefix = parsedString.Split(' ')[0].ToLower();
            int parsedParameters = parsedString.Split(' ').Count() - 1;
            commandParameters = parsedString.Replace(parsedPrefix + " ", "").Split(" ");

            try
            {
                ServerCommand commandToFetch = ServerCommandStorage.serverCommands.ToList().Find(x => x.prefix == parsedPrefix);
                if (commandToFetch == null) Logger.WriteToConsole($"[ERROR] > Command '{parsedPrefix}' was not found", Logger.LogMode.Warning);
                else
                {
                    if (commandToFetch.parameters != parsedParameters && commandToFetch.parameters != -1)
                    {
                        Logger.WriteToConsole($"[ERROR] > Command '{commandToFetch.prefix}' wanted [{commandToFetch.parameters}] parameters "
                            + $"but was passed [{parsedParameters}]", Logger.LogMode.Warning);
                    }

                    else
                    {
                        if (commandToFetch.commandAction != null) commandToFetch.commandAction.Invoke();

                        else Logger.WriteToConsole($"[ERROR] > Command '{commandToFetch.prefix}' didn't have any action built in", 
                            Logger.LogMode.Warning);
                    }
                }
            }
            catch (Exception e) { Logger.WriteToConsole($"[Error] > Couldn't parse command '{parsedPrefix}'. Reason: {e}", Logger.LogMode.Error); }
        }

        public static void ListenForServerCommands()
        {
            bool interactiveConsole;

            try
            {
                if (Console.In.Peek() != -1) interactiveConsole = true;
                else interactiveConsole = false;
            }

            catch
            {
                interactiveConsole = false;
                Logger.WriteToConsole($"[Warning] > Couldn't found interactive console, disabling commands", Logger.LogMode.Warning);
            }

            if (interactiveConsole)
            {
                while (true)
                {
                    ParseServerCommands(Console.ReadLine());
                }
            }
            else Logger.WriteToConsole($"[Warning] > Couldn't found interactive console, disabling commands", Logger.LogMode.Warning);
        }
    }

    public static class ServerCommandStorage
    {
        private static ServerCommand helpCommand = new ServerCommand("help", 0,
            "Shows a list of all available commands to use",
            HelpCommandAction);

        private static ServerCommand listCommand = new ServerCommand("list", 0,
            "Shows all connected players",
            ListCommandAction);

        private static ServerCommand opCommand = new ServerCommand("op", 1,
            "Gives admin privileges to the selected player",
            OpCommandAction);

        private static ServerCommand deopCommand = new ServerCommand("deop", 1,
            "Removes admin privileges from the selected player",
            DeopCommandAction);

        private static ServerCommand kickCommand = new ServerCommand("kick", 1,
            "Kicks the selected player from the server",
            KickCommandAction);

        private static ServerCommand banCommand = new ServerCommand("ban", 1,
            "Bans the selected player from the server",
            BanCommandAction);

        private static ServerCommand pardonCommand = new ServerCommand("pardon", 1,
            "Pardons the selected player from the server",
            PardonCommandAction);

        private static ServerCommand deepListCommand = new ServerCommand("deeplist", 0,
            "Shows a list of all server players",
            DeepListCommandAction);

        private static ServerCommand banListCommand = new ServerCommand("banlist", 0,
            "Shows a list of all banned server players",
            BanListCommandAction);

        private static ServerCommand reloadCommand = new ServerCommand("reload", 0,
            "Reloads all server resources",
            ReloadCommandAction);

        private static ServerCommand modListCommand = new ServerCommand("modlist", 0,
            "Shows all currently loaded mods",
            ModListCommandAction);

        private static ServerCommand doSiteRewards = new ServerCommand("dositerewards", 0,
            "Forces site rewards to run",
            DoSiteRewardsCommandAction);

        private static ServerCommand eventCommand = new ServerCommand("event", 2,
            "Sends a command to the selecter players",
            EventCommandAction);

        private static ServerCommand eventAllCommand = new ServerCommand("eventall", 1,
            "Sends a command to all connected players",
            EventAllCommandAction);

        private static ServerCommand eventListCommand = new ServerCommand("eventlist", 0,
            "Shows a list of all available events to use",
            EventListCommandAction);

        private static ServerCommand broadcastCommand = new ServerCommand("broadcast", -1,
            "Broadcast a message to all connected players",
            BroadcastCommandAction);

        private static ServerCommand serverMessageCommand = new ServerCommand("chat", -1,
            "Send a message in chat from the Server",
            ServerMessageCommandAction);

        private static ServerCommand clearCommand = new ServerCommand("clear", 0,
            "Clears the console output",
            ClearCommandAction);

        private static ServerCommand whitelistCommand = new ServerCommand("whitelist", 0,
            "Shows all whitelisted players",
            WhitelistCommandAction);

        private static ServerCommand whitelistAddCommand = new ServerCommand("whitelistadd", 1,
            "Adds a player to the whitelist",
            WhitelistAddCommandAction);

        private static ServerCommand whitelistRemoveCommand = new ServerCommand("whitelistremove", 1,
            "Removes a player from the whitelist",
            WhitelistRemoveCommandAction);

        private static ServerCommand whitelistToggleCommand = new ServerCommand("togglewhitelist", 0,
            "Toggles the whitelist ON or OFF",
            WhitelistToggleCommandAction);

        private static ServerCommand forceSaveCommand = new ServerCommand("forcesave", 1,
            "Forces a player to sync their save",
            ForceSaveCommandAction);

        private static ServerCommand deletePlayerCommand = new ServerCommand("deleteplayer", 1,
            "Deletes all data of a player",
            DeletePlayerCommandAction);

        private static ServerCommand enableDifficultyCommand = new ServerCommand("enabledifficulty", 0,
            "Enables custom difficulty in the server",
            EnableDifficultyCommandAction);

        private static ServerCommand disableDifficultyCommand = new ServerCommand("disabledifficulty", 0,
            "Disables custom difficulty in the server",
            DisableDifficultyCommandAction);

        private static ServerCommand quitCommand = new ServerCommand("quit", 0,
            "Saves all player details and then closes the server",
            QuitCommandAction);

        private static ServerCommand forceQuitCommand = new ServerCommand("forcequit", 0,
            "Closes the server without saving player details",
            ForceQuitCommandAction);

        public static ServerCommand[] serverCommands = new ServerCommand[]
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
            clearCommand,
            forceSaveCommand,
            deletePlayerCommand,
            enableDifficultyCommand,
            disableDifficultyCommand,
            quitCommand,
            forceQuitCommand
        };

        private static void HelpCommandAction()
        {
            Logger.WriteToConsole($"List of available commands: [{serverCommands.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (ServerCommand command in serverCommands)
            {
                Logger.WriteToConsole($"{command.prefix} - {command.description}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
        }

        private static void ListCommandAction()
        {
            Logger.WriteToConsole($"Connected players: [{Network.connectedClients.ToArray().Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                Logger.WriteToConsole($"{client.username} - {client.SavedIP}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
        }

        private static void DeepListCommandAction()
        {
            UserFile[] userFiles = UserManager.GetAllUserFiles();

            Logger.WriteToConsole($"Server players: [{userFiles.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (UserFile user in userFiles)
            {
                Logger.WriteToConsole($"{user.username} - {user.SavedIP}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
        }

        private static void OpCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == ServerCommandManager.commandParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found", 
                Logger.LogMode.Warning);

            else
            {
                if (CheckIfIsAlready(toFind)) return;
                else
                {
                    toFind.isAdmin = true;

                    UserFile userFile = UserManager.GetUserFile(toFind);
                    userFile.isAdmin = true;
                    UserManager.SaveUserFile(toFind, userFile);

                    CommandManager.SendOpCommand(toFind);

                    Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' has now admin privileges",
                        Logger.LogMode.Warning);
                }
            }

            bool CheckIfIsAlready(ServerClient client)
            {
                if (client.isAdmin)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{client.username}' " +
                    $"was already an admin", Logger.LogMode.Warning);
                    return true;
                }

                else return false;
            }
        }

        private static void DeopCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == ServerCommandManager.commandParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found", 
                Logger.LogMode.Warning);

            else
            {
                if (CheckIfIsAlready(toFind)) return;
                else
                {
                    toFind.isAdmin = false;

                    UserFile userFile = UserManager.GetUserFile(toFind);
                    userFile.isAdmin = false;
                    UserManager.SaveUserFile(toFind, userFile);

                    CommandManager.SendDeOpCommand(toFind);

                    Logger.WriteToConsole($"User '{toFind.username}' is no longer an admin",
                        Logger.LogMode.Warning);
                }
            }

            bool CheckIfIsAlready(ServerClient client)
            {
                if (!client.isAdmin)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{client.username}' " +
                    $"was not an admin", Logger.LogMode.Warning);
                    return true;
                }

                else return false;
            }
        }

        private static void KickCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == ServerCommandManager.commandParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                Logger.LogMode.Warning);

            else
            {
                toFind.listener.disconnectFlag = true;

                Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' has been kicked from the server",
                    Logger.LogMode.Warning);
            }
        }

        private static void BanCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == ServerCommandManager.commandParameters[0]);
            if (toFind == null)
            {
                UserFile userFile = UserManager.GetUserFileFromName(ServerCommandManager.commandParameters[0]);
                if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                    Logger.LogMode.Warning);

                else
                {
                    if (CheckIfIsAlready(userFile)) return;
                    else
                    {
                        userFile.isBanned = true;
                        UserManager.SaveUserFileFromName(userFile.username, userFile);

                        Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' has been banned from the server",
                            Logger.LogMode.Warning);
                    }
                }
            }

            else
            {
                toFind.listener.disconnectFlag = true;

                UserFile userFile = UserManager.GetUserFile(toFind);
                userFile.isBanned = true;
                UserManager.SaveUserFile(toFind, userFile);

                Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' has been banned from the server",
                    Logger.LogMode.Warning);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (userFile.isBanned)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' " +
                    $"was already banned from the server", Logger.LogMode.Warning);
                    return true;
                }

                else return false;
            }
        }

        private static void BanListCommandAction()
        {
            List<UserFile> userFiles = UserManager.GetAllUserFiles().ToList().FindAll(x => x.isBanned);

            Logger.WriteToConsole($"Banned players: [{userFiles.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (UserFile user in userFiles)
            {
                Logger.WriteToConsole($"{user.username} - {user.SavedIP}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
        }

        private static void PardonCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(ServerCommandManager.commandParameters[0]);
            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                Logger.LogMode.Warning);

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else
                {
                    userFile.isBanned = false;
                    UserManager.SaveUserFileFromName(userFile.username, userFile);

                    Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' is no longer banned from the server",
                        Logger.LogMode.Warning);
                }
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!userFile.isBanned)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' " +
                    $"was not banned from the server", Logger.LogMode.Warning);
                    return true;
                }

                else return false;
            }
        }

        private static void ReloadCommandAction()
        {
            Master.LoadResources();
        }

        private static void ModListCommandAction()
        {
            Logger.WriteToConsole($"Required Mods: [{Master.loadedRequiredMods.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (string str in Master.loadedRequiredMods)
            {
                Logger.WriteToConsole($"{str}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);

            Logger.WriteToConsole($"Optional Mods: [{Master.loadedOptionalMods.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (string str in Master.loadedOptionalMods)
            {
                Logger.WriteToConsole($"{str}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);

            Logger.WriteToConsole($"Forbidden Mods: [{Master.loadedForbiddenMods.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (string str in Master.loadedForbiddenMods)
            {
                Logger.WriteToConsole($"{str}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
        }

        private static void DoSiteRewardsCommandAction()
        {
            Logger.WriteToConsole($"Forced site rewards", Logger.LogMode.Title);
            SiteManager.SiteRewardTick();
        }

        private static void EventCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == ServerCommandManager.commandParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                Logger.LogMode.Warning);

            else
            {
                for(int i = 0; i < ServerCommandManager.eventTypes.Count(); i++)
                {
                    if (ServerCommandManager.eventTypes[i] == ServerCommandManager.commandParameters[1])
                    {
                        CommandManager.SendEventCommand(toFind, i);

                        Logger.WriteToConsole($"Sent event '{ServerCommandManager.commandParameters[1]}' to {toFind.username}", 
                            Logger.LogMode.Warning);

                        return;
                    }
                }

                Logger.WriteToConsole($"[ERROR] > Event '{ServerCommandManager.commandParameters[1]}' was not found",
                    Logger.LogMode.Warning);
            }   
        }

        private static void EventAllCommandAction()
        {
            for (int i = 0; i < ServerCommandManager.eventTypes.Count(); i++)
            {
                if (ServerCommandManager.eventTypes[i] == ServerCommandManager.commandParameters[0])
                {
                    foreach (ServerClient client in Network.connectedClients.ToArray())
                    {
                        CommandManager.SendEventCommand(client, i);
                    }

                    Logger.WriteToConsole($"Sent event '{ServerCommandManager.commandParameters[0]}' to every connected player",
                        Logger.LogMode.Title);

                    return;
                }
            }

            Logger.WriteToConsole($"[ERROR] > Event '{ServerCommandManager.commandParameters[0]}' was not found",
                    Logger.LogMode.Warning);
        }

        private static void EventListCommandAction()
        {
            Logger.WriteToConsole($"Available events: [{ServerCommandManager.eventTypes.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (string str in ServerCommandManager.eventTypes)
            {
                Logger.WriteToConsole($"{str}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
        }

        private static void BroadcastCommandAction()
        {
            string fullText = "";
            foreach(string str in ServerCommandManager.commandParameters)
            {
                fullText += $"{str} ";
            }
            fullText = fullText.Remove(fullText.Length - 1, 1);

            CommandManager.SendBroadcastCommand(fullText);

            Logger.WriteToConsole($"Sent broadcast '{fullText}'", Logger.LogMode.Title);
        }

        private static void ServerMessageCommandAction()
        {
            string fullText = "";
            foreach(string str in ServerCommandManager.commandParameters)
            {
                fullText += $"{str} ";
            }
            fullText = fullText.Remove(fullText.Length - 1, 1);

            ChatManager.BroadcastServerMessages(fullText);
        }

        private static void WhitelistCommandAction()
        {
            Logger.WriteToConsole($"Whitelisted usernames: [{Master.whitelist.WhitelistedUsers.Count()}]", Logger.LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
            foreach (string str in Master.whitelist.WhitelistedUsers)
            {
                Logger.WriteToConsole($"{str}", Logger.LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", Logger.LogMode.Title, false);
        }

        private static void WhitelistAddCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(ServerCommandManager.commandParameters[0]);
            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                Logger.LogMode.Warning);

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.AddUserToWhitelist(ServerCommandManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (Master.whitelist.WhitelistedUsers.Contains(userFile.username))
                {
                    Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' " +
                        $"was already whitelisted", Logger.LogMode.Warning);

                    return true;
                }

                else return false;
            }
        }

        private static void WhitelistRemoveCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(ServerCommandManager.commandParameters[0]);
            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                Logger.LogMode.Warning);

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.RemoveUserFromWhitelist(ServerCommandManager.commandParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!Master.whitelist.WhitelistedUsers.Contains(userFile.username))
                {
                    Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' " +
                        $"was not whitelisted", Logger.LogMode.Warning);

                    return true;
                }

                else return false;
            }
        }

        private static void WhitelistToggleCommandAction()
        {
            WhitelistManager.ToggleWhitelist();
        }

        private static void ForceSaveCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == ServerCommandManager.commandParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                Logger.LogMode.Warning);

            else
            {
                CommandManager.SendForceSaveCommand(toFind);

                Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' has been forced to save",
                    Logger.LogMode.Warning);
            }
        }

        private static void DeletePlayerCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(ServerCommandManager.commandParameters[0]);
            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{ServerCommandManager.commandParameters[0]}' was not found",
                Logger.LogMode.Warning);

            else SaveManager.DeletePlayerDetails(userFile.username);
        }

        private static void EnableDifficultyCommandAction()
        {
            if (Master.difficultyValues.UseCustomDifficulty == true)
            {
                Logger.WriteToConsole($"[ERROR] > Custom difficulty was already enabled", Logger.LogMode.Warning);
            }

            else
            {
                Master.difficultyValues.UseCustomDifficulty = true;
                CustomDifficultyManager.SaveCustomDifficulty(Master.difficultyValues);

                Logger.WriteToConsole($"Custom difficulty is now enabled", Logger.LogMode.Warning);
            }
        }

        private static void DisableDifficultyCommandAction()
        {
            if (Master.difficultyValues.UseCustomDifficulty == false)
            {
                Logger.WriteToConsole($"[ERROR] > Custom difficulty was already disabled", Logger.LogMode.Warning);
            }

            else
            {
                Master.difficultyValues.UseCustomDifficulty = false;
                CustomDifficultyManager.SaveCustomDifficulty(Master.difficultyValues);

                Logger.WriteToConsole($"Custom difficulty is now disabled", Logger.LogMode.Warning);
            }
        }

        private static void QuitCommandAction()
        {
            Master.isClosing = true;

            Logger.WriteToConsole($"Waiting for all saves to quit", Logger.LogMode.Warning);

            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                CommandManager.SendForceSaveCommand(client);
            }

            while (Network.connectedClients.ToArray().Length > 0)
            {
                Thread.Sleep(1);
            }

            Environment.Exit(0);
        }

        private static void ForceQuitCommandAction() { Environment.Exit(0); }

        private static void ClearCommandAction()
        {
            Console.Clear();

            Logger.WriteToConsole("[Cleared console]", Logger.LogMode.Title);
        }
    }
}