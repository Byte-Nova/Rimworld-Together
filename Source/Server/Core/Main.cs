using GameServer.Managers;
using GameServer.Integrations.Discord;
using Shared;
using Shared.Files.Actions;
using Shared.Files.Configs;
using Shared.Files.Configs.Mods;
using Shared.Files.Guilds;
using System.Globalization;
using System.Reflection;
using Shared.Misc;
using static Shared.CommonEnumerators;

namespace GameServer.Core
{
    public static class Main_
    {
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.White;
            ServerPrinter.CreateLogger();
            SetPaths();

            if (!File.Exists(ServerConfigFile.SavePath))
            {
                Printer.Error("If this is your first time installing Rimworld Together, please take a look around the configuration files " +
                    "and our wiki > https://github.com/RimWorld-Together/Rimworld-Together/wiki");
            }

            LoadFiles();
            SetCulture();
            LoadResources();

            MethodGatherer.CacheAllMethods(MethodGatherer.AssemblyType.Server);

            if (Master.BackupConfig.AutomaticBackups) Task.Run(BackupManager.AutoBackup);

            ServerBrowserManager.StartFeature();

            ServerNetwork _ = new ServerNetwork();

            DiscordBridge.TryStart();

            while (true) ConsoleManager.ListenForServerCommands();
        }

        public static void SetPaths()
        {
            ServerConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
            ActionsConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ActionConfig.json");
            PlanetConfigFile.SavePath = Path.Combine(Master.WorldPath, "WorldValuesFile.json");
            StorytellerConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
            ScenarioConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
            ModsConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ModConfig.json");
            DifficultyConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
            ServerBrowserConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ServerBrowserConfig.json");
            WhitelistConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "WhitelistConfig.json");
            BackupsConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
            ChatConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ChatConfig.json");

            CommonValues.ServerUsersPath = Master.UsersPath;
            CommonValues.ServerSitesPath = Master.SitesPath;

            GuildFile.SavePath = Path.Combine(Master.GuildsPath);

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
            if (!Directory.Exists(Master.GuildsPath)) Directory.CreateDirectory(Master.GuildsPath);
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
            Printer.Warning($"{GC.GetTotalAllocatedBytes() / 1024 / 1024}MB in allocation after resource loading", LogImportanceMode.Verbose);
            Printer.Title($"----------------------------------------", LogImportanceMode.Verbose);
        }

        private static void LoadFiles()
        {
            Master.ServerConfig = (ServerConfigFile)ServerConfigFile.Load<ServerConfigFile>();
            Master.ActionConfigs = (ActionsConfigFile)ActionsConfigFile.Load<ActionsConfigFile>();
            Master.Whitelist = (WhitelistConfigFile)WhitelistConfigFile.Load<WhitelistConfigFile>();
            Master.DifficultyValues = (DifficultyConfigFile)DifficultyConfigFile.Load<DifficultyConfigFile>();
            Master.ScenarioValues = (ScenarioConfigFile)ScenarioConfigFile.Load<ScenarioConfigFile>();
            Master.StorytellerValues = (StorytellerConfigFile)StorytellerConfigFile.Load<StorytellerConfigFile>();
            Master.BackupConfig = (BackupsConfigFile)BackupsConfigFile.Load<BackupsConfigFile>();
            Master.ModConfig = (ModsConfigFile)ModsConfigFile.Load<ModsConfigFile>();
            Master.ChatConfig = (ChatConfigFile)ChatConfigFile.Load<ChatConfigFile>();
            Master.WorldValues = (PlanetConfigFile)PlanetConfigFile.Load<PlanetConfigFile>();
            Master.ServerBrowserConfig = (ServerBrowserConfigFile)ServerBrowserConfigFile.Load<ServerBrowserConfigFile>();
        }

        public static void ChangeTitle()
        {
            Console.Title = $"RimWorld Together {CommonValues.ExecutableVersion} - " +
                $"Players [{ServerNetwork.Instance.GetConnectedClientsSafe().Length}/{Master.ServerConfig.MaxPlayers}]";
        }
    }
}