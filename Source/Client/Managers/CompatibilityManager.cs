using GameClient.SOS2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Verse;

namespace GameClient
{
    public static class CompatibilityManager 
    {
        public static void LoadAllPatchedAssemblies()
        {
            string[] allCompatibilitiesToLoad = CompatibilityManagerHelper.GetAllPatchedMods(); // We get all patches by their names
            
            foreach (string compatibility in allCompatibilitiesToLoad)
            {
                string compatibilityName = Path.GetFileNameWithoutExtension(compatibility);

                if (LoadedModManager.RunningModsListForReading.Any(mod => mod.PackageId == compatibilityName)) // Looks if the dll name matches the name of any mod ids loaded.
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(compatibility);
                        Type toUse = typeof(CompatibilityManager);
                        string toFind = Regex.Replace(compatibilityName, @"[^a-zA-Z0-9]", ""); // Makes the name C# friendly
                        Logger.Warning(toFind);
                        MethodInfo methodInfo = toUse.GetMethod(toFind, BindingFlags.NonPublic | BindingFlags.Static); // We try to find a function with the same name as the mod id
                        Master.loadedCompatibilityPatches.Add(compatibilityName, assembly);
                        methodInfo.Invoke(toFind, null);

                        Logger.Message($"Loaded patch for '{compatibilityName}'");
                    }
                    catch (Exception ex){ Logger.Error($"Failed to load patch for '{compatibilityName}' because :\n{ex}"); }
                }
            }
        }

        //Entry point for the SOS2 patch

        private static void kentingtonsaveourship2()
        {
            SOS2SendData.StartSOS2();
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
