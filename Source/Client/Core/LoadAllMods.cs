﻿using GameClient.SOS2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace GameClient
{
    public static class LoadAllMods 
    {
        public static readonly string assemblyPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "Patches");
        public static readonly string fileExtension = ".dll";
        public static void LoadAllPatchAssemblies()
        {
            Dictionary<string,string> allModsToLoad = GetAllPatchedMods();
            foreach(string name in allModsToLoad.Keys)
            {
                if(LoadedModManager.RunningModsListForReading.Any(mod => mod.Name == allModsToLoad[name]))
                {
                    if(File.Exists(Path.Combine(assemblyPath, name + fileExtension))) {
                        Assembly assembly = Assembly.LoadFrom(Path.Combine(assemblyPath, name + fileExtension));
                        Master.loadedPatches.Add(name,assembly);
                        try 
                        {
                            Type toUse = typeof(LoadAllMods);
                            MethodInfo methodInfo = toUse.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                            methodInfo.Invoke(name,null);
                        }
                        catch (Exception ex){ Logger.Warning($"Failed to load patch {name} because :\n{ex.ToString()}"); }
                        Logger.Message($"Loaded patch {name} patching {allModsToLoad[name]}");
                    }
                }
            }
        } 

        private static Dictionary<string,string> GetAllPatchedMods()
        {
            return new Dictionary<string,string>()
            {
                // PatchName    // Mod name
                {"SOS2Patch", "Save Our Ship 2"}
            };
        }

        private static void SOS2Patch() 
        {
            SOS2SendData.StartSOS2();
        }
    }
}
