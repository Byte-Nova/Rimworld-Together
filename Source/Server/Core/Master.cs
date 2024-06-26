using Shared;
using System;
using System.Globalization;
using System.IO;
using static Shared.CommonEnumerators;

namespace GameServer
{
    //Class with all the critical variables for the client to work

    public static class Master
    {
        //Paths

        public static string mainPath;
        public static string corePath;
        public static string mapsPath;
        public static string logsPath;
        public static string systemLogsPath;
        public static string chatLogsPath;
        public static string usersPath;
        public static string savesPath;
        public static string sitesPath;
        public static string factionsPath;
        public static string settlementsPath;
        public static string roadsPath;

        public static string backupsPath;
        public static string backupWorldPath;
        public static string backupUsersPath;

        public static string modsPath;
        public static string requiredModsPath;
        public static string optionalModsPath;
        public static string forbiddenModsPath;

        //Lists

        public static List<string> loadedRequiredMods = new List<string>();
        public static List<string> loadedOptionalMods = new List<string>();
        public static List<string> loadedForbiddenMods = new List<string>();

        //References

        public static MarketFile marketFile;
        public static WhitelistFile whitelist;
        public static SiteValuesFile siteValues;
        public static WorldValuesFile worldValues;
        public static EventValuesFile eventValues;
        public static ServerConfigFile serverConfig;
        public static ActionValuesFile actionValues;
        public static DifficultyValuesFile difficultyValues;
        public static RoadValuesFile roadValues;

        //Booleans

        public static bool isClosing;

        public static void Main()
        {
            Console.ForegroundColor = ConsoleColor.White;

            TryDisableQuickEdit();
            SetPaths();
            SetCulture();
            LoadResources();
            ChangeTitle();

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
            catch { };
        }

        public static void SetPaths()
        {
            mainPath = Directory.GetCurrentDirectory();
            corePath = Path.Combine(mainPath, "Core");
            mapsPath = Path.Combine(mainPath, "Maps");
            logsPath = Path.Combine(mainPath, "Logs");
            systemLogsPath = Path.Combine(logsPath, "System");
            chatLogsPath = Path.Combine(logsPath, "Chat");
            usersPath = Path.Combine(mainPath, "Users");
            savesPath = Path.Combine(mainPath, "Saves");
            sitesPath = Path.Combine(mainPath, "Sites");
            factionsPath = Path.Combine(mainPath, "Factions");
            settlementsPath = Path.Combine(mainPath, "Settlements");
            roadsPath = Path.Combine(mainPath, "Roads");

            backupsPath = Path.Combine(mainPath, "Backups");
            backupUsersPath = Path.Combine(backupsPath, "Users");
            backupWorldPath = Path.Combine(backupsPath, "Worlds");

            modsPath = Path.Combine(mainPath, "Mods");
            requiredModsPath = Path.Combine(modsPath, "Required");
            optionalModsPath = Path.Combine(modsPath, "Optional");
            forbiddenModsPath = Path.Combine(modsPath, "Forbidden");

            if (!Directory.Exists(corePath)) Directory.CreateDirectory(corePath);
            if (!Directory.Exists(usersPath)) Directory.CreateDirectory(usersPath);
            if (!Directory.Exists(savesPath)) Directory.CreateDirectory(savesPath);
            if (!Directory.Exists(mapsPath)) Directory.CreateDirectory(mapsPath);
            if (!Directory.Exists(logsPath)) Directory.CreateDirectory(logsPath);
            if (!Directory.Exists(systemLogsPath)) Directory.CreateDirectory(systemLogsPath);
            if (!Directory.Exists(chatLogsPath)) Directory.CreateDirectory(chatLogsPath);
            if (!Directory.Exists(sitesPath)) Directory.CreateDirectory(sitesPath);
            if (!Directory.Exists(factionsPath)) Directory.CreateDirectory(factionsPath);
            if (!Directory.Exists(settlementsPath)) Directory.CreateDirectory(settlementsPath);
            if (!Directory.Exists(roadsPath)) Directory.CreateDirectory(roadsPath);

            if (!Directory.Exists(backupsPath)) Directory.CreateDirectory(backupsPath);
            if (!Directory.Exists(backupUsersPath)) Directory.CreateDirectory(backupUsersPath);
            if (!Directory.Exists(backupWorldPath)) Directory.CreateDirectory(backupWorldPath);

            if (!Directory.Exists(modsPath)) Directory.CreateDirectory(modsPath);
            if (!Directory.Exists(requiredModsPath)) Directory.CreateDirectory(requiredModsPath);
            if (!Directory.Exists(optionalModsPath)) Directory.CreateDirectory(optionalModsPath);
            if (!Directory.Exists(forbiddenModsPath)) Directory.CreateDirectory(forbiddenModsPath);
        }

        private static void SetCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);

            Logger.Title($"Loading server culture > [{CultureInfo.CurrentCulture}]");
        }

        public static void LoadResources()
        {
            Logger.Title($"Loading version {CommonValues.executableVersion}");
            Logger.Title($"Loading all necessary resources");
            Logger.Title($"----------------------------------------");

            LoadValueFile(ServerLoadMode.Configs);
            LoadValueFile(ServerLoadMode.Actions);
            LoadValueFile(ServerLoadMode.Sites);
            LoadValueFile(ServerLoadMode.Events);
            LoadValueFile(ServerLoadMode.Roads);
            ModManager.LoadMods();
            WorldManager.LoadWorldFile();
            WhitelistManager.LoadServerWhitelist();
            CustomDifficultyManager.LoadCustomDifficulty();
            MarketManager.LoadMarketStock();

            Logger.Title($"----------------------------------------");
        }

        public static void SaveServerConfig()
        {
            string path = Path.Combine(corePath, "ServerConfig.json");

            Serializer.SerializeToFile(path, serverConfig);

            Logger.Warning("Saved server Config");
        }

        private static void LoadValueFile(ServerLoadMode loadMode)
        {
            string pathToLoad = "";

            switch(loadMode)
            {
                case ServerLoadMode.Configs:
                    pathToLoad = Path.Combine(corePath, "ServerConfig.json");
                    if (File.Exists(pathToLoad)) serverConfig = Serializer.SerializeFromFile<ServerConfigFile>(pathToLoad);
                    else
                    {
                        serverConfig = new ServerConfigFile();
                        Serializer.SerializeToFile(pathToLoad, serverConfig);
                    }
                    break;

                case ServerLoadMode.Actions:
                    pathToLoad = Path.Combine(corePath, "ActionValues.json");
                    if (File.Exists(pathToLoad)) actionValues = Serializer.SerializeFromFile<ActionValuesFile>(pathToLoad);
                    else
                    {
                        actionValues = new ActionValuesFile();
                        Serializer.SerializeToFile(pathToLoad, actionValues);
                    }
                    break;

                case ServerLoadMode.Sites:
                    pathToLoad = Path.Combine(corePath, "SiteValues.json");
                    if (File.Exists(pathToLoad)) siteValues = Serializer.SerializeFromFile<SiteValuesFile>(pathToLoad);
                    else
                    {
                        siteValues = new SiteValuesFile();
                        Serializer.SerializeToFile(pathToLoad, siteValues);
                    }
                    break;

                case ServerLoadMode.Events:
                    pathToLoad = Path.Combine(corePath, "EventValues.json");
                    if (File.Exists(pathToLoad)) eventValues = Serializer.SerializeFromFile<EventValuesFile>(pathToLoad);
                    else
                    {
                        eventValues = new EventValuesFile();
                        Serializer.SerializeToFile(pathToLoad, eventValues);
                    }
                    break;

                case ServerLoadMode.Roads:
                    pathToLoad = Path.Combine(corePath, "RoadValues.json");
                    if (File.Exists(pathToLoad)) roadValues = Serializer.SerializeFromFile<RoadValuesFile>(pathToLoad);
                    else
                    {
                        roadValues = new RoadValuesFile();
                        Serializer.SerializeToFile(pathToLoad, roadValues);
                    }
                    break;
            }

            Logger.Warning($"Loaded '{pathToLoad}'");
        }

        public static void ChangeTitle()
        {
            Console.Title = $"Rimworld Together {CommonValues.executableVersion} - " +
                $"Players [{Network.connectedClients.Count}/{serverConfig.MaxPlayers}]";
        }
    }
}
