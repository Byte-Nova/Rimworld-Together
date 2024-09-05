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
                    Assembly assembly = Assembly.LoadFrom(Path.Combine(assemblyPath, name + fileExtension));
                    if(assembly != null)
                    {
                         Master.loadedPatches.Add(name,assembly);
                         Logger.Message($"Loaded patch {name} patching {allModsToLoad[name]}");
                    }
                }
            }
        } 

        private static Dictionary<string,string> GetAllPatchedMods()
        {
            return new Dictionary<string,string>()
            {
                // PatchName    //Mod ID
                {"SOS2Patch", "Save Our Ship 2"}
            };
        }
    }
}
