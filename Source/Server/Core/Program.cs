using System.Globalization;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Managers;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Core
{
    public static class Program
    {
        public static string mainPath;
        public static string corePath;
        public static string usersPath;
        public static string settlementsPath;
        public static string savesPath;
        public static string mapsPath;
        public static string logsPath;
        public static string sitesPath;
        public static string factionsPath;

        public static string modsPath;
        public static string requiredModsPath;
        public static string optionalModsPath;
        public static string forbiddenModsPath;

        public static List<string> loadedRequiredMods = new List<string>();
        public static List<string> loadedOptionalMods = new List<string>();
        public static List<string> loadedForbiddenMods = new List<string>();

        public static ServerConfigFile serverConfig;
        public static ServerValuesFile serverValues;
        public static WorldValuesFile worldValues;
        public static DifficultyValuesFile difficultyValues;
        public static EventValuesFile eventValues;
        public static SiteValuesFile siteValues;
        public static ActionValuesFile actionValues;
        public static WhitelistFile whitelist;

        public static string serverVersion = "1.0.9";

        public static bool isClosing;
        public static CancellationToken serverCancelationToken = new();

        public static void Main()
        {
            try
            {
                QuickEdit quickEdit = new QuickEdit();
                quickEdit.DisableQuickEdit();
            }
            catch { };

            Console.ForegroundColor = ConsoleColor.White;

            LoadResources();
            Threader.GenerateServerThread(Threader.ServerMode.Start, serverCancelationToken);
            Threader.GenerateServerThread(Threader.ServerMode.Console, serverCancelationToken);

            while (true) Thread.Sleep(1);
        }

        public static void LoadResources()
        {
            SetPaths();

            Logger.WriteToConsole($"Loading all necessary resources", Logger.LogMode.Title);
            Logger.WriteToConsole($"----------------------------------------", Logger.LogMode.Title);

            SetCulture();
            CustomDifficultyManager.LoadCustomDifficulty();
            LoadServerConfig();
            LoadServerValues();
            LoadEventValues();
            LoadSiteValues();
            WorldManager.LoadWorldFile();
            LoadActionValues();
            WhitelistManager.LoadServerWhitelist();
            ModManager.LoadMods();

            Titler.ChangeTitle();

            Logger.WriteToConsole($"----------------------------------------", Logger.LogMode.Title);
        }

        private static void SetCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);

            Logger.WriteToConsole($"Loaded server culture > [{CultureInfo.CurrentCulture}]");
        }

        private static void SetPaths()
        {
            mainPath = Directory.GetCurrentDirectory();
            corePath = Path.Combine(mainPath, "Core");
            usersPath = Path.Combine(mainPath, "Users");
            settlementsPath = Path.Combine(mainPath, "Settlements");
            savesPath = Path.Combine(mainPath, "Saves");
            mapsPath = Path.Combine(mainPath, "Maps");
            logsPath = Path.Combine(mainPath, "Logs");
            sitesPath = Path.Combine(mainPath, "Sites");
            factionsPath = Path.Combine(mainPath, "Factions");

            modsPath = Path.Combine(mainPath, "Mods");
            requiredModsPath = Path.Combine(modsPath, "Required");
            optionalModsPath = Path.Combine(modsPath, "Optional");
            forbiddenModsPath = Path.Combine(modsPath, "Forbidden");

            if (!Directory.Exists(corePath)) Directory.CreateDirectory(corePath);
            if (!Directory.Exists(usersPath)) Directory.CreateDirectory(usersPath);
            if (!Directory.Exists(settlementsPath)) Directory.CreateDirectory(settlementsPath);
            if (!Directory.Exists(savesPath)) Directory.CreateDirectory(savesPath);
            if (!Directory.Exists(mapsPath)) Directory.CreateDirectory(mapsPath);
            if (!Directory.Exists(logsPath)) Directory.CreateDirectory(logsPath);
            if (!Directory.Exists(sitesPath)) Directory.CreateDirectory(sitesPath);
            if (!Directory.Exists(factionsPath)) Directory.CreateDirectory(factionsPath);

            if (!Directory.Exists(modsPath)) Directory.CreateDirectory(modsPath);
            if (!Directory.Exists(requiredModsPath)) Directory.CreateDirectory(requiredModsPath);
            if (!Directory.Exists(optionalModsPath)) Directory.CreateDirectory(optionalModsPath);
            if (!Directory.Exists(forbiddenModsPath)) Directory.CreateDirectory(forbiddenModsPath);
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

            Logger.WriteToConsole("Loaded server configs");
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

            Logger.WriteToConsole("Loaded server values");
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

            Logger.WriteToConsole("Loaded event values");
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

            Logger.WriteToConsole("Loaded site values");
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

            Logger.WriteToConsole("Loaded action values");
        }
    }
}