using Shared;
using System.Globalization;
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
        public static string archivedWorldPath;

        public static string modsPath;
        public static string requiredModsPath;
        public static string optionalModsPath;
        public static string forbiddenModsPath;

        //Lists

        public static List<string> loadedRequiredMods = new List<string>();
        public static List<string> loadedOptionalMods = new List<string>();
        public static List<string> loadedForbiddenMods = new List<string>();

        //References

        public static WhitelistFile whitelist;
        public static SiteValuesFile siteValues;
        public static WorldValuesFile worldValues;
        public static EventValuesFile eventValues;
        public static ServerConfigFile serverConfig;
        public static ServerValuesFile serverValues;
        public static ActionValuesFile actionValues;
        public static DifficultyValuesFile difficultyValues;

        //Booleans

        public static bool isClosing;

        public static void Main()
        {
            Console.ForegroundColor = ConsoleColor.White;

            TryDisablyQuickEdit();
            SetPaths();
            SetCulture();
            LoadResources();
            ChangeTitle();

            Threader.GenerateServerThread(Threader.ServerMode.Start);
            Threader.GenerateServerThread(Threader.ServerMode.Console);

            while (true) Thread.Sleep(1);
        }

        private static void TryDisablyQuickEdit()
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
            archivedWorldPath = Path.Combine(mainPath, "ArchivedWorlds");

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
            if (!Directory.Exists(archivedWorldPath)) Directory.CreateDirectory(archivedWorldPath);

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

            ConsoleManager.WriteToConsole($"Loading server culture > [{CultureInfo.CurrentCulture}]", LogMode.Title);
        }

        public static void LoadResources()
        {
            ConsoleManager.WriteToConsole($"Loading version {CommonValues.executableVersion}", LogMode.Title);
            ConsoleManager.WriteToConsole($"Loading all necessary resources", LogMode.Title);
            ConsoleManager.WriteToConsole($"----------------------------------------", LogMode.Title);

            LoadSiteValues();
            LoadEventValues();
            LoadServerConfig();
            LoadServerValues();
            LoadActionValues();
            ModManager.LoadMods();
            WorldManager.LoadWorldFile();
            WhitelistManager.LoadServerWhitelist();
            CustomDifficultyManager.LoadCustomDifficulty();

            ConsoleManager.WriteToConsole($"----------------------------------------", LogMode.Title);
        }

        private static void LoadServerConfig()
        {
            string path = Path.Combine(corePath, "ServerConfig.json");

            if (File.Exists(path)) serverConfig = Serializer.SerializeFromFile<ServerConfigFile>(path);
            else
            {
                serverConfig = new ServerConfigFile();
                Serializer.SerializeToFile(path, serverConfig);
            }

            ConsoleManager.WriteToConsole("Loaded server configs", LogMode.Warning);
        }

        public static void SaveServerConfig(ServerConfigFile serverConfig)
        {
            string path = Path.Combine(corePath, "ServerConfig.json");

            Serializer.SerializeToFile(path, serverConfig);

            ConsoleManager.WriteToConsole("Saved server Config", LogMode.Warning);

        }

        private static void LoadServerValues()
        {
            string path = Path.Combine(corePath, "ServerValues.json");

            if (File.Exists(path)) serverValues = Serializer.SerializeFromFile<ServerValuesFile>(path);
            else
            {
                serverValues = new ServerValuesFile();
                Serializer.SerializeToFile(path, serverValues);
            }

            ConsoleManager.WriteToConsole("Loaded server values", LogMode.Warning);
        }

        public static void SaveServerValues(ServerValuesFile serverValues)
        {
            string path = Path.Combine(corePath, "ServerValues.json");

            Serializer.SerializeToFile(path, serverValues);

            ConsoleManager.WriteToConsole("Saved server values", LogMode.Warning);
        }

        private static void LoadEventValues()
        {
            string path = Path.Combine(corePath, "EventValues.json");

            if (File.Exists(path)) eventValues = Serializer.SerializeFromFile<EventValuesFile>(path);
            else
            {
                eventValues = new EventValuesFile();
                Serializer.SerializeToFile(path, eventValues);
            }

            ConsoleManager.WriteToConsole("Loaded event values", LogMode.Warning);
        }

        private static void LoadSiteValues()
        {
            string path = Path.Combine(corePath, "SiteValues.json");

            if (File.Exists(path)) siteValues = Serializer.SerializeFromFile<SiteValuesFile>(path);
            else
            {
                siteValues = new SiteValuesFile();
                Serializer.SerializeToFile(path, siteValues);
            }

            ConsoleManager.WriteToConsole("Loaded site values", LogMode.Warning);
        }

        private static void LoadActionValues()
        {
            string path = Path.Combine(corePath, "ActionValues.json");

            if (File.Exists(path)) actionValues = Serializer.SerializeFromFile<ActionValuesFile>(path);
            else
            {
                actionValues = new ActionValuesFile();
                Serializer.SerializeToFile(path, actionValues);
            }

            ConsoleManager.WriteToConsole("Loaded action values", LogMode.Warning);
        }

        public static void ChangeTitle()
        {
            Console.Title = $"Rimworld Together {CommonValues.executableVersion} - " +
                $"Players [{Network.connectedClients.Count}/{serverConfig.MaxPlayers}]";
        }

        public static void SaveServerConfig()
        {
            string path = Path.Combine(corePath, "ServerConfig.json");

            if (serverConfig != null)
            {
                Serializer.SerializeToFile(path, serverConfig);
            }
        }


    }
}
