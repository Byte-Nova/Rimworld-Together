using System.Globalization;
using System.IO;
using UnityEngine;
using Verse;
using Shared;

namespace GameClient
{
    //Class with all the critical variables for the client to work

    public static class Master
    {
        //Instances

        public static UnityMainThreadDispatcher threadDispatcher;
        public static ModConfigs modConfigs = new ModConfigs();

        //Paths

        public static string mainPath;
        public static string modFolderPath;
        public static string connectionDataPath;
        public static string loginDataPath;
        public static string clientPreferencesPath;
        public static string savesFolderPath;
        public static string worldSavesFolderPath;

        public static void PrepareCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
        }

        public static void PreparePaths()
        {
            mainPath = GenFilePaths.SaveDataFolderPath;
            modFolderPath = Path.Combine(mainPath, "Rimworld Together");

            worldSavesFolderPath = Path.Combine(modFolderPath, "Saved Server Worlds");
            connectionDataPath = Path.Combine(modFolderPath, "ConnectionData.json");
            clientPreferencesPath = Path.Combine(modFolderPath, "Preferences.json");
            loginDataPath = Path.Combine(modFolderPath, "LoginData.json");
            savesFolderPath = GenFilePaths.SavedGamesFolderPath;

            if (!Directory.Exists(modFolderPath)) Directory.CreateDirectory(modFolderPath);
            if (!Directory.Exists(worldSavesFolderPath)) Directory.CreateDirectory(worldSavesFolderPath);
        }

        public static void CreateUnityDispatcher()
        {
            if (threadDispatcher == null)
            {
                GameObject go = new GameObject("Dispatcher");
                threadDispatcher = go.AddComponent(typeof(UnityMainThreadDispatcher)) as UnityMainThreadDispatcher;
                Object.Instantiate(go);

                Log.Message($"[Rimworld Together] > Created dispatcher for version {CommonValues.executableVersion}");
            }
        }
    }
}
