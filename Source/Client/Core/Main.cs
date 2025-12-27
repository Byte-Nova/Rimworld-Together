using GameClient.Core.Configs;
using GameClient.Files;
using GameClient.Misc;
using HarmonyLib;
using Shared;
using Shared.Misc;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Core
{
    //Class that works as an entry point for the mod

    public static class Main_
    {
        [StaticConstructorOnStartup]
        public static class RimworldTogether
        {
            static RimworldTogether()
            {
                ClientPrinter.CreateLogger();

                ApplyHarmonyPatches();
                PrepareCulture();
                PreparePaths();

                CreateUnityDispatcher();
                MethodGatherer.CacheAllMethods(MethodGatherer.AssemblyType.Client);
                PersistentSettings.SetFilePath(Path.Combine(Master.AppdataRTPath, "PersistentSettings" + CommonValues.DefaultSaveFormat));
            }
        }

        private static void ApplyHarmonyPatches()
        {
            Harmony harmony = new Harmony(Master.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void PrepareCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
        }

        private static void PreparePaths()
        {
            Master.SavesFolderPath = GenFilePaths.SavedGamesFolderPath;

            Master.AppdataPath = GenFilePaths.SaveDataFolderPath;
            Master.AppdataRTPath = Path.Combine(Master.AppdataPath, "RimWorld Together");
            Master.AppdataTempPath = Path.Combine(Master.AppdataRTPath, "Temp");
            Master.AppdataVersionPath = Path.Combine(Master.AppdataTempPath, "Version");

            string mod = LoadedModManager.RunningMods.First(m => (m.PackageId == Master.ModPackageID || m.PackageId == Master.ModPackageID + "_steam") 
                && ModLister.GetActiveModWithIdentifier(m.PackageId) != null).RootDir;

            Master.ModMainPath = mod;
            Master.ModScriptsPath = Path.Combine(Master.ModMainPath, "Scripts");
            Master.ModAssemblyPath = Path.Combine(Master.ModMainPath, "Current", "Assemblies");

            if (!Directory.Exists(Master.AppdataRTPath)) Directory.CreateDirectory(Master.AppdataRTPath);

            if (Directory.Exists(Master.AppdataTempPath)) Directory.Delete(Master.AppdataTempPath, true);
            Directory.CreateDirectory(Master.AppdataTempPath);
            Directory.CreateDirectory(Master.AppdataVersionPath);
        }

        private static void CreateUnityDispatcher()
        {
            if (MainThreadHandler.Instance == null)
            {
                GameObject go = UnityEngine.Object.Instantiate(new GameObject());
                go.AddComponent(typeof(MainThreadHandler));
            }
        }
    }
}