using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient
{
    public static class LoadAllMods 
    {
        public static string AssemblyPath = Path.Combine(Assembly.GetExecutingAssembly().Location, Path.DirectorySeparatorChar.ToString(), "..");
        public static void LoadAllModAssemblies()
        {
            LoadSOS2Patch();
        } 

        public static bool LoadSOS2Patch() 
        {
            if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Save Our Ship 2"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(Path.Combine(AssemblyPath, Path.DirectorySeparatorChar.ToString(), "SOS2Patch"));
                    Logger.Message($"Successfuly loaded SOS2Patches");
                    return true;
                }
                catch (Exception ex) { Logger.Warning($"SOS2Patch could not be loaded despite SOS2 being in load order.\n{ex}"); }
            }
            return false;
        }
    }
}
