using System.Globalization;
using GameServer.Core.Configs;
using GameServer.Managers;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Core
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
            MethodGatherer.CacheAllMethods(MethodGatherer.AssemblyType.Server);

            Printer.Title($"----------------------------------------");

            Threader.GenerateServerThread(Threader.ServerMode.Start);
            Threader.GenerateServerThread(Threader.ServerMode.Console);
            if (Master.ActionConfigs.EnableSites) SiteManager.UpdateAllSiteInfo();
            if (Master.BackupConfig.AutomaticBackups) Threader.GenerateServerThread(Threader.ServerMode.Backup);
            ServerBrowserManager.StartLoops();
            while (true) Thread.Sleep(1);
        }

        public static void SetPaths()
        {
            Master.MainPath = Directory.GetCurrentDirectory();
            Master.ConfigsPath = Path.Combine(Master.MainPath, "Configs");
            Master.TempPath = Path.Combine(Master.MainPath, "Temp");

            Master.AssetsPath = Path.Combine(Master.MainPath, "Assets");
            Master.MapsPath = Path.Combine(Master.AssetsPath, "Maps");
            Master.UsersPath = Path.Combine(Master.AssetsPath, "Users");
            Master.SavesPath = Path.Combine(Master.AssetsPath, "Saves");
            Master.SitesPath = Path.Combine(Master.AssetsPath, "Sites");
            Master.FactionsPath = Path.Combine(Master.AssetsPath, "Factions");
            Master.SettlementsPath = Path.Combine(Master.AssetsPath, "Settlements");
            Master.EventsPath = Path.Combine(Master.AssetsPath, "Events");
            Master.CompatibilityPatchesPath = Path.Combine(Master.AssetsPath, "Patches");

            Master.LogsPath = Path.Combine(Master.MainPath, "Logs");
            Master.SystemLogsPath = Path.Combine(Master.LogsPath, "System");
            Master.ChatLogsPath = Path.Combine(Master.LogsPath, "Chat");

            Master.BackupsPath = Path.Combine(Master.MainPath, "Backups");
            Master.BackupUsersPath = Path.Combine(Master.BackupsPath, "Users");
            Master.BackupServerPath = Path.Combine(Master.BackupsPath, "Servers");

            if (!Directory.Exists(Master.AssetsPath)) Directory.CreateDirectory(Master.AssetsPath);
            if (!Directory.Exists(Master.ConfigsPath)) Directory.CreateDirectory(Master.ConfigsPath);
            if (!Directory.Exists(Master.LogsPath)) Directory.CreateDirectory(Master.LogsPath);
            if (!Directory.Exists(Master.BackupsPath)) Directory.CreateDirectory(Master.BackupsPath);
            if (!Directory.Exists(Master.TempPath)) Directory.CreateDirectory(Master.TempPath);

            if (!Directory.Exists(Master.UsersPath)) Directory.CreateDirectory(Master.UsersPath);
            if (!Directory.Exists(Master.SavesPath)) Directory.CreateDirectory(Master.SavesPath);
            if (!Directory.Exists(Master.MapsPath)) Directory.CreateDirectory(Master.MapsPath);
            if (!Directory.Exists(Master.SystemLogsPath)) Directory.CreateDirectory(Master.SystemLogsPath);
            if (!Directory.Exists(Master.ChatLogsPath)) Directory.CreateDirectory(Master.ChatLogsPath);
            if (!Directory.Exists(Master.SitesPath)) Directory.CreateDirectory(Master.SitesPath);
            if (!Directory.Exists(Master.FactionsPath)) Directory.CreateDirectory(Master.FactionsPath);
            if (!Directory.Exists(Master.SettlementsPath)) Directory.CreateDirectory(Master.SettlementsPath);
            if (!Directory.Exists(Master.EventsPath)) Directory.CreateDirectory(Master.EventsPath);

            if (!Directory.Exists(Master.BackupUsersPath)) Directory.CreateDirectory(Master.BackupUsersPath);
            if (!Directory.Exists(Master.BackupServerPath)) Directory.CreateDirectory(Master.BackupServerPath);

            if (!Directory.Exists(Master.CompatibilityPatchesPath)) Directory.CreateDirectory(Master.CompatibilityPatchesPath);
        }

        private static void SetCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);

            Printer.Title($"Server culture > [{CultureInfo.CurrentCulture}]");
        }

        public static void LoadResources()
        {
            Printer.Title($"Server version {CommonValues.ExecutableVersion}");
            Printer.Title($"Loading all necessary resources");
            Printer.Title($"----------------------------------------");

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

            LoadValueFile(ServerFileMode.Scenario);
            SaveValueFile(ServerFileMode.Scenario, false);

            LoadValueFile(ServerFileMode.Storyteller);
            SaveValueFile(ServerFileMode.Storyteller, false);

            LoadValueFile(ServerFileMode.Backup);
            SaveValueFile(ServerFileMode.Backup, false);

            LoadValueFile(ServerFileMode.Mods);
            SaveValueFile(ServerFileMode.Mods, false);

            LoadValueFile(ServerFileMode.Chat);
            SaveValueFile(ServerFileMode.Chat, false);

            LoadValueFile(ServerFileMode.World);

            EventManager.LoadEvents();
        }

        public static void SaveValueFile(ServerFileMode mode, bool broadcast = true)
        {
            string pathToSave = "";

            switch (mode)
            {
                case ServerFileMode.Configs:
                    pathToSave = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.ServerConfig);
                    break;

                case ServerFileMode.Actions:
                    pathToSave = Path.Combine(Master.ConfigsPath, "ActionConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.ActionConfigs);
                    break;

                case ServerFileMode.Sites:
                    pathToSave = Path.Combine(Master.ConfigsPath, "SiteConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.SiteValues);
                    break;

                case ServerFileMode.Roads:
                    pathToSave = Path.Combine(Master.ConfigsPath, "RoadConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.RoadValues);
                    break;

                case ServerFileMode.World:
                    pathToSave = Path.Combine(Master.ConfigsPath, "WorldConfig.json");
                    Serializer.ObjectBytesToFile(pathToSave, Master.WorldValues);
                    break;

                case ServerFileMode.Whitelist:
                    pathToSave = Path.Combine(Master.ConfigsPath, "WhitelistConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.Whitelist);
                    break;

                case ServerFileMode.Difficulty:
                    pathToSave = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.DifficultyValues);
                    break;

                case ServerFileMode.Scenario:
                    pathToSave = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.ScenarioValues);
                    break;

                case ServerFileMode.Storyteller:
                    pathToSave = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.StorytellerValues);
                    break;

                case ServerFileMode.Backup:
                    pathToSave = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.BackupConfig);
                    break;

                case ServerFileMode.Mods:
                    pathToSave = Path.Combine(Master.ConfigsPath, "ModConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.ModConfig);
                    break;

                case ServerFileMode.Chat:
                    pathToSave = Path.Combine(Master.ConfigsPath, "ChatConfig.json");
                    Serializer.SerializeToFile(pathToSave, Master.ChatConfig);
                    break;
            }

            if (broadcast) InformationDisplayer.DisplaySaveFile(pathToSave);
        }

        public static void LoadValueFile(ServerFileMode mode, bool broadcast = true)
        {
            string pathToLoad = "";

            switch (mode)
            {
                case ServerFileMode.Configs:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
                    if (File.Exists(pathToLoad)) Master.ServerConfig = Serializer.SerializeFromFile<ServerConfigFile>(pathToLoad);
                    else
                    {
                        Master.ServerConfig = new ServerConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.ServerConfig);
                    }
                    break;

                case ServerFileMode.Actions:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "ActionConfig.json");
                    if (File.Exists(pathToLoad)) Master.ActionConfigs = Serializer.SerializeFromFile<ActionValuesFile>(pathToLoad);
                    else
                    {
                        Master.ActionConfigs = new ActionValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.ActionConfigs);
                    }
                    break;

                case ServerFileMode.Sites:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "SiteConfig.json");
                    if (File.Exists(pathToLoad)) Master.SiteValues = Serializer.SerializeFromFile<SiteValuesFile>(pathToLoad);
                    else
                    {
                        Master.SiteValues = new SiteValuesFile();
                        SiteManagerHelper.SetSitePresets();
                        Serializer.SerializeToFile(pathToLoad, Master.SiteValues);
                    }
                    break;

                case ServerFileMode.Roads:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "RoadConfig.json");
                    if (File.Exists(pathToLoad)) Master.RoadValues = Serializer.SerializeFromFile<RoadValuesFile>(pathToLoad);
                    else
                    {
                        Master.RoadValues = new RoadValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.RoadValues);
                    }
                    break;

                case ServerFileMode.World:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "WorldConfig.json");
                    if (File.Exists(pathToLoad)) Master.WorldValues = Serializer.FileBytesToObject<WorldValuesFile>(pathToLoad);
                    else return;
                    break;

                case ServerFileMode.Whitelist:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "WhitelistConfig.json");
                    if (File.Exists(pathToLoad)) Master.Whitelist = Serializer.SerializeFromFile<WhitelistConfigFile>(pathToLoad);
                    else
                    {
                        Master.Whitelist = new WhitelistConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.Whitelist);
                    }
                    break;

                case ServerFileMode.Difficulty:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
                    if (File.Exists(pathToLoad)) Master.DifficultyValues = Serializer.SerializeFromFile<DifficultyValuesFile>(pathToLoad);
                    else
                    {
                        Master.DifficultyValues = new DifficultyValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.DifficultyValues);
                    }
                    break;

                case ServerFileMode.Scenario:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
                    if (File.Exists(pathToLoad)) Master.ScenarioValues = Serializer.SerializeFromFile<ScenarioValuesFile>(pathToLoad);
                    else
                    {
                        Master.ScenarioValues = new ScenarioValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.ScenarioValues);
                    }
                    break;

                case ServerFileMode.Storyteller:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
                    if (File.Exists(pathToLoad)) Master.StorytellerValues = Serializer.SerializeFromFile<StorytellerValuesFile>(pathToLoad);
                    else
                    {
                        Master.StorytellerValues = new StorytellerValuesFile();
                        Serializer.SerializeToFile(pathToLoad, Master.StorytellerValues);
                    }
                    break;

                case ServerFileMode.Backup:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
                    if (File.Exists(pathToLoad)) Master.BackupConfig = Serializer.SerializeFromFile<BackupConfigFile>(pathToLoad);
                    else
                    {
                        Master.BackupConfig = new BackupConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.BackupConfig);
                    }
                    break;

                case ServerFileMode.Mods:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "ModConfig.json");
                    if (File.Exists(pathToLoad)) Master.ModConfig = Serializer.SerializeFromFile<ModConfigFile>(pathToLoad);
                    else
                    {
                        Master.ModConfig = new ModConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.ModConfig);
                    }
                    break;

                case ServerFileMode.Chat:
                    pathToLoad = Path.Combine(Master.ConfigsPath, "ChatConfig.json");
                    if (File.Exists(pathToLoad)) Master.ChatConfig = Serializer.SerializeFromFile<ChatConfigFile>(pathToLoad);
                    else
                    {
                        Master.ChatConfig = new ChatConfigFile();
                        Serializer.SerializeToFile(pathToLoad, Master.ChatConfig);
                    }
                    break;
            }

            if (broadcast) InformationDisplayer.DisplayLoadFile(pathToLoad);
        }

        public static void ChangeTitle()
        {
            Console.Title = $"RimWorld Together {CommonValues.ExecutableVersion} - " +
                $"Players [{NetworkHelper.GetConnectedClientsSafe().Length}/{Master.ServerConfig.MaxPlayers}]";
        }
    }
}