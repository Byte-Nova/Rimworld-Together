using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class WhitelistManager
    {
        public static void AddUserToWhitelist(string username)
        {
            Master.whitelist.WhitelistedUsers.Add(username);

            SaveWhitelistFile();

            ConsoleManager.WriteToConsole($"User '{ServerCommandManager.parsedParameters[0]}' has been whitelisted",
                        LogMode.Warning);
        }

        public static void RemoveUserFromWhitelist(string username)
        {
            Master.whitelist.WhitelistedUsers.Remove(username);

            SaveWhitelistFile();

            ConsoleManager.WriteToConsole($"User '{ServerCommandManager.parsedParameters[0]}' is no longer whitelisted",
                LogMode.Warning);
        }

        public static void ToggleWhitelist()
        {
            Master.whitelist.UseWhitelist = !Master.whitelist.UseWhitelist;

            SaveWhitelistFile();

            if (Master.whitelist.UseWhitelist) ConsoleManager.WriteToConsole("Whitelist is now ON", LogMode.Warning);
            else ConsoleManager.WriteToConsole("Whitelist is now OFF", LogMode.Warning);
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

            ConsoleManager.WriteToConsole("Loaded server whitelist", LogMode.Warning);
        }
    }
}
