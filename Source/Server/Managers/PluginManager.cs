using PluginServer;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using static Shared.CommonEnumerators;

namespace GameServer
{

    public static class PluginManager
    {
        public class PluginReflection : IPlugin
        {
            protected object obj;
            protected MethodInfo init;
            protected MethodInfo onEvent;

            public PluginReflection(Type type)
            {
                obj = Activator.CreateInstance(type) ?? throw new Exception();
                init = type.GetMethod("Init") ?? throw new Exception();
                onEvent = type.GetMethod("OnEvent") ?? throw new Exception();
            }

            public void Init(string pluginPath)
            {
                init.Invoke(obj, new object[] { pluginPath });
            }

            public void OnEvent(string name, object[] args)
            {
                onEvent.Invoke(obj, new object[] { name, args });
            }
        }

        public class Plugins
        {
            public struct Entry
            {
                public AssemblyLoadContext loadContext;
                public IPlugin plugin;
            }

            protected Dictionary<string, Entry> entries;

            public Plugins()
            {
                entries = new();
            }

            public void UnloadAll()
            {
                foreach (Entry entry in entries.Values)
                {
                    entry.loadContext.Unload();
                }

                entries.Clear();
            }

            public void Load(string pluginPath)
            {
                IPlugin? plugin = null;
                AssemblyLoadContext? loadContext = null;

                string[] DLLPaths = Directory.GetFiles(pluginPath, "*.dll");

                foreach (string DLLPath in DLLPaths)
                {
                    var DLLLoadContext = new AssemblyLoadContext(DLLPath, true);

                    try
                    {
                        var DLL = DLLLoadContext.LoadFromAssemblyPath(DLLPath);
                        var DLLPluginClassName = "Plugin";
                        var DLLPluginClass = DLL.GetType(DLLPluginClassName) ?? throw new Exception();
                        IPlugin DLLPluginObject;

                        try
                        {
                            DLLPluginObject = (IPlugin)(Activator.CreateInstance(DLLPluginClass) ?? throw new Exception());
                        }
                        catch
                        {
                            DLLPluginObject = new PluginReflection(DLLPluginClass);
                        }

                        loadContext = DLLLoadContext;
                        plugin = DLLPluginObject;
                    }
                    catch
                    {
                        DLLLoadContext.Unload();
                        continue;
                    }
                    break;
                }

                if (plugin == null || loadContext == null)
                {
                    throw new Exception();
                }

                plugin.Init(pluginPath);

                Add(pluginPath, loadContext, plugin);
            }

            protected void Add(string name, AssemblyLoadContext loadContext, IPlugin plugin)
            {
                entries.Add(name, new Entry {
                    plugin = plugin,
                    loadContext = loadContext,
                });
            }

            public void Emit(string name, params object[] args)
            {
                foreach(Entry entry in entries.Values)
                {
                    entry.plugin.OnEvent(name, args);
                }
            }

            public int Count()
            {
                return entries.Count;
            }

            public Entry? GetPlugin(string name)
            {
                if (!entries.ContainsKey(name))
                {
                    return null;
                }

                return entries[name];
            }
        }

        public static void UnloadPlugins()
        {
            Master.plugins.Emit("pluginsUnloaded_pre");

            Master.plugins.UnloadAll();
        }

        public static bool assemblyLoaderBound = false;
        public static void BindAssemblyLoader()
        {
            if (assemblyLoaderBound) return;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            assemblyLoaderBound = true;
        }

        [UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file", Justification = "<Pending>")]
        public static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
        {
            if (args.RequestingAssembly == null)
            {
                return null;
            }

            Assembly? loadedAssembly = null;
            string assemblyRequesting = args.RequestingAssembly.Location;
            string assemblyDesired = args.Name;

            try
            {
                string assemblyFolder = Directory.GetParent(args.RequestingAssembly.Location)?.FullName ?? throw new Exception();
                string[] assemblyFilePaths = Directory.GetFiles(assemblyFolder, "*.dll");

                foreach (string assemblyFilePath in assemblyFilePaths)
                {
                    var loadContext = new AssemblyLoadContext(assemblyFilePath, true);
                    var assembly = loadContext.LoadFromAssemblyPath(assemblyFilePath);
                    var assemblyName = assembly.FullName;
                    loadContext.Unload();

                    if (assemblyName == assemblyDesired)
                    {
                        loadedAssembly = Assembly.LoadFile(assemblyFilePath);

                        break;
                    }
                }
            }
            catch { }

            if (loadedAssembly == null)
            {
                throw new Exception("Failed to load assembly " + assemblyDesired + " for requesting assembly " + assemblyRequesting);
            }

            return loadedAssembly;
        }

        public static void LoadPlugins()
        {
            BindAssemblyLoader();

            UnloadPlugins();

            string[] pluginsToLoad = Directory.GetDirectories(Master.pluginsPath);

            foreach (string pluginPath in pluginsToLoad)
            {
                try
                {
                    Master.plugins.Load(pluginPath);
                }
                catch { Logger.WriteToConsole($"[Error] > Failed to load plugin '{pluginPath}'", LogMode.Error); }
            }

            Master.plugins.Emit("pluginsLoaded_post");

            Logger.WriteToConsole($"Loaded plugins [{Master.plugins.Count()}]", LogMode.Warning);
        }
    }
}
