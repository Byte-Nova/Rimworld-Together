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

            Logger.Warning($"User '{ServerCommandManager.commandParameters[0]}' has been whitelisted");
        }

        public static void RemoveUserFromWhitelist(string username)
        {
            Master.whitelist.WhitelistedUsers.Remove(username);

            SaveWhitelistFile();

            Logger.Warning($"User '{ServerCommandManager.commandParameters[0]}' is no longer whitelisted");
        }

        public static void ToggleWhitelist()
        {
            Master.whitelist.UseWhitelist = !Master.whitelist.UseWhitelist;

            SaveWhitelistFile();

            if (Master.whitelist.UseWhitelist) Logger.Warning("Whitelist is now ON");
            else Logger.Warning("Whitelist is now OFF");
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

            Logger.Warning("Loaded server whitelist");
        }
    }
}
