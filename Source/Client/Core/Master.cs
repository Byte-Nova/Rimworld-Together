using System.Globalization;
using System.IO;
using UnityEngine;
using Verse;
using Shared;
using GameClient;

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
        public static string modPath;
        public static string connectionDataPath;
        public static string loginDataPath;
        public static string clientPreferencesPath;
        public static string savesPath;

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
            modPath = Path.Combine(mainPath, "Rimworld Together");
            connectionDataPath = Path.Combine(modPath, "ConnectionData.json");
            loginDataPath = Path.Combine(modPath, "LoginData.json");
            clientPreferencesPath = Path.Combine(modPath, "Preferences.json");
            savesPath = GenFilePaths.SavedGamesFolderPath;

            if (!Directory.Exists(modPath)) Directory.CreateDirectory(modPath);

            Logs.PrepareFileName(modPath);
        }

        public static void LoadClientPreferences()
        {
            ClientPreferencesFile newPreferences;

            if (File.Exists(clientPreferencesPath))
            {
                newPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(clientPreferencesPath);
                ClientValues.autosaveDays = int.Parse(newPreferences.AutosaveInterval);
                ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);
            }

            else
            {
                ClientValues.autosaveDays = 3;
                ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);

                PreferenceManager.SaveClientPreferences(ClientValues.autosaveDays.ToString());
            }
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
