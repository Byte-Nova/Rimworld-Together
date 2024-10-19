using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
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
                catch (Exception ex) { Logger.Error($"Failed to load patch for '{compatibilityName}' because :\n{ex}"); }
            }
        }
        private static void LoadCommandsFromAssembly(Assembly assembly)
        {
            Type type = assembly.GetType($"GameServer.CommandStorage");
            if (type != null)
            {
                FieldInfo field = type.GetField("serverCommands", BindingFlags.Static | BindingFlags.Public);
                CommandStorage.serverCommands.AddRange((List<ServerCommand>)field.GetValue(null));
            }
        }
    }
    public static class CompatibilityManagerHelper
    {
        public static readonly string fileExtension = ".dll";

        public static string[] GetAllPatchedMods()
        {
            return Directory.GetFiles(Master.compatibilityPatchesPath)
                .Where(fetch => fetch.EndsWith(fileExtension)).ToArray();
        }
    }
}
