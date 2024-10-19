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

                    string result = MethodManager.TryExecuteMethod(assembly, compatibilityName, "Main");
                    if (result != "") Logger.Warning($"Fail to find entry point of assembly {compatibility}.\nDebugging info: {result}");
                    else Logger.Warning($"Loaded patch for '{compatibilityName}'", CommonEnumerators.LogImportanceMode.Verbose);
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
