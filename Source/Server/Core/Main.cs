using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            CheckForServerName();
            MethodGatherer.CacheAllMethods(MethodGatherer.AssemblyType.Server);

            Printer.Title($"----------------------------------------");

            // Initialize Discord integration if enabled
            if (Master.DiscordConfig != null && Master.DiscordConfig.Enabled)
            {
                Printer.Message("Initializing Discord integration...");
                DiscordManager.InitializeAsync().GetAwaiter().GetResult();
            }

            Threader.GenerateServerThread(Threader.ServerMode.Start);
            Threader.GenerateServerThread(Threader.ServerMode.Console);
            if (Master.ActionConfigs.EnableSites) SiteManager.UpdateAllSiteInfo();
            if (Master.BackupConfig.AutomaticBackups) Threader.GenerateServerThread(Threader.ServerMode.Backup);
            ServerBrowserManager.StartLoops();
            while (true) Thread.Sleep(1);
        }

        public static void SetPaths()
        {
            Master.MainPath  = Directory.GetCurrentDirectory();
            Master.ConfigsPath = Path.Combine(Master.MainPath, "Configs");
            Master.TempPath    = Path.Combine(Master.MainPath, "Temp");

            Master.AssetsPath = Path.Combine(Master.MainPath, "Assets");
            Master.MapsPath   = Path.Combine(Master.AssetsPath, "Maps");
            Master.UsersPath  = Path.Combine(Master.AssetsPath, "Users");
            Master.SavesPath  = Path.Combine(Master.AssetsPath, "Saves");
            Master.SitesPath  = Path.Combine(Master.AssetsPath, "Sites");
            Master.FactionsPath     = Path.Combine(Master.AssetsPath, "Factions");
            Master.SettlementsPath  = Path.Combine(Master.AssetsPath, "Settlements");
            Master.EventsPath       = Path.Combine(Master.AssetsPath, "Events");
            Master.CompatibilityPatchesPath = Path.Combine(Master.AssetsPath, "Patches");

            Master.LogsPath        = Path.Combine(Master.MainPath, "Logs");
            Master.SystemLogsPath  = Path.Combine(Master.LogsPath, "System");
            Master.ChatLogsPath    = Path.Combine(Master.LogsPath, "Chat");

            Master.BackupsPath       = Path.Combine(Master.MainPath, "Backups");
            Master.BackupUsersPath   = Path.Combine(Master.BackupsPath, "Users");
            Master.BackupServerPath  = Path.Combine(Master.BackupsPath, "Servers");

            // create missing dirs (unchanged logic)
            string[] paths =
            {
                Master.AssetsPath, Master.ConfigsPath, Master.LogsPath, Master.BackupsPath, Master.TempPath,
                Master.UsersPath,  Master.SavesPath,  Master.MapsPath, Master.SystemLogsPath, Master.ChatLogsPath,
                Master.SitesPath,  Master.FactionsPath, Master.SettlementsPath, Master.EventsPath,
                Master.CompatibilityPatchesPath, Master.BackupUsersPath, Master.BackupServerPath
            };
            foreach (var p in paths) if (!Directory.Exists(p)) Directory.CreateDirectory(p);
        }

        private static void SetCulture()
        {
            CultureInfo.CurrentCulture            = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture          = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture     = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture   = new CultureInfo("en-US", false);

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
            string path = string.Empty;

            switch (mode)
            {
                case ServerFileMode.Configs:
                    path = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
                    Serializer.SerializeToFile(path, Master.ServerConfig);

                    var discordPath = Path.Combine(Master.ConfigsPath, "DiscordConfig.json");
                    Serializer.SerializeToFile(discordPath, Master.DiscordConfig);
                    break;

                case ServerFileMode.Actions:
                    path = Path.Combine(Master.ConfigsPath, "ActionConfig.json");
                    Serializer.SerializeToFile(path, Master.ActionConfigs);
                    break;

                case ServerFileMode.Sites:
                    path = Path.Combine(Master.ConfigsPath, "SiteConfig.json");
                    Serializer.SerializeToFile(path, Master.SiteValues);
                    break;

                case ServerFileMode.Roads:
                    path = Path.Combine(Master.ConfigsPath, "RoadConfig.json");
                    Serializer.SerializeToFile(path, Master.RoadValues);
                    break;

                case ServerFileMode.World:
                    path = Path.Combine(Master.ConfigsPath, "WorldConfig.json");
                    Serializer.ObjectBytesToFile(path, Master.WorldValues);
                    break;

                case ServerFileMode.Whitelist:
                    path = Path.Combine(Master.ConfigsPath, "WhitelistConfig.json");
                    Serializer.SerializeToFile(path, Master.Whitelist);
                    break;

                case ServerFileMode.Difficulty:
                    path = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
                    Serializer.SerializeToFile(path, Master.DifficultyValues);
                    break;

                case ServerFileMode.Scenario:
                    path = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
                    Serializer.SerializeToFile(path, Master.ScenarioValues);
                    break;

                case ServerFileMode.Storyteller:
                    path = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
                    Serializer.SerializeToFile(path, Master.StorytellerValues);
                    break;

                case ServerFileMode.Backup:
                    path = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
                    Serializer.SerializeToFile(path, Master.BackupConfig);
                    break;

                case ServerFileMode.Mods:
                    path = Path.Combine(Master.ConfigsPath, "ModConfig.json");
                    Serializer.SerializeToFile(path, Master.ModConfig);
                    break;

                case ServerFileMode.Chat:
                    path = Path.Combine(Master.ConfigsPath, "ChatConfig.json");
                    Serializer.SerializeToFile(path, Master.ChatConfig);
                    break;
            }

            if (broadcast) InformationDisplayer.DisplaySaveFile(path);
        }

        public static void LoadValueFile(ServerFileMode mode, bool broadcast = true)
        {
            string path = string.Empty;

            switch (mode)
            {
                case ServerFileMode.Configs:
                    path = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
                    Master.ServerConfig = File.Exists(path)
                        ? Serializer.SerializeFromFile<ServerConfigFile>(path)
                        : new ServerConfigFile();

                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.ServerConfig);

                    // Discord
                    var dPath = Path.Combine(Master.ConfigsPath, "DiscordConfig.json");
                    Master.DiscordConfig = File.Exists(dPath)
                        ? Serializer.SerializeFromFile<DiscordConfigFile>(dPath)
                        : new DiscordConfigFile();
                    if (!File.Exists(dPath)) Serializer.SerializeToFile(dPath, Master.DiscordConfig);
                    break;

                case ServerFileMode.Actions:
                    path = Path.Combine(Master.ConfigsPath, "ActionConfig.json");
                    Master.ActionConfigs = File.Exists(path)
                        ? Serializer.SerializeFromFile<ActionValuesFile>(path)
                        : new ActionValuesFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.ActionConfigs);
                    break;

                case ServerFileMode.Sites:
                    path = Path.Combine(Master.ConfigsPath, "SiteConfig.json");
                    Master.SiteValues = File.Exists(path)
                        ? Serializer.SerializeFromFile<SiteValuesFile>(path)
                        : new SiteValuesFile();
                    if (!File.Exists(path))
                    {
                        SiteManagerHelper.SetSitePresets();
                        Serializer.SerializeToFile(path, Master.SiteValues);
                    }
                    break;

                case ServerFileMode.Roads:
                    path = Path.Combine(Master.ConfigsPath, "RoadConfig.json");
                    Master.RoadValues = File.Exists(path)
                        ? Serializer.SerializeFromFile<RoadValuesFile>(path)
                        : new RoadValuesFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.RoadValues);
                    break;

                case ServerFileMode.World:
                    path = Path.Combine(Master.ConfigsPath, "WorldConfig.json");
                    if (File.Exists(path)) Master.WorldValues = Serializer.FileBytesToObject<WorldValuesFile>(path);
                    break;

                case ServerFileMode.Whitelist:
                    path = Path.Combine(Master.ConfigsPath, "WhitelistConfig.json");
                    Master.Whitelist = File.Exists(path)
                        ? Serializer.SerializeFromFile<WhitelistConfigFile>(path)
                        : new WhitelistConfigFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.Whitelist);
                    break;

                case ServerFileMode.Difficulty:
                    path = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
                    Master.DifficultyValues = File.Exists(path)
                        ? Serializer.SerializeFromFile<DifficultyValuesFile>(path)
                        : new DifficultyValuesFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.DifficultyValues);
                    break;

                case ServerFileMode.Scenario:
                    path = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
                    Master.ScenarioValues = File.Exists(path)
                        ? Serializer.SerializeFromFile<ScenarioValuesFile>(path)
                        : new ScenarioValuesFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.ScenarioValues);
                    break;

                case ServerFileMode.Storyteller:
                    path = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
                    Master.StorytellerValues = File.Exists(path)
                        ? Serializer.SerializeFromFile<StorytellerValuesFile>(path)
                        : new StorytellerValuesFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.StorytellerValues);
                    break;

                case ServerFileMode.Backup:
                    path = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
                    Master.BackupConfig = File.Exists(path)
                        ? Serializer.SerializeFromFile<BackupConfigFile>(path)
                        : new BackupConfigFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.BackupConfig);
                    break;

                case ServerFileMode.Mods:
                    path = Path.Combine(Master.ConfigsPath, "ModConfig.json");
                    Master.ModConfig = File.Exists(path)
                        ? Serializer.SerializeFromFile<ModConfigFile>(path)
                        : new ModConfigFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.ModConfig);
                    break;

                case ServerFileMode.Chat:
                    path = Path.Combine(Master.ConfigsPath, "ChatConfig.json");
                    Master.ChatConfig = File.Exists(path)
                        ? Serializer.SerializeFromFile<ChatConfigFile>(path)
                        : new ChatConfigFile();
                    if (!File.Exists(path)) Serializer.SerializeToFile(path, Master.ChatConfig);
                    break;
            }

            if (broadcast) InformationDisplayer.DisplayLoadFile(path);
        }

        public static void ChangeTitle()
        {
            Console.Title =
                $"RimWorld Together {CommonValues.ExecutableVersion} - Players [{NetworkHelper.GetConnectedClientsSafe().Length}/{Master.ServerConfig.MaxPlayers}]";
        }

        private static void CheckForServerName()
        {
            if (!StringChecker.CheckIfStringValid(Master.ServerConfig.Name))
            {
                // placeholder for additional logic
            }
        }
    }
}