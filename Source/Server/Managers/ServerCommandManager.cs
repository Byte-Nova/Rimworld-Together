using static GameServer.ServerCommandManager;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ServerCommandManager
    {

        //history of commands and the current one being written
        //Index 0 is the current command being written
        public static List<string> commandHistory = new() { "" };
        public static int commandHistoryPosition = 0;



        public class Command
        {
            public string command;
            public string description;
            public int parameterCount;
            public Action commandAction;

            public Command(string command, int parameterCount, string description, Action commandAction)
            {
                this.command = command;
                this.parameterCount = parameterCount;
                this.description = description;
                this.commandAction = commandAction;
            }
        }

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

        public static List<string> parsedParameters;

        public static void ParseServerCommands(string unparsedCommand)
        {
            //Grab the command string and the number of passed parameters
            List<string> parsedCommand = unparsedCommand.Split(' ').ToList();
            string mnemonic = parsedCommand[0];
            int parsedParameterCount = parsedCommand.Count() - 1;
            parsedParameters = parsedCommand.Skip(1).ToList();

            try
            {
                //Find the command in the dictionary
                Command commandToFetch = commandDictionary.ToList().Find(x => x.Key == mnemonic).Value;

                //Could not find command
                if (commandToFetch == null)
                {
                    Logger.Warning($"Command '{parsedCommand[0]}' was not found");
                }

                //Incorrect number of parameters
                else if (commandToFetch.parameterCount != parsedParameterCount && commandToFetch.parameterCount != -1)
                {
                    Logger.Warning($"Command '{commandToFetch.command}' wanted [{commandToFetch.parameterCount}] parameters but was passed [{parsedParameterCount}]");
                }

                //Run Action
                else { commandToFetch.commandAction.Invoke(); }
            }
            catch (Exception e) { Logger.Error($"Couldn't parse command '{parsedCommand[0]}'. Reason: {e}"); }
        }

        public static void ListenForServerCommands()
        {

            List<string> tabbedCommands = new List<string>();
            int tabbedCommandsIndex = 0;

            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);

                switch (cki.Key)
                {
                    case ConsoleKey.Enter:

                        if (commandHistoryPosition != 0)  commandHistory[0] = commandHistory[commandHistoryPosition];
                        if (commandHistory.Count() >= 20) commandHistory.RemoveAt(commandHistory.Count() - 1);

                        Logger.ClearCurrentLine();
                        Logger.Message(commandHistory[0]);

                        commandHistory.Insert(0, "");
                        commandHistoryPosition = 0;

                        ParseServerCommands(commandHistory[1]);
                        continue;

                    case ConsoleKey.Backspace:
                        if (commandHistory[0].Count() > 0) 
                            commandHistory[0] = commandHistory[0].Substring(0, commandHistory[0].Count() - 1);
                        break;

                    case ConsoleKey.UpArrow:
                        if (commandHistoryPosition != commandHistory.Count() - 1) 
                            commandHistoryPosition++;
                        break;

                    case ConsoleKey.DownArrow:
                        if (commandHistoryPosition != 0) 
                            commandHistoryPosition--;
                        break;

                    case ConsoleKey.Tab:
                        if (tabbedCommands.Count() > 0)
                        {
                            tabbedCommandsIndex++;
                            if(tabbedCommandsIndex >= tabbedCommands.Count())
                                tabbedCommandsIndex = 0;
                            commandHistory[0] = tabbedCommands[tabbedCommandsIndex];
                        }
                        else
                        {
                            tabbedCommands = commandDictionary.Keys.ToList().FindAll(x => x.StartsWith(commandHistory[0], StringComparison.OrdinalIgnoreCase)).ToList();
                            if (tabbedCommands.Count() > 0)
                                commandHistory[0] = tabbedCommands[0];
                        }
                        break;
                    default:
                        commandHistory[0] += cki.KeyChar;
                        break;
                }

                if (cki.Key != ConsoleKey.Tab)
                {
                    tabbedCommands.Clear();
                    tabbedCommandsIndex = -1;
                }

                Console.CursorVisible = false;

                Logger.ClearCurrentLine();
                Console.Write($"{commandHistory[commandHistoryPosition]}");

                Console.CursorVisible = true;
            }
        }

        public static void WriteCurrentCommand()
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(commandHistory[0]);
            Console.ForegroundColor = currentColor;
        }

        public static Dictionary<string, Command> commandDictionary = new()
        {
            {
                "help", new Command("help", 0,
                "Shows a list of all available commands to use",
                HelpCommandAction)
            },

            {
                "list", new Command("list", 0,
                "Shows all connected players",
                ListCommandAction)
            },

            {
                "op", new Command("op", 1,
                "Gives admin privileges to the selected player",
                OpCommandAction)
            },

            {
                "deop", new Command("deop", 1,
                "Removes admin privileges from the selected player",
                DeopCommandAction)
            },

            {
                "kick", new Command("kick", 1,
                "Kicks the selected player from the server",
                KickCommandAction)
            },

            {
                "ban", new Command("ban", 1,
                "Bans the selected player from the server",
                BanCommandAction)
            },

            {
                "pardon", new Command("pardon", 1,
                "Pardons the selected player from the server",
                PardonCommandAction)
            },

            {
                "deeplist", new Command("deeplist", 0,
                "Shows a list of all server players",
                DeepListCommandAction)
            },

            {
                "banlist", new Command("banlist", 0,
                "Shows a list of all banned server players",
                BanListCommandAction)
            },

            {
                "reload", new Command("reload", 0,
                "Reloads all server resources",
                ReloadCommandAction)
            },

            {
                "modlist", new Command("modlist", 0,
                "Shows all currently loaded mods",
                ModListCommandAction)
            },

            {
                "dositerewards", new Command("dositerewards", 0,
                "Forces site rewards to run",
                DoSiteRewardsCommandAction)
            },

            {
                "event", new Command("event", 2,
                "Sends a command to the selecter players",
                EventCommandAction)
            },

            {
                "eventall", new Command("eventall", 1,
                "Sends a command to all connected players",
                EventAllCommandAction)
            },

            {
                "eventlist", new Command("eventlist", 0,
                "Shows a list of all available events to use",
                EventListCommandAction)
            },

            {
                "broadcast", new Command("broadcast", -1,
                "Broadcast a message to all connected players",
                BroadcastCommandAction)
            },

            {
                "chat", new Command("chat", -1,
                "Send a message in chat from the Server",
                ServerMessageCommandAction)
            },

            {
                "clear", new Command("clear", 0,
                "Clears the console output",
                ClearCommandAction)
            },

            {
                "whitelist", new Command("whitelist", 0,
                "Shows all whitelisted players",
                WhitelistCommandAction)
            },

            {
                "whitelistadd", new Command("whitelistadd", 1,
                "Adds a player to the whitelist",
                WhitelistAddCommandAction)
            },

            {
                "whitelistremove", new Command("whitelistremove", 1,
                "Removes a player from the whitelist",
                WhitelistRemoveCommandAction)
            },

            {
                "togglewhitelist", new Command("togglewhitelist", 0,
                "Toggles the whitelist ON or OFF",
                WhitelistToggleCommandAction)
            },

            {
                "forcesave", new Command("forcesave", 1,
                "Forces a player to sync their save",
                ForceSaveCommandAction)
            },

            {
                "deleteplayer", new Command("deleteplayer", 1,
                "Deletes all data of a player",
                DeletePlayerCommandAction)
            },

            {
                "enabledifficulty", new Command("enabledifficulty", 0,
                "Enables custom difficulty in the server",
                EnableDifficultyCommandAction)
            },

            {
                "disabledifficulty", new Command("disabledifficulty", 0,
                "Disables custom difficulty in the server",
                DisableDifficultyCommandAction)
            },

            {
                "togglecustomscenarios", new Command("togglecustomscenarios", 0,
                "enables/disables custom scenarios on the server",
                ToggleCustomScenariosCommandAction) 
            },

            {
                "quit", new Command("quit", 0,
                "Saves all player details and then closes the server",
                QuitCommandAction)
            },

            {
                "forcequit", new Command("forcequit", 0,
                "Closes the server without saving player details",
                ForceQuitCommandAction) 
            },

            {
                "toggleverboselogs", new Command("toggleverboselogs",0,
                "toggles verbose logs to be true or false",
                ToggleVerboseLogs)
            }
        };

        private static void HelpCommandAction()
        {
            Logger.WriteToConsole($"List of available commands: [{commandDictionary.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (Command command in commandDictionary.Values)
            {
                Logger.WriteToConsole($"{command.command} - {command.description}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
        }

        private static void ListCommandAction()
        {
            Logger.WriteToConsole($"Connected players: [{Network.connectedClients.ToArray().Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                Logger.WriteToConsole($"{client.username} - {client.SavedIP}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
        }

        private static void DeepListCommandAction()
        {
            UserFile[] userFiles = UserManager.GetAllUserFiles();

            Logger.WriteToConsole($"Server players: [{userFiles.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (UserFile user in userFiles)
            {
                Logger.WriteToConsole($"{user.username} - {user.SavedIP}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
        }

        private static void OpCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == parsedParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found", 
                LogMode.Warning);

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

                    Logger.WriteToConsole($"User '{parsedParameters[0]}' has now admin privileges",
                        LogMode.Warning);
                }
            }

            bool CheckIfIsAlready(ServerClient client)
            {
                if (client.isAdmin)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{client.username}' " +
                    $"was already an admin", LogMode.Warning);
                    return true;
                }

                else return false;
            }
        }

        private static void DeopCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == parsedParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found", 
                LogMode.Warning);

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
                        LogMode.Warning);
                }
            }

            bool CheckIfIsAlready(ServerClient client)
            {
                if (!client.isAdmin)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{client.username}' " +
                    $"was not an admin", LogMode.Warning);
                    return true;
                }

                else return false;
            }
        }

        private static void KickCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == parsedParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found",
                LogMode.Warning);

            else
            {
                toFind.listener.disconnectFlag = true;

                Logger.WriteToConsole($"User '{parsedParameters[0]}' has been kicked from the server",
                    LogMode.Warning);
            }
        }

        private static void BanCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == parsedParameters[0]);
            if (toFind == null)
            {
                UserFile userFile = UserManager.GetUserFileFromName(parsedParameters[0]);
                if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found",
                    LogMode.Warning);

                else
                {
                    if (CheckIfIsAlready(userFile)) return;
                    else
                    {
                        userFile.isBanned = true;
                        UserManager.SaveUserFileFromName(userFile.username, userFile);

                        Logger.WriteToConsole($"User '{parsedParameters[0]}' has been banned from the server",
                            LogMode.Warning);
                    }
                }
            }

            else
            {
                toFind.listener.disconnectFlag = true;

                UserFile userFile = UserManager.GetUserFile(toFind);
                userFile.isBanned = true;
                UserManager.SaveUserFile(toFind, userFile);

                Logger.WriteToConsole($"User '{parsedParameters[0]}' has been banned from the server",
                    LogMode.Warning);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (userFile.isBanned)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' " +
                    $"was already banned from the server", LogMode.Warning);
                    return true;
                }

                else return false;
            }
        }

        private static void BanListCommandAction()
        {
            List<UserFile> userFiles = UserManager.GetAllUserFiles().ToList().FindAll(x => x.isBanned);

            Logger.WriteToConsole($"Banned players: [{userFiles.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (UserFile user in userFiles)
            {
                Logger.WriteToConsole($"{user.username} - {user.SavedIP}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
        }

        private static void PardonCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(parsedParameters[0]);

            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found", LogMode.Warning);
            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else
                {
                    userFile.isBanned = false;
                    UserManager.SaveUserFileFromName(userFile.username, userFile);

                    Logger.WriteToConsole($"User '{parsedParameters[0]}' is no longer banned from the server",
                        LogMode.Warning);
                }
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!userFile.isBanned)
                {
                    Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' " +
                    $"was not banned from the server", LogMode.Warning);
                    return true;
                }
                else return false;
            }
        }

        private static void ReloadCommandAction() { Master.LoadResources(); }

        private static void ModListCommandAction()
        {
            Logger.WriteToConsole($"Required Mods: [{Master.loadedRequiredMods.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (string str in Master.loadedRequiredMods)
            {
                Logger.WriteToConsole($"{str}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);

            Logger.WriteToConsole($"Optional Mods: [{Master.loadedOptionalMods.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (string str in Master.loadedOptionalMods)
            {
                Logger.WriteToConsole($"{str}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);

            Logger.WriteToConsole($"Forbidden Mods: [{Master.loadedForbiddenMods.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (string str in Master.loadedForbiddenMods)
            {
                Logger.WriteToConsole($"{str}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
        }

        private static void DoSiteRewardsCommandAction()
        {
            Logger.WriteToConsole($"Forced site rewards", LogMode.Title);
            SiteManager.SiteRewardTick();
        }

        private static void EventCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == parsedParameters[0]);
            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found",
                LogMode.Warning);

            else
            {
                for(int i = 0; i < eventTypes.Count(); i++)
                {
                    if (eventTypes[i] == parsedParameters[1])
                    {
                        CommandManager.SendEventCommand(toFind, i);

                        Logger.WriteToConsole($"Sent event '{parsedParameters[1]}' to {toFind.username}", 
                            LogMode.Warning);

                        return;
                    }
                }

                Logger.WriteToConsole($"[ERROR] > Event '{parsedParameters[1]}' was not found",
                    LogMode.Warning);
            }   
        }

        private static void EventAllCommandAction()
        {
            for (int i = 0; i < eventTypes.Count(); i++)
            {
                if (eventTypes[i] == parsedParameters[0])
                {
                    foreach (ServerClient client in Network.connectedClients.ToArray())
                    {
                        CommandManager.SendEventCommand(client, i);
                    }

                    Logger.WriteToConsole($"Sent event '{parsedParameters[0]}' to every connected player",
                        LogMode.Title);

                    return;
                }
            }

            Logger.WriteToConsole($"[ERROR] > Event '{parsedParameters[0]}' was not found",
                LogMode.Warning);
        }

        private static void EventListCommandAction()
        {
            Logger.WriteToConsole($"Available events: [{eventTypes.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (string str in eventTypes)
            {
                Logger.WriteToConsole($"{str}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
        }

        private static void BroadcastCommandAction()
        {
            string fullText = "";
            foreach(string str in parsedParameters)
            {
                fullText += $"{str} ";
            }
            fullText = fullText.Remove(fullText.Length - 1, 1);

            CommandManager.SendBroadcastCommand(fullText);

            Logger.WriteToConsole($"Sent broadcast: '{fullText}'", LogMode.Title);
        }

        private static void ServerMessageCommandAction()
        {
            string fullText = "";
            foreach(string str in parsedParameters)
            {
                fullText += $"{str} ";
            }
            fullText = fullText.Remove(fullText.Length - 1, 1);

            ChatManager.BroadcastServerMessage(fullText);

            Logger.WriteToConsole($"Sent chat: '{fullText}'", LogMode.Title);
        }

        private static void WhitelistCommandAction()
        {
            Logger.WriteToConsole($"Whitelisted usernames: [{Master.whitelist.WhitelistedUsers.Count()}]", LogMode.Title, false);
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
            foreach (string str in Master.whitelist.WhitelistedUsers)
            {
                Logger.WriteToConsole($"{str}", LogMode.Warning, writeToLogs: false);
            }
            Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
        }

        private static void WhitelistAddCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(parsedParameters[0]);
            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found",
                LogMode.Warning);

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.AddUserToWhitelist(parsedParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (Master.whitelist.WhitelistedUsers.Contains(userFile.username))
                {
                    Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' " +
                        $"was already whitelisted", LogMode.Warning);

                    return true;
                }

                else return false;
            }
        }

        private static void WhitelistRemoveCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(parsedParameters[0]);
            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found",
                LogMode.Warning);

            else
            {
                if (CheckIfIsAlready(userFile)) return;
                else WhitelistManager.RemoveUserFromWhitelist(parsedParameters[0]);
            }

            bool CheckIfIsAlready(UserFile userFile)
            {
                if (!Master.whitelist.WhitelistedUsers.Contains(userFile.username))
                {
                    Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' " +
                        $"was not whitelisted", LogMode.Warning);

                    return true;
                }

                else return false;
            }
        }

        private static void WhitelistToggleCommandAction() { WhitelistManager.ToggleWhitelist(); }

        private static void ForceSaveCommandAction()
        {
            ServerClient toFind = Network.connectedClients.ToList().Find(x => x.username == parsedParameters[0]);

            if (toFind == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found", LogMode.Warning);
            else
            {
                CommandManager.SendForceSaveCommand(toFind);

                Logger.WriteToConsole($"User '{parsedParameters[0]}' has been forced to save",
                    LogMode.Warning);
            }
        }

        private static void DeletePlayerCommandAction()
        {
            UserFile userFile = UserManager.GetUserFileFromName(parsedParameters[0]);

            if (userFile == null) Logger.WriteToConsole($"[ERROR] > User '{parsedParameters[0]}' was not found", LogMode.Warning);
            else SaveManager.DeletePlayerData(userFile.username);
        }

        private static void EnableDifficultyCommandAction()
        {
            if (Master.difficultyValues.UseCustomDifficulty == true)
            {
                Logger.WriteToConsole($"[ERROR] > Custom difficulty was already enabled", LogMode.Warning);
            }

            else
            {
                Master.difficultyValues.UseCustomDifficulty = true;
                CustomDifficultyManager.SaveCustomDifficulty(Master.difficultyValues);

                Logger.WriteToConsole($"Custom difficulty is now enabled", LogMode.Warning);
            }
        }

        private static void DisableDifficultyCommandAction()
        {
            if (Master.difficultyValues.UseCustomDifficulty == false)
            {
                Logger.WriteToConsole($"[ERROR] > Custom difficulty was already disabled", LogMode.Warning);
            }

            else
            {
                Master.difficultyValues.UseCustomDifficulty = false;
                CustomDifficultyManager.SaveCustomDifficulty(Master.difficultyValues);

                Logger.WriteToConsole($"Custom difficulty is now disabled", LogMode.Warning);
            }
        }

        private static void ToggleCustomScenariosCommandAction()
        {
            Master.serverValues.AllowCustomScenarios = !Master.serverValues.AllowCustomScenarios;
            Logger.WriteToConsole($"Custom scenarios are now {(Master.serverValues.AllowCustomScenarios ? ("Enabled") : ("Disabled"))}", LogMode.Warning);
            Master.SaveServerValues(Master.serverValues);
        }

        private static void QuitCommandAction()
        {
            Master.isClosing = true;

            Logger.WriteToConsole($"Waiting for all saves to quit", LogMode.Warning);

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

            Logger.WriteToConsole("[Cleared console]", LogMode.Title);
        }

        private static void ToggleVerboseLogs()
        {
            Master.serverConfig.VerboseLogs = !Master.serverConfig.VerboseLogs;
            Logger.Warning($"Verbose Logs set to {Master.serverConfig.VerboseLogs}");
            Master.SaveServerConfig();
        }
    }
}