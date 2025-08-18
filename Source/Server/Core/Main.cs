using GameServer.Managers;
using GameServer.Misc;
using Shared;
using Shared.Files;
using System.Globalization;
using System.Reflection;
using static Shared.CommonEnumerators;

namespace GameServer.Core
{
    public static class Main_
    {
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.White;

            SetPaths();
            LoadFiles();
            SetCulture();
            LoadResources();

            Validator.CheckIfFirstBoot();
            MethodGatherer.CacheAllMethods(MethodGatherer.AssemblyType.Server);

            if (Master.ActionConfigs.EnableSites) SiteManager.UpdateAllSiteInfo();

            ServerNetwork _ = new ServerNetwork();

            if (Master.BackupConfig.AutomaticBackups) Threader.GenerateServerThread(Threader.ServerMode.Backup);
            ServerBrowserManager.StartLoops();

            while (true) ConsoleManager.ListenForServerCommands();
        }

        public static void SetPaths()
        {
            ServerConfigFile.Path = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
            ActionValuesFile.Path = Path.Combine(Master.ConfigsPath, "ActionConfig.json");
            WorldValuesFile.Path = Path.Combine(Master.WorldPath, "WorldValuesFile.json");
            StorytellerValuesFile.Path = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
            SiteValuesFile.Path = Path.Combine(Master.ConfigsPath, "SiteConfig.json");
            ScenarioValuesFile.Path = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
            RoadValuesFile.Path = Path.Combine(Master.ConfigsPath, "RoadConfig.json");
            ModConfigFile.Path = Path.Combine(Master.ConfigsPath, "ModConfig.json");
            DifficultyValuesFile.Path = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
            ServerBrowserConfig.Path = Path.Combine(Master.ConfigsPath, "ServerBrowserConfig.json");
            WhitelistConfigFile.Path = Path.Combine(Master.ConfigsPath, "WhitelistConfig.json");
            BackupConfigFile.Path = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
            ChatConfigFile.Path = Path.Combine(Master.ConfigsPath, "ChatConfig.json");

            if (!Directory.Exists(Master.AssetsPath)) Directory.CreateDirectory(Master.AssetsPath);
            if (!Directory.Exists(Master.ConfigsPath)) Directory.CreateDirectory(Master.ConfigsPath);

            if (!Directory.Exists(Master.LogsPath)) Directory.CreateDirectory(Master.LogsPath);
            if (!Directory.Exists(Master.SystemLogsPath)) Directory.CreateDirectory(Master.SystemLogsPath);
            if (!Directory.Exists(Master.ChatLogsPath)) Directory.CreateDirectory(Master.ChatLogsPath);
            if (!Directory.Exists(Master.BackupsPath)) Directory.CreateDirectory(Master.BackupsPath);
            if (!Directory.Exists(Master.BackupUsersPath)) Directory.CreateDirectory(Master.BackupUsersPath);
            if (!Directory.Exists(Master.BackupServerPath)) Directory.CreateDirectory(Master.BackupServerPath);

            if (!Directory.Exists(Master.TempPath)) Directory.CreateDirectory(Master.TempPath);
            if (!Directory.Exists(Master.UsersPath)) Directory.CreateDirectory(Master.UsersPath);
            if (!Directory.Exists(Master.SavesPath)) Directory.CreateDirectory(Master.SavesPath);
            if (!Directory.Exists(Master.MapsPath)) Directory.CreateDirectory(Master.MapsPath);
            if (!Directory.Exists(Master.SitesPath)) Directory.CreateDirectory(Master.SitesPath);
            if (!Directory.Exists(Master.FactionsPath)) Directory.CreateDirectory(Master.FactionsPath);
            if (!Directory.Exists(Master.SettlementsPath)) Directory.CreateDirectory(Master.SettlementsPath);
            if (!Directory.Exists(Master.EventsPath)) Directory.CreateDirectory(Master.EventsPath);
            if (!Directory.Exists(Master.WorldPath)) Directory.CreateDirectory(Master.WorldPath);
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

            LoadFiles();

            EventManagerH.LoadAllEvents();
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Printer.Warning($"{GC.GetTotalAllocatedBytes() / 1024 / 1024}MB after resource loading", LogImportanceMode.Verbose);
        }

        private static void LoadFiles()
        {
            Master.ServerConfig = (ServerConfigFile)ServerConfigFile.Load<ServerConfigFile>();
            Master.ActionConfigs = (ActionValuesFile)ActionValuesFile.Load<ActionValuesFile>();
            Master.SiteValues = (SiteValuesFile)SiteValuesFile.Load<SiteValuesFile>();
            Master.RoadValues = (RoadValuesFile)RoadValuesFile.Load<RoadValuesFile>();
            Master.Whitelist = (WhitelistConfigFile)WhitelistConfigFile.Load<WhitelistConfigFile>();
            Master.DifficultyValues = (DifficultyValuesFile)DifficultyValuesFile.Load<DifficultyValuesFile>();
            Master.ScenarioValues = (ScenarioValuesFile)ScenarioValuesFile.Load<ScenarioValuesFile>();
            Master.StorytellerValues = (StorytellerValuesFile)StorytellerValuesFile.Load<StorytellerValuesFile>();
            Master.BackupConfig = (BackupConfigFile)BackupConfigFile.Load<BackupConfigFile>();
            Master.ModConfig = (ModConfigFile)ModConfigFile.Load<ModConfigFile>();
            Master.ChatConfig = (ChatConfigFile)ChatConfigFile.Load<ChatConfigFile>();
            Master.WorldValues = (WorldValuesFile)WorldValuesFile.Load<WorldValuesFile>();
            Master.ServerBrowserConfig = (ServerBrowserConfig)ServerBrowserConfig.Load<ServerBrowserConfig>();
        }

        public static void ChangeTitle()
        {
            Console.Title = $"RimWorld Together {CommonValues.ExecutableVersion} - " +
                $"Players [{ServerNetwork.Instance.GetConnectedClientsSafe().Length}/{Master.ServerConfig.MaxPlayers}]";
        }
    }
}