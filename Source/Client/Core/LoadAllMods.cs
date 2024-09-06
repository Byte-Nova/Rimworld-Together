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
                if (LoadedModManager.RunningModsListForReading.Any(mod => mod.Name == allModsToLoad[name]))
                {
                    if (File.Exists(Path.Combine(assemblyPath, name + fileExtension))) 
                    {
                        Assembly assembly = Assembly.LoadFrom(Path.Combine(assemblyPath, name + fileExtension));
                        Master.loadedPatches.Add(name,assembly);
                        
                        try 
                        {
                            Type toUse = typeof(LoadAllMods);
                            MethodInfo methodInfo = toUse.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                            methodInfo.Invoke(name,null);

                            Logger.Message($"Loaded patch {name} patching {allModsToLoad[name]}");
                        }
                        catch (Exception ex){ Logger.Error($"Failed to load patch {name} because :\n{ex.ToString()}"); }
                    }
                    else Logger.Error($"Couldn't find patch for mod {allModsToLoad[name]}");
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
    }
}
