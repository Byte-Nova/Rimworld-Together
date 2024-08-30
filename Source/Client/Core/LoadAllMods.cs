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
        public static readonly string AssemblyPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "Patches");
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
                    Assembly assembly = Assembly.LoadFrom(Path.Combine(AssemblyPath, "SOS2Patch.dll"));
                    Master.SOS2 = assembly;
                    SOS2SendData.StartSOS2();
                    return true;
                }
                catch (Exception ex) { Logger.Error($"SOS2Patch could not be loaded despite SOS2 being in load order.\n{ex}"); }
            }
            return false;
        }
    }
}
