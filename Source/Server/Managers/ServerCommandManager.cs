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
            public string parameterDescription;
            public int parameterCount;
            public Action commandAction;

            public Command(string command, int parameterCount, string description, string parameterDescription, Action commandAction)
            {
                this.command = command;
                this.parameterCount = parameterCount;
                this.description = description;
                this.parameterDescription = parameterDescription;
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

        private static string EventList(int tabCount)
        {
            string tab = string.Empty;
            for (int i = 0; i < tabCount; i++) tab += "  ";

            string eventList = string.Empty;
            eventList += tab + "Available events:";
            foreach (string eventType in eventTypes)
            {
                eventList += "\n" + tab + eventType;
            }
            return eventList;
        }

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
                else if (commandToFetch.parameterCount > parsedParameterCount)
                {
                    Logger.Warning($"Command '{commandToFetch.command}' needs at minimum [{commandToFetch.parameterCount}] parameters but was passed [{parsedParameterCount}]");
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
                        //Do command
                        if (commandHistoryPosition != 0) commandHistory[0] = commandHistory[commandHistoryPosition];
                        if (commandHistory.Count() >= 20) commandHistory.RemoveAt(commandHistory.Count() - 1);

                        Logger.ClearCurrentLine();
                        Logger.Message(commandHistory[0]);

                        commandHistory.Insert(0, "");
                        commandHistoryPosition = 0;

                        ParseServerCommands(commandHistory[1]);
                        continue;

                    case ConsoleKey.Backspace:
                        if (commandHistoryPosition != 0)
                        {
                            commandHistory[0] = commandHistory[commandHistoryPosition];
                            commandHistoryPosition = 0;
                        }
                        if (commandHistory[0].Count() > 0) commandHistory[0] = commandHistory[0].Substring(0, commandHistory[0].Count() - 1);
                        break;

                    case ConsoleKey.LeftArrow:
                        break;

                    case ConsoleKey.RightArrow:
                        break;

                    case ConsoleKey.UpArrow:
                        //increment up through the command history
                        if (commandHistoryPosition != commandHistory.Count() - 1)
                            commandHistoryPosition++;
                        break;

                    case ConsoleKey.DownArrow:
                        //increment down through the command history
                        if (commandHistoryPosition != 0)
                            commandHistoryPosition--;
                        break;

                    case ConsoleKey.Tab:
                        //Auto complete unfinished command
                        if (tabbedCommands.Count() > 0)
                        {
                            tabbedCommandsIndex++;
                            if (tabbedCommandsIndex >= tabbedCommands.Count())
                            {
                                tabbedCommandsIndex = 0;
                            }
                            commandHistory[0] = tabbedCommands[tabbedCommandsIndex];
                        }

                        else
                        {
                            tabbedCommands = commandDictionary.Keys.ToList().FindAll(x => x.StartsWith(commandHistory[0], StringComparison.OrdinalIgnoreCase)).ToList();
                            if (tabbedCommands.Count() > 0) commandHistory[0] = tabbedCommands[0];
                        }
                        break;

                    default:
                        if (commandHistoryPosition != 0) {
                            commandHistory[0] = commandHistory[commandHistoryPosition];
                            commandHistoryPosition = 0;
                        }
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
                "Provides help information for server commands",
                "help [] - shows a list of all available commands to use\n\nhelp [Command]\n  Command - Gives help for the given command",
                HelpCommandAction)
            },

            {
                "list", new Command("list", 0,
                "Shows all connected players",
                "list [] - shows all connected players",
                ListCommandAction)
            },

            {
                "op", new Command("op", 1,
                "Gives admin privileges to the selected player",
                "op [Player]\n  Player - makes the given player an Admin",
                OpCommandAction)
            },

            {
                "deop", new Command("deop", 1,
                "Removes admin privileges from the selected player",
                "deop [Player]\n  Player - removes Admin privileges from the given player",
                DeopCommandAction)
            },

            {
                "kick", new Command("kick", 1,
                "Kicks the selected player from the server",
                "kick [Player]\n  Player - kicks the given player from the server without saving their game",
                KickCommandAction)
            },

            {
                "ban", new Command("ban", 1,
                "Bans the selected player from the server",
                "ban [Player]\n  Player - bans the given player from the server",
                BanCommandAction)
            },

            {
                "pardon", new Command("pardon", 1,
                "Pardons the selected player from the server",
                "pardon [Player]\n  Player - unbans the given player",
                PardonCommandAction)
            },

            {
                "deeplist", new Command("deeplist", 0,
                "Shows a list of all server players",
                "deeplist [] - shows a list of all server players",
                DeepListCommandAction)
            },

            {
                "banlist", new Command("banlist", 0,
                "Shows a list of all banned server players",
                "banlist [] - shows a list of all banned players",
                BanListCommandAction)
            },

            {
                "reload", new Command("reload", 0,
                "Reloads all server resources",
                "reload [] - reloads all server resources",
                ReloadCommandAction)
            },

            {
                "modlist", new Command("modlist", 0,
                "Shows all currently loaded mods",
                "modlist [] - shows all currently loaded mods",
                ModListCommandAction)
            },

            {
                "dositerewards", new Command("dositerewards", 0,
                "Forces site rewards to run",
                "dositerewards [] - Forces site rewards to run",
                DoSiteRewardsCommandAction)
            },

            {
                "sendevent", new Command("sendevent", 2,
                "Sends a command to the selecter players",
                "sendevent [Player] [Event]\n  Player - sends an event to the given player\n  Event - the event to send to a player\n" + EventList(2) ,
                EventCommandAction)
            },

            {
                "sendeventall", new Command("sendeventall", 1,
                "Sends a command to all connected players",
                "sendeventall [Event]\n  Event - sends the given event to all players\n" + EventList(2),
                EventAllCommandAction)
            },

            {
                "eventlist", new Command("eventlist", 0,
                "Shows a list of all available events to use",
                "eventlist [] - shows a list of all avaialable events to use",
                EventListCommandAction)
            },

            {
                "broadcast", new Command("broadcast", -1,
                "Broadcast a message to all connected players",
                "broadcast [Message]\n  Message - sends a message as a notification to all players",
                BroadcastCommandAction)
            },

            {
                "chat", new Command("chat", -1,
                "Send a message in chat from the Server",
                "chat [Message]\n  Message - sends a chat message under the name 'Server'",
                ServerMessageCommandAction)
            },

            {
                "clear", new Command("clear", 0,
                "Clears the console output",
                "clear [] - clears the console",
                ClearCommandAction)
            },

            {
                "whitelist", new Command("whitelist", 0,
                "Shows all whitelisted players",
                "whitelist [] - shows all whitelisted players",
                WhitelistCommandAction)
            },

            {
                "whitelistadd", new Command("whitelistadd", 1,
                "Adds a player to the whitelist",
                "whitelistadd [Player]\n  Player - adds the given player to the whitelist",
                WhitelistAddCommandAction)
            },

            {
                "whitelistremove", new Command("whitelistremove", 1,
                "Removes a player from the whitelist",
                "whitelistremove [Player]\n  Player - removes the given player from the whitelist",
                WhitelistRemoveCommandAction)
            },

            {
                "togglewhitelist", new Command("togglewhitelist", 0,
                "Toggles the whitelist ON or OFF",
                "togglewhitelist - toggles the whitelist On or OFF\n\ntogglewhitelist [Value]" +
                    "\n  Value - sets the whitelist to the given value\n    valid values: true, false",
                WhitelistToggleCommandAction)
            },

            {
                "forcesave", new Command("forcesave", 1,
                "Forces a player to sync their save",
                "forcesave [Player]\n  Player - forces the given player to save their game",
                ForceSaveCommandAction)
            },

            {
                "deleteplayer", new Command("deleteplayer", 1,
                "Deletes all data of a player",
                "deleteplayer [Player]\n  Player - will delete the save of the given player, but will not delete the user",
                DeletePlayerCommandAction)
            },

            {
                "enabledifficulty", new Command("enabledifficulty", 0,
                "Enables custom difficulty in the server",
                "enabledifficulty [] - enables custom difficulty in the server.\nPlayers will be forced to use a difficulty setting made by the server",
                EnableDifficultyCommandAction)
            },

            {
                "disabledifficulty", new Command("disabledifficulty", 0,
                "Disables custom difficulty in the server",
                "disabledifficulty [] - disables custom difficulty in the server.\nPlayers will be allowed to use their own choice in difficulty",
                DisableDifficultyCommandAction)
            },

            {
                "togglecustomscenarios", new Command("togglecustomscenarios", 0,
                "enables/disables custom scenarios on the server",
                "togglecustomscenarios [] - enables/disables custom scenarios on the server\n\ntogglecustomscenarios [Value]" +
                    "\n  Value - sets custom scenarios to the given value\n    valid values: true, false",
                ToggleCustomScenariosCommandAction)
            },

            {
                "toggleupnp", new Command("toggleupnp", 0,
                "enables/disables UPnP port mapping (auto-portforwarding)",
                "toggleupnp [] - enables/disables UPnP port mapping (auto-portforwarding)\n\ntoggleupnp [Value]" +
                    "\n  Value - sets the UPnP status to the given value\n    valid values: true, false",
                ToggleUPnPCommandAction)
            },

            {
                "portforward", new Command("portforward", 0,
                "will attempt use UPnP to portforward the server",
                "portforward [] - will attempt to use UPnP (Universal Plug n' Play) to try and portforward the server",
                PortForwardCommandAction)
            },

            {
                "quit", new Command("quit", 0,
                "Saves all player details and then closes the server",
                "quit [] - saves all player details then closes the server",
                QuitCommandAction)
            },

            {
                "forcequit", new Command("forcequit", 0,
                "Closes the server without saving player details",
                "forcequit [] - closes the server without saving player details",
                ForceQuitCommandAction) 
            },

            {
                "toggleverboselogs", new Command("toggleverboselogs",0,
                "enables/disables verbose logs",
                "toggleverboselogs - enables/disables verbose logs\n\ntoggleverboselogs [Value]" +
                    "\n  Value - sets verbose logs to the given value\n    valid values: true, false",
                ToggleVerboseLogs)
            }
        };

        private static void HelpCommandAction()
        {
            switch (parsedParameters.Count())
            {
                case 0:
                    Logger.WriteToConsole($"List of available commands: [{commandDictionary.Count()}]", LogMode.Title, false, false);
                    Logger.WriteToConsole("----------------------------------------", LogMode.Title, false);
                    foreach (Command command in commandDictionary.Values)
                    {
                        Logger.WriteToConsole($"{command.command} - {command.description}", LogMode.Warning, writeToLogs: false, false);
                    }
                    Logger.WriteToConsole("----------------------------------------", LogMode.Title, false, false);
                    Logger.WriteToConsole($"Write 'help [Command]' to get help for the given command", displayTime:false);
                    break;
                case 1:

                    if (!commandDictionary.ContainsKey(parsedParameters[0])){
                        Logger.WriteToConsole($"Error: Could not find the command {parsedParameters[0]}");
                        return;
                    }
                    Logger.WriteToConsole("----------------------------------------", LogMode.Title, false, false, false);
                    Command givenCommand = commandDictionary[parsedParameters[0]];
                    Logger.WriteToConsole($"{givenCommand.command}", LogMode.Warning, displayTime: false);
                    Logger.WriteToConsole($"\n{givenCommand.parameterDescription}", LogMode.Warning, displayTime: false);
                    Logger.WriteToConsole("----------------------------------------", LogMode.Title, false, false, false);
                    break;
            }
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
            if (parsedParameters.Count() == 0)
            {
                Master.serverValues.AllowCustomScenarios = !Master.serverValues.AllowCustomScenarios;
            } 
            else if (parsedParameters.Count() == 1)
            {
                bool value;
                try
                {
                    value = bool.Parse(parsedParameters[0].ToLower());
                }catch {
                    Logger.WriteToConsole($"{parsedParameters[0]} is not a valid parameter", LogMode.Error);
                    return;
                }
                if (Master.serverValues.AllowCustomScenarios == value)
                {
                    Logger.WriteToConsole($"Custom scenarios are already set to {value}", LogMode.Error);
                    return;
                }
                Master.serverValues.AllowCustomScenarios = value;
            }
            Logger.WriteToConsole($"Custom scenarios are now {(Master.serverValues.AllowCustomScenarios ? ("Enabled") : ("Disabled"))}", LogMode.Warning);
            Master.SaveServerValues(Master.serverValues);
        }

        private static void ToggleUPnPCommandAction()
        {
            if (parsedParameters.Count() == 0)
            {
                Master.serverConfig.UseUPnP = !Master.serverConfig.UseUPnP;
            }
            else if (parsedParameters.Count() == 1)
            {
                bool value;
                try
                {
                     value = bool.Parse(parsedParameters[0]);
                }
                catch
                {
                    Logger.WriteToConsole($"{parsedParameters[0]} is not a valid parameter", LogMode.Error);
                    return;
                }
                if (Master.serverConfig.UseUPnP == value)
                {
                    Logger.WriteToConsole($"UPnP is already set to {value}", LogMode.Error);
                    return;
                }
                Master.serverConfig.UseUPnP = value;
            }
            Logger.WriteToConsole($"UPnP port mapping is now {(Master.serverConfig.UseUPnP ? ("Enabled") : ("Disabled"))}", LogMode.Warning);

            Master.SaveServerConfig(Master.serverConfig);

            if (Master.serverConfig.UseUPnP)
            {
                portforwardQuestion:
                Logger.WriteToConsole("You have enabled UPnP on the server. Would you like to portforward?", LogMode.Warning);
                Logger.WriteToConsole("Please type 'YES' or 'NO'", LogMode.Warning);

                string response = Console.ReadLine();

                if (response == "YES") _ = new UPnP();

                else if (response == "NO")
                {
                    Logger.WriteToConsole("You can use the command 'portforward' in the future to portforward the server", 
                        LogMode.Warning);
                }

                else
                {
                    Logger.WriteToConsole("The response you have entered is not a valid option. Please make sure your response is capitalized", LogMode.Error);
                    goto portforwardQuestion;
                }
            }

            else
            {
                Logger.WriteToConsole("If a port has already been forwarded using UPnP, it will continute to be active until the server is restarted", 
                    LogMode.Warning);
            }
        }

        private static void PortForwardCommandAction()
        {
            if (!Master.serverConfig.UseUPnP)
            {
                Logger.WriteToConsole("Cannot portforward because UPnP is disabled on the server. You can use the command 'toggleupnp' to enable it.", 
                    LogMode.Error);
            }
            else _ = new UPnP();
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

        //May be a bit messy, but it's trying its best (;_;) 
        private static void ResetWorldCommandAction()
        {
            //Make sure the user wants to reset the world
            Logger.WriteToConsole("Are you sure you want to reset the world?", LogMode.Warning);
            Logger.WriteToConsole("Please type 'YES' or 'NO'", LogMode.Warning);
            deleteWorldQuestion:

            string response = Console.ReadLine();

            if (response == "NO") return;
            else if (response != "YES") 
            {
                Logger.WriteToConsole($"{response} is not a valid option; The options must be capitalized", LogMode.Error);
                goto deleteWorldQuestion;
            }

            //Get the name of the new folder for the world
            Logger.WriteToConsole("The current world will be saved in the 'ArchivedWorlds' folder.\n" +
                                  "Would you like to name the world before it is moved?\n" +
                                  "If not, the world will be named with the current date", LogMode.Warning);
            Logger.WriteToConsole("Please type 'YES' or 'NO'", LogMode.Warning);
            nameWorldQuestion:

            response = Console.ReadLine();
            string newWorldFolderPath;
            string newWorldFolderName;

            if (response == "YES")
            {
                customName:
                Console.WriteLine("Please enter the name you would like to use:", LogMode.Warning);
                newWorldFolderName = Console.ReadLine();
                newWorldFolderPath = $"{Master.archivedWorldPath + Path.DirectorySeparatorChar}{newWorldFolderName}";

                try { if (!Directory.Exists($"{newWorldFolderPath}")) Directory.CreateDirectory($"{newWorldFolderPath}"); }
                catch
                {
                    Logger.WriteToConsole("The name you entered is invalid.\n" +
                        " Please make sure your name does not contain any of these sybols:\n" +
                        "\\/*:<>?\"|", LogMode.Error);

                    goto customName;
                }
            }

            else if (response == "NO")
            {
                newWorldFolderName = $"World-{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}";
                newWorldFolderPath = $"{Master.archivedWorldPath + Path.DirectorySeparatorChar}{newWorldFolderName}";
                if (!Directory.Exists($"{newWorldFolderPath}")) Directory.CreateDirectory($"{newWorldFolderPath}");
            }

            else
            {
                Logger.WriteToConsole($"{response} is not a valid option; The options must be capitalized", LogMode.Error);
                goto nameWorldQuestion;
            }

            //Make the new folder and move all the current world folders to it
            Logger.WriteToConsole($"The archived world will be saved as:\n{newWorldFolderPath}", LogMode.Warning);
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
            if (Directory.Exists(Master.logsPath)) Directory.Move(Master.logsPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Logs");
            if (Directory.Exists(Master.mapsPath)) Directory.Move(Master.mapsPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Maps");
            if (Directory.Exists(Master.savesPath)) Directory.Move(Master.savesPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Saves");
            if (Directory.Exists(Master.settlementsPath)) Directory.Move(Master.settlementsPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Settlements");
            if (Directory.Exists(Master.sitesPath)) Directory.Move(Master.sitesPath, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Sites");

            Master.SetPaths();

            Logger.WriteToConsole("World has been successfully reset and archived", LogMode.Warning);
        }
    }
}