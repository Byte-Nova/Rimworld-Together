using Shared;

namespace GameServer
{
    public static class WhitelistManager
    {
        public static void AddUserToWhitelist(string username)
        {
            Master.whitelist.WhitelistedUsers.Add(username);

            SaveWhitelistFile();

            Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' has been whitelisted",
                        Logger.LogMode.Warning);
        }

        public static void RemoveUserFromWhitelist(string username)
        {
            Master.whitelist.WhitelistedUsers.Remove(username);

            SaveWhitelistFile();

            Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' is no longer whitelisted",
                Logger.LogMode.Warning);
        }

        public static void ToggleWhitelist()
        {
            Master.whitelist.UseWhitelist = !Master.whitelist.UseWhitelist;

            SaveWhitelistFile();

            if (Master.whitelist.UseWhitelist) Logger.WriteToConsole("Whitelist is now ON", Logger.LogMode.Warning);
            else Logger.WriteToConsole("Whitelist is now OFF", Logger.LogMode.Warning);
        }

        private static void SaveWhitelistFile()
        {
            Serializer.SerializeToFile(Path.Combine(Master.corePath, "Whitelist.json"), 
                Master.whitelist);
        }

        public static void LoadServerWhitelist()
        {
            string path = Path.Combine(Master.corePath, "Whitelist.json");

            if (File.Exists(path)) Master.whitelist = Serializer.SerializeFromFile<WhitelistFile>(path);
            else
            {
                Master.whitelist = new WhitelistFile();
                Serializer.SerializeToFile(path, Master.whitelist);
            }

            Logger.WriteToConsole("Loaded server whitelist", Logger.LogMode.Warning);
        }
    }
}
