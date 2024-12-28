using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Verse;
namespace GameClient
{
    public static class CompatibilityManager 
    {
        public static void LoadAllPatchedAssemblies()
        {
            List<Assembly> toLoad = new List<Assembly>();
            foreach (string compatibility in CompatibilityManagerHelper.GetAllPatchedMods())
            {
                Assembly toAdd = LoadCustomAssembly(compatibility);
                if (toAdd != null) toLoad.Add(toAdd);    
            }
            
            if (toLoad.Count > 0)
            {
                Master.loadedCompatibilityPatches = toLoad.ToArray();
                Logger.Warning($"Loaded > {Master.loadedCompatibilityPatches.Length} patches");
                Logger.Warning($"CAUTION > Custom patches aren't created by the mod developers, always use them with care");
            }
        }

        private static Assembly LoadCustomAssembly(string assemblyPath)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace == null) continue;
                    else if (type.Namespace.StartsWith("System") || type.Namespace.StartsWith("Microsoft")) continue;
                    else if (type.GetCustomAttributes(typeof(RTStartupAttribute), false).Length != 0)
                    {
                        if (type.IsAbstract && type.IsSealed)
                        {
                            ConstructorInfo constructor = type.TypeInitializer;
                            if (constructor != null)
                            {
                                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                                return assembly;
                            }
                            else Logger.Error($"Mod {MethodManager.GetAssemblyName(assembly)} has class {type.Name} with attribute 'RTStartup' but no constructor.");
                        }
                        else Logger.Error($"Mod {MethodManager.GetAssemblyName(assembly)} has class {type.Name} with attribute 'RTStartup' but isn't static.");
                    } 
                }
            }
            catch (Exception e) { Logger.Error($"Failed to load patch '{assemblyPath}'. {e}"); }

            return null;
        }
    }

    public static class CompatibilityManagerHelper
    {
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
