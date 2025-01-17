using GameServer.Core;
using GameServer.Misc;
using Shared;
using System.Reflection;

namespace GameServer.Managers
{
    public static class CompatibilityManager
    {
        public static void LoadAllPatches()
        {
            foreach (string compatibility in CompatibilityManagerH.GetAllPatchedMods())
            {
                LoadCustomAssembly(compatibility);
            }
        }

        private static void LoadCustomAssembly(string assemblyPath)
        {
            try
            {
                Assembly toLoad = Assembly.LoadFrom(assemblyPath);

                Type[] foundTypes = toLoad.GetTypes().Where(type => type.GetCustomAttributes(typeof(RTManager), false).Length != 0 ||
                    type.GetCustomAttributes(typeof(RTStartupAttribute), false).Length != 0).ToArray();

                foreach (Type type in foundTypes)
                {
                    if (type.GetCustomAttributes(typeof(RTManager), false).Length != 0)
                    {
                        MethodInfo method = type.GetMethod("ParsePacket");

                        if (method != null)
                        {
                            Master.managerDictionary[type.Name] = method;
                            Printer.Message($"Found modded manager {type.Name}", CommonEnumerators.LogImportanceMode.Verbose);
                        }
                    }

                    else if (type.GetCustomAttributes(typeof(RTStartupAttribute), false).Length != 0)
                    {
                        if (type.IsAbstract && type.IsSealed) // It needs to be static
                        {
                            ConstructorInfo constructor = type.TypeInitializer;
                            if (constructor != null) System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                            else Printer.Error($"Mod '{toLoad.FullName}' has class '{type.Name}' with attribute 'RTStartup' but no constructor.");
                        }
                        else Printer.Error($"Mod '{toLoad.FullName}' has class '{type.Name}' with attribute 'RTStartup' but isn't static.");
                    } 
                }
            }
            catch (Exception e) { Printer.Error($"Failed to load patch '{assemblyPath}'\n{e}"); }
        }
    }

    public static class CompatibilityManagerH
    {
        public static readonly string fileExtension = ".dll";

        public static string[] GetAllPatchedMods()
        {
            return Directory.GetFiles(Master.compatibilityPatchesPath)
                .Where(fetch => fetch.EndsWith(fileExtension)).ToArray();
        }
    }
}
