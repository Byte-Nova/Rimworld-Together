using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Managers
{
    public static class WhitelistManager
    {
        public static void AddUserToWhitelist(string username)
        {
            Program.whitelist.WhitelistedUsers.Add(username);

            SaveWhitelistFile();

            Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' has been whitelisted",
                        Logger.LogMode.Warning);
        }

        public static void RemoveUserFromWhitelist(string username)
        {
            Program.whitelist.WhitelistedUsers.Remove(username);

            SaveWhitelistFile();

            Logger.WriteToConsole($"User '{ServerCommandManager.commandParameters[0]}' is no longer whitelisted",
                Logger.LogMode.Warning);
        }

        public static void ToggleWhitelist()
        {
            Program.whitelist.UseWhitelist = !Program.whitelist.UseWhitelist;

            SaveWhitelistFile();

            if (Program.whitelist.UseWhitelist) Logger.WriteToConsole("Whitelist is now ON", Logger.LogMode.Warning);
            else Logger.WriteToConsole("Whitelist is now OFF", Logger.LogMode.Warning);
        }

        private static void SaveWhitelistFile()
        {
            Serializer.SerializeToFile(Path.Combine(Program.corePath, "Whitelist.json"), 
                Program.whitelist);
        }

        public static void LoadServerWhitelist()
        {
            string path = Path.Combine(Program.corePath, "Whitelist.json");

            if (File.Exists(path)) Program.whitelist = Serializer.SerializeFromFile<WhitelistFile>(path);
            else
            {
                Program.whitelist = new WhitelistFile();
                Serializer.SerializeToFile(path, Program.whitelist);
            }

            Logger.WriteToConsole("Loaded server whitelist");
        }
    }
}
