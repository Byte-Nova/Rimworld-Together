using RTServer.Files;
using RTServer.Hooks.ServerBrowser;
using RTServer.Hooks.Shared;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTShared.Commands;
using RTShared.Files;
using RTShared.Files.Configs;
using RTShared.Misc;
using RTNetwork.PacketManagers;
using RTServer.PacketManagers;
using RTShared.Files.Actions;
using static RTShared.Misc.Printer;
using RTShared.Files.Marketplace;

namespace RTServer.Core
{
    public static class Program
    {
        static void Main()
        {
            ServerPrinter.CreateLogger();
            CultureHandler.SetCulture();

            SetPaths();
            CreateFolders();
            LoadFiles();
            LoadActions();

            Printer.Title($"Server version {CommonValues.ExecutableVersion} ({CommonValues.HotfixVersion})");
            Printer.Title($"Loading all necessary resources");
            Printer.Title(Printer.SeparatorString);

            PM_Events.LoadAllEvents();
            Printer.Title(Printer.SeparatorString, Verbosity.Extreme);
            CMD_Base.GetAllCommands();
            Printer.Title(Printer.SeparatorString, Verbosity.Extreme);
            PM_Base.CacheAllPackets(PM_Base.AssemblyType.Server);
            Printer.Title(Printer.SeparatorString, Verbosity.Extreme);

            ServerNetwork.StartFeature();
            Task.Run(BackupManager.StartFeature);
            Task.Run(ServerBrowserManager.StartFeature);

            while (true) CMD_Base.ListenForCommands();
        }

        private static void SetPaths()
        {
            // Normal files
            FL_ServerConfig.SavePath = Path.Combine(Master.ConfigsPath, "ServerConfig.json");
            FL_PlanetConfig.SavePath = Path.Combine(Master.AssetsPath, "WorldValuesFile.json");
            FL_StorytellerConfig.SavePath = Path.Combine(Master.ConfigsPath, "StorytellerConfig.json");
            FL_ScenarioConfig.SavePath = Path.Combine(Master.ConfigsPath, "ScenarioConfig.json");
            FL_ModConfig.SavePath = Path.Combine(Master.ConfigsPath, "ModConfig.json");
            FL_DifficultyConfig.SavePath = Path.Combine(Master.ConfigsPath, "DifficultyConfig.json");
            FL_PasswordConfig.SavePath = Path.Combine(Master.ConfigsPath, "PasswordConfig.json");
            FL_BackupsConfig.SavePath = Path.Combine(Master.ConfigsPath, "BackupConfig.json");
            FL_ChatConfig.SavePath = Path.Combine(Master.ConfigsPath, "ChatConfig.json");
            FL_Leaderboard.SavePath = Path.Combine(Master.AssetsPath, "Leaderboard.json");
            FL_Market.SavePath = Path.Combine(Master.AssetsPath, "Market.json");
            FL_Road.SavePath = Path.Combine(Master.AssetsPath, "Roads.json");
            FL_River.SavePath = Path.Combine(Master.AssetsPath, "Rivers.json");
            FL_Guild.SavePath = Path.Combine(Master.GuildsPath);
            
            // Actions
            ACT_WorldObject.SavePath = Path.Combine(Master.ActionsPath, "WorldObject.json");
            ACT_Pollution.SavePath = Path.Combine(Master.ActionsPath, "Pollution.json");
            ACT_Raid.SavePath = Path.Combine(Master.ActionsPath, "Raid.json");
            ACT_Zoom.SavePath = Path.Combine(Master.ActionsPath, "Zoom.json");
            ACT_Event.SavePath = Path.Combine(Master.ActionsPath, "Event.json");
            ACT_Aid.SavePath = Path.Combine(Master.ActionsPath, "Aid.json");
            ACT_Market.SavePath = Path.Combine(Master.ActionsPath, "Market.json");
            ACT_Guild.SavePath = Path.Combine(Master.ActionsPath, "Guild.json");
            ACT_Trade.SavePath = Path.Combine(Master.ActionsPath, "Trade.json");
            ACT_Leaderboard.SavePath = Path.Combine(Master.ActionsPath, "Leaderboard.json");
            ACT_Caravan.SavePath = Path.Combine(Master.ActionsPath, "Caravan.json");
            ACT_Site.SavePath = Path.Combine(Master.ActionsPath, "Site.json");
            ACT_Road.SavePath = Path.Combine(Master.ActionsPath, "Road.json");
            ACT_River.SavePath = Path.Combine(Master.ActionsPath, "River.json");
            ACT_Scenario.SavePath =  Path.Combine(Master.ActionsPath, "Scenario.json");

            // Find a way to move these two to another place or merge with the above
            CommonValues.ServerUsersPath = Master.UsersPath;
            CommonValues.ServerSitesPath = Master.SitesPath;
        }

        private static void CreateFolders()
        {
            if (!Directory.Exists(Master.AssetsPath)) Directory.CreateDirectory(Master.AssetsPath);
            if (!Directory.Exists(Master.ConfigsPath)) Directory.CreateDirectory(Master.ConfigsPath);
            if (!Directory.Exists(Master.ActionsPath)) Directory.CreateDirectory(Master.ActionsPath);
            if (!Directory.Exists(Master.LogsPath)) Directory.CreateDirectory(Master.LogsPath);
            if (!Directory.Exists(Master.SystemLogsPath)) Directory.CreateDirectory(Master.SystemLogsPath);
            if (!Directory.Exists(Master.ChatLogsPath)) Directory.CreateDirectory(Master.ChatLogsPath);
            if (!Directory.Exists(Master.BackupsPath)) Directory.CreateDirectory(Master.BackupsPath);
            if (!Directory.Exists(Master.BackupUsersPath)) Directory.CreateDirectory(Master.BackupUsersPath);
            if (!Directory.Exists(Master.BackupServerPath)) Directory.CreateDirectory(Master.BackupServerPath);
            if (!Directory.Exists(Master.UsersPath)) Directory.CreateDirectory(Master.UsersPath);
            if (!Directory.Exists(Master.SavesPath)) Directory.CreateDirectory(Master.SavesPath);
            if (!Directory.Exists(Master.MapsPath)) Directory.CreateDirectory(Master.MapsPath);
            if (!Directory.Exists(Master.SitesPath)) Directory.CreateDirectory(Master.SitesPath);
            if (!Directory.Exists(Master.GuildsPath)) Directory.CreateDirectory(Master.GuildsPath);
            if (!Directory.Exists(Master.SettlementsPath)) Directory.CreateDirectory(Master.SettlementsPath);
            if (!Directory.Exists(Master.EventsPath)) Directory.CreateDirectory(Master.EventsPath);
            if (!Directory.Exists(Master.WorldObjectsPath)) Directory.CreateDirectory(Master.WorldObjectsPath);
        }

        private static void LoadFiles()
        {
            Master.ServerConfig = (FL_ServerConfig)FL_ServerConfig.Load<FL_ServerConfig>(FL_ServerConfig.SavePath);
            if (CommonValues.ExecutableVersion == "dev") Master.ServerConfig.EnableServerTelemetry = true;
            if (CommonValues.ExecutableVersion == "dev") Master.ServerConfig.EnableServerBrowser = true;
            if (CommonValues.ExecutableVersion == "dev") Master.ServerConfig.UseClientSave = false;
            FL_ServerConfig.Save(FL_ServerConfig.SavePath, Master.ServerConfig);

            Master.PasswordConfig = (FL_PasswordConfig)FL_PasswordConfig.Load<FL_PasswordConfig>(FL_PasswordConfig.SavePath);
            FL_PasswordConfig.Save(FL_PasswordConfig.SavePath, Master.PasswordConfig);

            Master.DifficultyValues = (FL_DifficultyConfig)FL_DifficultyConfig.Load<FL_DifficultyConfig>(FL_DifficultyConfig.SavePath);
            FL_DifficultyConfig.Save(FL_DifficultyConfig.SavePath, Master.DifficultyValues);

            Master.ScenarioValues = (FL_ScenarioConfig)FL_ScenarioConfig.Load<FL_ScenarioConfig>(FL_ScenarioConfig.SavePath);
            FL_ScenarioConfig.Save(FL_ScenarioConfig.SavePath, Master.ScenarioValues);

            Master.StorytellerValues = (FL_StorytellerConfig)FL_StorytellerConfig.Load<FL_StorytellerConfig>(FL_StorytellerConfig.SavePath);
            FL_StorytellerConfig.Save(FL_StorytellerConfig.SavePath, Master.StorytellerValues);

            Master.BackupConfig = (FL_BackupsConfig)FL_BackupsConfig.Load<FL_BackupsConfig>(FL_BackupsConfig.SavePath);
            FL_BackupsConfig.Save(FL_BackupsConfig.SavePath, Master.BackupConfig);

            Master.ModConfig = (FL_ModConfig)FL_ModConfig.Load<FL_ModConfig>(FL_ModConfig.SavePath);
            FL_ModConfig.Save(FL_ModConfig.SavePath, Master.ModConfig);

            Master.ChatConfig = (FL_ChatConfig)FL_ChatConfig.Load<FL_ChatConfig>(FL_ChatConfig.SavePath);
            FL_ChatConfig.Save(FL_ChatConfig.SavePath, Master.ChatConfig);

            Master.LeaderboardFile = (FL_Leaderboard)FL_Leaderboard.Load<FL_Leaderboard>(FL_Leaderboard.SavePath);
            FL_Leaderboard.Save(FL_Leaderboard.SavePath, Master.LeaderboardFile);

            Master.MarketFile = (FL_Market)FL_Market.Load<FL_Market>(FL_Market.SavePath);
            FL_Market.Save(FL_Market.SavePath, Master.MarketFile);
            
            Master.RoadFile = (FL_Road)FL_Road.Load<FL_Road>(FL_Road.SavePath);
            FL_Road.Save(FL_Road.SavePath, Master.RoadFile);
            
            Master.RiverFile = (FL_River)FL_River.Load<FL_River>(FL_River.SavePath);
            FL_River.Save(FL_River.SavePath, Master.RiverFile);

            // Don't automatically save this one
            // We require this file to be saved after a client upload
            Master.WorldValues = (FL_PlanetConfig)FL_PlanetConfig.Load<FL_PlanetConfig>(FL_PlanetConfig.SavePath, false);
        }

        private static void LoadActions()
        {
            Master.ActionConfigs = new FL_ActionsConfig();
            
            Master.ActionConfigs.WorldObjectAction = (ACT_WorldObject)ACT_WorldObject.Load<ACT_WorldObject>(ACT_WorldObject.SavePath);
            ACT_WorldObject.Save(ACT_WorldObject.SavePath, Master.ActionConfigs.WorldObjectAction);
            
            Master.ActionConfigs.PollutionAction = (ACT_Pollution)ACT_Pollution.Load<ACT_Pollution>(ACT_Pollution.SavePath);
            ACT_Pollution.Save(ACT_Pollution.SavePath, Master.ActionConfigs.PollutionAction);
            
            Master.ActionConfigs.RaidAction = (ACT_Raid)ACT_Raid.Load<ACT_Raid>(ACT_Raid.SavePath);
            ACT_Raid.Save(ACT_Raid.SavePath, Master.ActionConfigs.RaidAction);
            
            Master.ActionConfigs.ZoomAction = (ACT_Zoom)ACT_Zoom.Load<ACT_Zoom>(ACT_Zoom.SavePath);
            ACT_Zoom.Save(ACT_Zoom.SavePath, Master.ActionConfigs.ZoomAction);
            
            Master.ActionConfigs.EventAction = (ACT_Event)ACT_Event.Load<ACT_Event>(ACT_Event.SavePath);
            ACT_Event.Save(ACT_Event.SavePath, Master.ActionConfigs.EventAction);
            
            Master.ActionConfigs.AidAction = (ACT_Aid)ACT_Aid.Load<ACT_Aid>(ACT_Aid.SavePath);
            ACT_Aid.Save(ACT_Aid.SavePath, Master.ActionConfigs.AidAction);
            
            Master.ActionConfigs.MarketAction = (ACT_Market)ACT_Market.Load<ACT_Market>(ACT_Market.SavePath);
            ACT_Market.Save(ACT_Market.SavePath, Master.ActionConfigs.MarketAction);
            
            Master.ActionConfigs.GuildAction = (ACT_Guild)ACT_Guild.Load<ACT_Guild>(ACT_Guild.SavePath);
            ACT_Guild.Save(ACT_Guild.SavePath, Master.ActionConfigs.GuildAction);
            
            Master.ActionConfigs.TradeAction = (ACT_Trade)ACT_Trade.Load<ACT_Trade>(ACT_Trade.SavePath);
            ACT_Trade.Save(ACT_Trade.SavePath, Master.ActionConfigs.TradeAction);
            
            Master.ActionConfigs.LeaderboardAction = (ACT_Leaderboard)ACT_Leaderboard.Load<ACT_Leaderboard>(ACT_Leaderboard.SavePath);
            ACT_Leaderboard.Save(ACT_Leaderboard.SavePath, Master.ActionConfigs.LeaderboardAction);
            
            Master.ActionConfigs.CaravanAction = (ACT_Caravan)ACT_Caravan.Load<ACT_Caravan>(ACT_Caravan.SavePath);
            ACT_Caravan.Save(ACT_Caravan.SavePath, Master.ActionConfigs.CaravanAction);
            
            Master.ActionConfigs.SiteAction = (ACT_Site)ACT_Site.Load<ACT_Site>(ACT_Site.SavePath);
            ACT_Site.Save(ACT_Site.SavePath, Master.ActionConfigs.SiteAction);
            
            Master.ActionConfigs.RoadAction = (ACT_Road)ACT_Road.Load<ACT_Road>(ACT_Road.SavePath);
            ACT_Road.Save(ACT_Road.SavePath, Master.ActionConfigs.RoadAction);
            
            Master.ActionConfigs.RiverAction = (ACT_River)ACT_River.Load<ACT_River>(ACT_River.SavePath);
            ACT_River.Save(ACT_River.SavePath, Master.ActionConfigs.RiverAction);
            
            Master.ActionConfigs.ScenarioAction = (ACT_Scenario)ACT_Scenario.Load<ACT_Scenario>(ACT_Scenario.SavePath);
            ACT_Scenario.Save(ACT_Scenario.SavePath, Master.ActionConfigs.ScenarioAction);
        }
    }
}