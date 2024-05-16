using GameServer;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.IO.Compression;
using static Shared.CommonEnumerators;

namespace PluginServer_Autosave
{
    internal class Autosave
    {
        public static string pluginPath;
        public static Dictionary<string, DateTime> clientsLastSaved;
        public static bool waitingForUsersToAutosave;

        public static void Init(string pluginPath)
        {
            Autosave.pluginPath = pluginPath;
            clientsLastSaved = new();
            waitingForUsersToAutosave = false;
        }

        public struct Config
        {
            [JsonProperty(Required = Required.Always)]
            public int autosaveIntervalInSeconds;

            [JsonProperty(Required = Required.Always)]
            public int checkUsersAutosavedIntervalInSeconds;

            [JsonProperty(Required = Required.Always)]
            public int maxUsersAutosaveWaitInSeconds;

            public static Config defaultConfig
            {
                get
                {
                    return new Config
                    {
                        autosaveIntervalInSeconds = 5 * 60,
                        checkUsersAutosavedIntervalInSeconds = 15,
                        maxUsersAutosaveWaitInSeconds = 3 * 60,
                    };
                }
            }
        }

        public static Config LoadConfig(string pluginPath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;

            string configPath = Path.Combine(pluginPath, "config.json");
            Config config;

            try
            { 
                using (StreamReader r = new StreamReader(configPath))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Config>(json, settings);
                }
            }
            catch
            {
                config = Config.defaultConfig;
                var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

                File.WriteAllText(configPath, serializedConfig);
            }

            return config;
        }

        public static void ThreadMethod()
        {
            Config config = LoadConfig(pluginPath);

            while (true)
            {
                Thread.Sleep(config.autosaveIntervalInSeconds * 1000);

                Logger.WriteToConsole($"[Plugin:Autosave] > Requesting Users Autosave...", LogMode.Message);

                var autosaveRequestedAt = DateTime.Now;
                var autosaveStopAt = autosaveRequestedAt.AddSeconds(config.maxUsersAutosaveWaitInSeconds);
                var connectedClients = Network.connectedClients.ToArray();
                var savingClients = new List<ServerClient>();
                foreach (ServerClient client in connectedClients)
                {
                    if (client.isReadyToPlay)
                    {
                        CommandManager.SendForceSaveCommand(client, false);
                        savingClients.Add(client);
                    }
                }

                waitingForUsersToAutosave = true;
                while (waitingForUsersToAutosave)
                {
                    Thread.Sleep(config.checkUsersAutosavedIntervalInSeconds * 1000);

                    lock (clientsLastSaved)
                    {
                        bool stillWaitingForUsersToAutosave = false;
                        foreach (ServerClient client in savingClients)
                        {
                            stillWaitingForUsersToAutosave |= !(clientsLastSaved.ContainsKey(client.uid) && clientsLastSaved[client.uid] > autosaveRequestedAt);
                        }

                        waitingForUsersToAutosave = stillWaitingForUsersToAutosave;

                        if (waitingForUsersToAutosave && DateTime.Now > autosaveStopAt)
                        {
                            Logger.WriteToConsole($"[Plugin:Autosave] > Max Users Autosave Wait Time Exceeded!", LogMode.Error);

                            foreach (ServerClient client in savingClients)
                            {
                                if (!(clientsLastSaved.ContainsKey(client.uid) && clientsLastSaved[client.uid] > autosaveRequestedAt))
                                {
                                    Logger.WriteToConsole($"[Plugin:Autosave] > -- {client.username} Did Not Autosave!", LogMode.Warning);
                                }
                            }

                            waitingForUsersToAutosave = false;
                        }
                    }
                }

                Logger.WriteToConsole($"[Plugin:Autosave] > Users Autosave Complete!", LogMode.Message);

                PerformAutosave();
            }
        }

        public static void PerformAutosave()
        {
            string pluginsPath = Directory.GetParent(pluginPath).FullName;
            string serverPath = Directory.GetParent(pluginsPath).FullName;

            try
            {
                var now = DateTime.Now;
                string dateTime = now.ToString("MM-dd-yyyy_hh-mm-ss");

                Logger.WriteToConsole($"[Plugin:Autosave] > Autosave Started...", LogMode.Message);

                if (!Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);

                string zipPath = Path.Combine(pluginPath, dateTime + ".zip");
                string serverZipPath = Path.Combine(pluginPath, "Temp");

                CopyDirectory(serverPath, serverZipPath, new string[]
                {
                        /* Dev-Specific Files */
                        Path.Combine(serverPath, "GameServer.deps.json"),
                        Path.Combine(serverPath, "GameServer.dll"),
                        Path.Combine(serverPath, "GameServer.pdb"),
                        Path.Combine(serverPath, "GameServer.runtimeconfig.json"),
                        Path.Combine(serverPath, "Mono.Nat.dll"),
                        Path.Combine(serverPath, "Newtonsoft.Json.dll"),
                        Path.Combine(serverPath, "System.Security.Permissions.dll"),
                        Path.Combine(serverPath, "System.Windows.Extensions.dll"),
                        Path.Combine(serverPath, "runtimes"),

                        /* Excluded Files */
                        Path.Combine(serverPath, "GameServer.exe"),
                        Path.Combine(serverPath, "Mods"),
                        Path.Combine(serverPath, "Plugins"),
                        Path.Combine(serverPath, "Logs"),
                });

                ZipFile.CreateFromDirectory(serverZipPath, zipPath);

                Directory.Delete(serverZipPath, true);

                Logger.WriteToConsole($"[Plugin:Autosave] > Autosave Complete! [{zipPath}]", LogMode.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteToConsole($"[Plugin:Autosave] > Autosave Error! [{ex}]", LogMode.Error);
            }
        }

        public static void MarkClientSaved(ServerClient client)
        {
            if (waitingForUsersToAutosave)
            {
                lock (clientsLastSaved)
                {
                    clientsLastSaved[client.uid] = DateTime.Now;
                }
            }
        }

        protected static void CopyDirectory(string sourceDir, string destinationDir, string[] exclude)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                if (exclude.Contains(file.FullName))
                {
                    continue;
                }

                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                if (exclude.Contains(subDir.FullName))
                {
                    continue;
                }

                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, exclude);
            }
        }
    }
}
