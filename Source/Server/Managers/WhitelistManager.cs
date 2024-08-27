using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class WhitelistManager
    {
        public static void AddUserToWhitelist(string username)
        {
            Master.whitelist.WhitelistedUsers.Add(username);

            Main_.SaveValueFile(ServerFileMode.Whitelist);

            Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' has been whitelisted");
        }

        public static void RemoveUserFromWhitelist(string username)
        {
            Master.whitelist.WhitelistedUsers.Remove(username);

            Main_.SaveValueFile(ServerFileMode.Whitelist);

            Logger.Warning($"User '{ConsoleCommandManager.commandParameters[0]}' is no longer whitelisted");
        }

        public static void ToggleWhitelist()
        {
            Master.whitelist.UseWhitelist = !Master.whitelist.UseWhitelist;

            Main_.SaveValueFile(ServerFileMode.Whitelist);

            if (Master.whitelist.UseWhitelist) Logger.Warning("Whitelist is now ON");
            else Logger.Warning("Whitelist is now OFF");
        }
    }
}
