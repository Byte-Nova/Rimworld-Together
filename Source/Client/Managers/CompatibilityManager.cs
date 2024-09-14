using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace GameClient
{
    public static class CompatibilityManager 
    {
        public static void LoadAllPatchedAssemblies()
        {
            string[] allCompatibilitiesToLoad = CompatibilityManagerHelper.GetAllPatchedMods();
            
            foreach (string compatibility in allCompatibilitiesToLoad)
            {
                string compatibilityName = Path.GetFileNameWithoutExtension(compatibility);

                if (LoadedModManager.RunningModsListForReading.Any(mod => mod.Name == compatibilityName))
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(compatibility);
                        Type toUse = typeof(CompatibilityManager);

                        MethodInfo methodInfo = toUse.GetMethod(compatibilityName, BindingFlags.NonPublic | BindingFlags.Static);
                        methodInfo.Invoke(compatibilityName, null);

                        Master.loadedCompatibilityPatches.Add(compatibilityName, assembly);
                        Logger.Message($"Loaded patch for '{compatibilityName}'");
                    }
                    catch (Exception ex){ Logger.Error($"Failed to load patch for '{compatibilityName}' because :\n{ex}"); }
                }
            }
        }

        //Entry point for the soon-to-come SOS2 patch

        private static void SOS2Patch()
        {
            Logger.Warning("Loaded!");
        }
    }

    public static class CompatibilityManagerHelper
    {
        public static readonly string fileExtension = ".dll";

        public static string[] GetAllPatchedMods()
        {
            return Directory.GetFiles(Master.compatibilityPatchesFolderPath)
                .Where(fetch => fetch.EndsWith(fileExtension)).ToArray();
        }
    }
}
