using System.Collections.Generic;
using System.IO;
using GameClient.Files;
using Shared;

namespace GameClient.Core.Preferences
{
    public static class RecentServersManager
    {
        public static void SaveRecentServers(RecentServersFile toSave) { Serializer.SerializeToFile(Master.recentServersPath, toSave); }

        public static RecentServersFile LoadRecentServers()
        {
            if (File.Exists(Master.recentServersPath)) return Serializer.SerializeFromFile<RecentServersFile>(Master.recentServersPath);
            else
            {
                RecentServersFile file = new RecentServersFile();
                file.ServerNames = new List<string>() { "LocalHost" };
                file.ServerAddresses = new List<string>() { "127.0.0.1:25555" };

                SaveRecentServers(file);
            }

            return LoadRecentServers();
        }

        public static void AddServerToList(string name, string address)
        {
            RecentServersFile existingServers = LoadRecentServers();
            if (existingServers.ServerAddresses.Contains(address)) return;
            else
            {
                existingServers.ServerNames.Add(name);
                existingServers.ServerAddresses.Add(address);

                SaveRecentServers(existingServers);
            }
        }

        public static void RemoveServerFromList(string name, string address)
        {
            RecentServersFile existingServers = LoadRecentServers();

            if (!existingServers.ServerAddresses.Contains(address)) return;
            else
            {
                existingServers.ServerNames.RemoveAt(GetIndexOfServer(address));
                existingServers.ServerAddresses.Remove(address);

                SaveRecentServers(existingServers);
            }
        }

        private static int GetIndexOfServer(string address)
        {
            RecentServersFile existingServers = LoadRecentServers();
            return existingServers.ServerAddresses.IndexOf(address);
        }
    }
}
