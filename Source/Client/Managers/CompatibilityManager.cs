using HarmonyLib;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
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
                try
                {
                    Assembly assembly = Assembly.LoadFrom(compatibility);

                    Master.loadedCompatibilityPatches.Add(compatibilityName, assembly);

                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.Namespace != null && (type.Namespace.StartsWith("System") || type.Namespace.StartsWith("Microsoft")))
                        {
                            continue;
                        }
                        if (type.GetCustomAttributes(typeof(RTStartupAttribute), false).Any())
                        {
                            if (type.IsAbstract && type.IsSealed)
                            {
                                ConstructorInfo constructor = type.TypeInitializer;

                                if (constructor != null)
                                {
                                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                                    Logger.Message($"Succesfully loaded patch {compatibilityName}");
                                }
                                else
                                {
                                    Logger.Error($"Mod {compatibilityName} has class {type.Name} with attribute 'RTStartup' but no constructor.");
                                }
                            }
                            else
                            {
                                Logger.Error($"Mod {compatibilityName} has class {type.Name} with attribute 'RTStartup' but isn't static.");
                            }
                        } 
                    }
                }
                catch (Exception ex) { Logger.Error($"Failed to load patch for '{compatibilityName}'\nFull path:{compatibility}\n Debugging info:{ex}"); }
            }
        }
    }

    public static class CompatibilityManagerHelper
    {
        public static readonly string fileExtension = ".dll";
        public static readonly string PatchFolderName = "RTPatches";
        public static string[] GetAllPatchedMods()
        {
            List<string> results = new List<string>();
            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                if (Directory.Exists(Path.Combine(mod.RootDir, PatchFolderName)))
                {
                    results.AddRange(Directory.GetFiles(Path.Combine(mod.RootDir, PatchFolderName)));
                }
            }
            return results.ToArray();
        }
    }
}
