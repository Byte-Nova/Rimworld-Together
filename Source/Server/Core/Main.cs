using System.Globalization;
using System.Reflection;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class Main_
    {
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.White;

            SetPaths();
            SetCulture();
            LoadResources();
            ChangeTitle();
            TryDisableQuickEdit();

            Logger.Title($"----------------------------------------");

            if (Master.discordConfig.Enabled) DiscordManager.StartDiscordIntegration();
            if (Master.backupConfig.AutomaticBackups) BackupManager.AutoBackup();
            if (Master.actionValues.EnableSites) SiteManager.UpdateAllSiteInfo();

            Threader.GenerateServerThread(Threader.ServerMode.Start);
            Threader.GenerateServerThread(Threader.ServerMode.Console);

            while (true) Thread.Sleep(1);
        }

        private static void TryDisableQuickEdit()
        {
            try
            {
                QuickEdit quickEdit = new QuickEdit();
                quickEdit.DisableQuickEdit();
            }
            catch { Logger.Warning("Quick edit could not be disabled, ignore this if you are running on Linux"); }
        }

        public static void SetPaths()
        {
            Master.mainPath = Directory.GetCurrentDirectory();
            Master.corePath = Path.Combine(Master.mainPath, "Core");
            Master.mapsPath = Path.Combine(Master.mainPath, "Maps");
            Master.logsPath = Path.Combine(Master.mainPath, "Logs");
            Master.systemLogsPath = Path.Combine(Master.logsPath, "System");
            Master.chatLogsPath = Path.Combine(Master.logsPath, "Chat");
            Master.usersPath = Path.Combine(Master.mainPath, "Users");
            Master.savesPath = Path.Combine(Master.mainPath, "Saves");
            Master.sitesPath = Path.Combine(Master.mainPath, "Sites");
            Master.factionsPath = Path.Combine(Master.mainPath, "Factions");
            Master.settlementsPath = Path.Combine(Master.mainPath, "Settlements");
            Master.caravansPath = Path.Combine(Master.mainPath, "Caravans");
            Master.eventsPath = Path.Combine(Master.mainPath, "Events");

            Master.backupsPath = Path.Combine(Master.mainPath, "Backups");
            Master.backupUsersPath = Path.Combine(Master.backupsPath, "Users");
            Master.backupServerPath = Path.Combine(Master.backupsPath, "Servers");

            Master.compatibilityPatchesPath = Path.Combine(Master.mainPath, "Patches");

            if (!Directory.Exists(Master.corePath)) Directory.CreateDirectory(Master.corePath);
            if (!Directory.Exists(Master.usersPath)) Directory.CreateDirectory(Master.usersPath);
            if (!Directory.Exists(Master.savesPath)) Directory.CreateDirectory(Master.savesPath);
            if (!Directory.Exists(Master.mapsPath)) Directory.CreateDirectory(Master.mapsPath);
            if (!Directory.Exists(Master.logsPath)) Directory.CreateDirectory(Master.logsPath);
            if (!Directory.Exists(Master.systemLogsPath)) Directory.CreateDirectory(Master.systemLogsPath);
            if (!Directory.Exists(Master.chatLogsPath)) Directory.CreateDirectory(Master.chatLogsPath);
            if (!Directory.Exists(Master.sitesPath)) Directory.CreateDirectory(Master.sitesPath);
            if (!Directory.Exists(Master.factionsPath)) Directory.CreateDirectory(Master.factionsPath);
            if (!Directory.Exists(Master.settlementsPath)) Directory.CreateDirectory(Master.settlementsPath);
            if (!Directory.Exists(Master.caravansPath)) Directory.CreateDirectory(Master.caravansPath);
            if (!Directory.Exists(Master.eventsPath)) Directory.CreateDirectory(Master.eventsPath);

            if (!Directory.Exists(Master.backupsPath)) Directory.CreateDirectory(Master.backupsPath);
            if (!Directory.Exists(Master.backupUsersPath)) Directory.CreateDirectory(Master.backupUsersPath);
            if (!Directory.Exists(Master.backupServerPath)) Directory.CreateDirectory(Master.backupServerPath);

            if (!Directory.Exists(Master.compatibilityPatchesPath)) Directory.CreateDirectory(Master.compatibilityPatchesPath);
        }

        private static void SetCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);

            Logger.Title($"Server culture > [{CultureInfo.CurrentCulture}]");
        }

        public static void LoadResources()
        {
            Logger.Title($"Server version {CommonValues.executableVersion}");
            Logger.Title($"Loading all necessary resources");
            Logger.Title($"----------------------------------------");

            LoadValueFile(ServerFileMode.Configs);
            SaveValueFile(ServerFileMode.Configs, false);

            LoadValueFile(ServerFileMode.Actions);
            SaveValueFile(ServerFileMode.Actions, false);

            LoadValueFile(ServerFileMode.Sites);
            SaveValueFile(ServerFileMode.Sites, false);

            LoadValueFile(ServerFileMode.Roads);
            SaveValueFile(ServerFileMode.Roads, false);

            LoadValueFile(ServerFileMode.Whitelist);
            SaveValueFile(ServerFileMode.Whitelist, false);

            LoadValueFile(ServerFileMode.Difficulty);
            SaveValueFile(ServerFileMode.Difficulty, false);

            LoadValueFile(ServerFileMode.Discord);
            SaveValueFile(ServerFileMode.Discord, false);

            LoadValueFile(ServerFileMode.Backup);
            SaveValueFile(ServerFileMode.Backup, false);

            LoadValueFile(ServerFileMode.Mods);
            
            LoadValueFile(ServerFileMode.Chat);
            SaveValueFile(ServerFileMode.Chat, false);

            LoadValueFile(ServerFileMode.World);

            EventManager.LoadEvents();

            CompatibilityManager.LoadAllPatchedAssemblies();
        }

        public static void SaveValueFile(ServerFileMode mode, bool broadcast = true)
        {
            string pathToSave = "";

            switch (mode)
            {
                case ServerFileMode.Configs:
                    pathToSave = Path.Combine(Master.corePath, "ServerConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.serverConfig);
                    break;

                case ServerFileMode.Actions:
                    pathToSave = Path.Combine(Master.corePath, "ActionValues.json");
                    Serializer.SerializeToFile(pathToSave, Master.actionValues);
                    break;

                case ServerFileMode.Sites:
                    pathToSave = Path.Combine(Master.corePath, "SiteValues.json");
                    Serializer.SerializeToFile(pathToSave, Master.siteValues);
                    break;

                case ServerFileMode.Roads:
                    pathToSave = Path.Combine(Master.corePath, "RoadValues.json");
                    Serializer.SerializeToFile(pathToSave, Master.roadValues);
                    break;

                case ServerFileMode.World:
                    pathToSave = Path.Combine(Master.corePath, "WorldValues.json");
                    Serializer.SerializeToFile(pathToSave, Master.worldValues);
                    break;

                case ServerFileMode.Whitelist:
                    pathToSave = Path.Combine(Master.corePath, "Whitelist.json");
                    Serializer.SerializeToFile(pathToSave, Master.whitelist);
                    break;

                case ServerFileMode.Difficulty:
                    pathToSave = Path.Combine(Master.corePath, "DifficultyValues.json");
                    Serializer.SerializeToFile(pathToSave, Master.difficultyValues);
                    break;

                case ServerFileMode.Discord:
                    pathToSave = Path.Combine(Master.corePath, "DiscordConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.discordConfig);
                    break;

                case ServerFileMode.Backup:
                    pathToSave = Path.Combine(Master.corePath, "BackupConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.backupConfig);
                    break;

                case ServerFileMode.Mods:
                    pathToSave = Path.Combine(Master.corePath, "ModConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.modConfig);
                    break;
                case ServerFileMode.Chat:
                    pathToSave = Path.Combine(Master.corePath, "ChatConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.chatConfig);
                    break;
            }

            if (broadcast) Logger.Warning($"Saved > '{pathToSave}'");
        }

        public static void LoadValueFile(ServerFileMode mode, bool broadcast = true)
        {
            string pathToLoad = "";

            switch(mode)
            {
                case ServerFileMode.Configs:
                    pathToLoad = Path.Combine(Master.corePath, "ServerConfig.json");
                    if (File.Exists(pathToLoad)) Master.serverConfig = Serializer.SerializeFromFile<ServerConfigFile>(pathToLoad);
                    else
                    {
                        Master.serverConfig = new ServerConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.serverConfig);
                    }
                    break;

                case ServerFileMode.Actions:
                    pathToLoad = Path.Combine(Master.corePath, "ActionValues.json");
                    if (File.Exists(pathToLoad)) Master.actionValues = Serializer.SerializeFromFile<ActionValuesFile>(pathToLoad);
                    else
                    {
                        Master.actionValues = new ActionValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.actionValues);
                    }
                    break;

                case ServerFileMode.Sites:
                    pathToLoad = Path.Combine(Master.corePath, "SiteValues.json");
                    if (File.Exists(pathToLoad)) Master.siteValues = Serializer.SerializeFromFile<SiteValuesFile>(pathToLoad);
                    else
                    {
                        Master.siteValues = new SiteValuesFile();
                        SiteManagerHelper.SetSitePresets();
                        Serializer.SerializeToFile(pathToLoad, Master.siteValues);
                    }
                    break;

                case ServerFileMode.Roads:
                    pathToLoad = Path.Combine(Master.corePath, "RoadValues.json");
                    if (File.Exists(pathToLoad)) Master.roadValues = Serializer.SerializeFromFile<RoadValuesFile>(pathToLoad);
                    else
                    {
                        Master.roadValues = new RoadValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.roadValues);
                    }
                    break;

                case ServerFileMode.World:
                    pathToLoad = Path.Combine(Master.corePath, "WorldValues.json");
                    if (File.Exists(pathToLoad)) Master.worldValues = Serializer.SerializeFromFile<WorldValuesFile>(pathToLoad);
                    else Logger.Warning("World is missing. Join server to create it");
                    break;

                case ServerFileMode.Whitelist:
                    pathToLoad = Path.Combine(Master.corePath, "Whitelist.json");
                    if (File.Exists(pathToLoad)) Master.whitelist = Serializer.SerializeFromFile<WhitelistFile>(pathToLoad);
                    else
                    {
                        Master.whitelist = new WhitelistFile();
                        Serializer.SerializeToFile(pathToLoad, Master.whitelist);
                    }
                    break;

                case ServerFileMode.Difficulty:
                    pathToLoad = Path.Combine(Master.corePath, "DifficultyValues.json");
                    if (File.Exists(pathToLoad)) Master.difficultyValues = Serializer.SerializeFromFile<DifficultyValuesFile>(pathToLoad);
                    else
                    {
                        Master.difficultyValues = new DifficultyValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.difficultyValues);
                    }
                    break;

                case ServerFileMode.Discord:
                    pathToLoad = Path.Combine(Master.corePath, "DiscordConfig.json");
                    if (File.Exists(pathToLoad)) Master.discordConfig = Serializer.SerializeFromFile<DiscordConfigFile>(pathToLoad);
                    else
                    {
                        Master.discordConfig = new DiscordConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.discordConfig);
                    }
                    break;

                case ServerFileMode.Backup:
                    pathToLoad = Path.Combine(Master.corePath, "BackupConfig.json");
                    if (File.Exists(pathToLoad)) Master.backupConfig = Serializer.SerializeFromFile<BackupConfigFile>(pathToLoad);
                    else
                    {
                        Master.backupConfig = new BackupConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.backupConfig);
                    }
                    break;

                case ServerFileMode.Mods:
                    pathToLoad = Path.Combine(Master.corePath, "ModConfig.json");
                    if (File.Exists(pathToLoad)) Master.modConfig = Serializer.SerializeFromFile<ModConfigFile>(pathToLoad);
                    else
                    {
                        Master.modConfig = new ModConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.modConfig);
                    }
                    break;
                
                case ServerFileMode.Chat:
                    pathToLoad = Path.Combine(Master.corePath, "ChatConfig.json");
                    if (File.Exists(pathToLoad)) Master.chatConfig = Serializer.SerializeFromFile<ChatConfigFile>(pathToLoad);
                    else
                    {
                        Master.chatConfig = new ChatConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.chatConfig);
                    }
                    break;
            }
            if (broadcast) Logger.Warning($"Loaded > '{pathToLoad}'");
        }

        public static void ChangeTitle()
        {
            Console.Title = $"RimWorld Together {CommonValues.executableVersion} - " +
                $"Players [{Network.connectedClients.Count}/{Master.serverConfig.MaxPlayers}]";
        }
    }
}