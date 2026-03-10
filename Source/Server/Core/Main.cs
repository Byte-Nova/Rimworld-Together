using GameServer.Managers;
using Shared;
using Shared.Files.Actions;
using Shared.Files.Configs;
using Shared.Files.Configs.Mods;
using Shared.Files.Guilds;
using System.Globalization;
using System.Reflection;
using Shared.Misc;
using TCPNetwork.Misc;
using static Shared.CommonEnumerators;
using GameServer.Hooks.TCPNetwork;
using GameServer.Hooks.Shared;
using Shared.Files;

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
                Printer.Error("If this is your first time installing Rimworld Together, please take a look at our wiki > " +
                    "https://rimworldtogether.wiki.gg/");
            }

            LoadFiles();
            SetCulture();
            LoadResources();

            PacketCache.CacheAllPacketsInAppDomain(AssemblyType.Server);

            if (Master.BackupConfig.AutomaticBackups) Task.Run(BackupManager.AutoBackup);

            ServerBrowserManager.StartFeature();

            ServerNetwork _ = new ServerNetwork();
            // In Docker/non-interactive consoles, ListenForServerCommands() returns immediately.
            // Keep the main thread alive without spinning a core.
            ConsoleManager.ListenForServerCommands();
            Thread.Sleep(Timeout.Infinite);
        }

        public static void SetPaths()
        {
            ServerConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
            ActionsConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ActionConfig.json");
            PlanetConfigFile.SavePath = Path.Combine(Master.AssetsPath, "WorldValuesFile.json");
            StorytellerConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
            ScenarioConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
            ModsConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ModConfig.json");
            DifficultyConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
            ServerBrowserConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ServerBrowserConfig.json");
            WhitelistConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "WhitelistConfig.json");
            BackupsConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
            ChatConfigFile.SavePath = Path.Combine(Master.ConfigsPath, "ChatConfig.json");
            LeaderboardFile.SavePath = Path.Combine(Master.AssetsPath, "Leaderboard.json");

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
            Master.ServerConfig = (ServerConfigFile)ServerConfigFile.Load<ServerConfigFile>(ServerConfigFile.SavePath);
            Master.ActionConfigs = (ActionsConfigFile)ActionsConfigFile.Load<ActionsConfigFile>(ActionsConfigFile.SavePath);
            Master.Whitelist = (WhitelistConfigFile)WhitelistConfigFile.Load<WhitelistConfigFile>(WhitelistConfigFile.SavePath);
            Master.DifficultyValues = (DifficultyConfigFile)DifficultyConfigFile.Load<DifficultyConfigFile>(DifficultyConfigFile.SavePath);
            Master.ScenarioValues = (ScenarioConfigFile)ScenarioConfigFile.Load<ScenarioConfigFile>(ScenarioConfigFile.SavePath);
            Master.StorytellerValues = (StorytellerConfigFile)StorytellerConfigFile.Load<StorytellerConfigFile>(StorytellerConfigFile.SavePath);
            Master.BackupConfig = (BackupsConfigFile)BackupsConfigFile.Load<BackupsConfigFile>(BackupsConfigFile.SavePath);
            Master.ModConfig = (ModsConfigFile)ModsConfigFile.Load<ModsConfigFile>(ModsConfigFile.SavePath);
            Master.ChatConfig = (ChatConfigFile)ChatConfigFile.Load<ChatConfigFile>(ChatConfigFile.SavePath);
            Master.WorldValues = (PlanetConfigFile)PlanetConfigFile.Load<PlanetConfigFile>(PlanetConfigFile.SavePath, true, false);
            Master.ServerBrowserConfig = (ServerBrowserConfigFile)ServerBrowserConfigFile.Load<ServerBrowserConfigFile>(ServerBrowserConfigFile.SavePath);
            Master.LeaderboardFile = (LeaderboardFile)LeaderboardFile.Load<LeaderboardFile>(LeaderboardFile.SavePath);
        }

        public static void ChangeTitle()
        {
            Console.Title = $"RimWorld Together {CommonValues.ExecutableVersion} - " +
                $"Players [{ServerNetwork.GetConnectedClients().Length}/{Master.ServerConfig.MaxPlayers}]";
        }
    }
}
