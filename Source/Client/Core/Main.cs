using HarmonyLib;
using Shared;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace GameClient
{
    //Class that works as an entry point for the mod

    public static class Main_
    {
        private static readonly string modID = "RimWorld Together";

        [StaticConstructorOnStartup]
        public static class RimworldTogether
        {
            static RimworldTogether() 
            {
                ApplyHarmonyPathches();

                PrepareCulture();
                PreparePaths();
                CreateUnityDispatcher();

                FactionValues.SetPlayerFactionDefs();
                CaravanManagerHelper.SetCaravanDefs();
                PreferenceManager.LoadClientPreferences();

                CompatibilityManager.LoadAllPatchedAssemblies();
            }
        }

        private static void ApplyHarmonyPathches()
        {
            Harmony harmony = new Harmony(modID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void PrepareCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
        }

        public static void PreparePaths()
        {
            Master.mainPath = GenFilePaths.SaveDataFolderPath;
            Master.modFolderPath = Path.Combine(Master.mainPath, "RimWorld Together");
            
            Master.modAssemblyPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();
            Master.compatibilityPatchesFolderPath = Path.Combine(Master.modAssemblyPath, "Patches");

            Master.connectionDataPath = Path.Combine(Master.modFolderPath, "ConnectionData.json");
            Master.clientPreferencesPath = Path.Combine(Master.modFolderPath, "Preferences.json");
            Master.loginDataPath = Path.Combine(Master.modFolderPath, "LoginData.json");
            Master.savesFolderPath = GenFilePaths.SavedGamesFolderPath;

            if (!Directory.Exists(Master.modFolderPath)) Directory.CreateDirectory(Master.modFolderPath);
            if (!Directory.Exists(Master.compatibilityPatchesFolderPath)) Directory.CreateDirectory(Master.compatibilityPatchesFolderPath);
        }

        public static void CreateUnityDispatcher()
        {
            if (Master.threadDispatcher == null)
            {
                GameObject go = new GameObject("Dispatcher");
                Master.threadDispatcher = go.AddComponent(typeof(UnityMainThreadDispatcher)) as UnityMainThreadDispatcher;
                Object.Instantiate(go);

                Logger.Message($"Created dispatcher for version {CommonValues.executableVersion}");
            }
        }
    }
}