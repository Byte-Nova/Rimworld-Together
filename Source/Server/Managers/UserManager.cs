using GameServer.Commands;
using GameServer.Core;
using GameServer.Misc;
using TCPNetwork.Packets;
using Shared;
using Shared.Files;
using static Shared.CommonEnumerators;
using TCPNetwork.Files.Client;
using Shared.Files.Sites;
using Shared.Misc;
using GameServer.Hooks.TCPNetwork;

namespace GameServer.Managers
{

    public static class UserManager
    {
        public static void SendPlayerRecount()
        {
            PlayerRecountData playerRecountData = new PlayerRecountData();
            playerRecountData._currentPlayerCount = ServerNetwork.Instance.GetConnectedClientsSafe().Count();
            foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe()) playerRecountData._currentPlayerNames.Add(client.UserFile.Username);

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.RecountManager, playerRecountData);
        }

        public static void BanPlayerFromName(string username)
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(username);
            ServerClient client = ServerNetwork.Instance.GetConnectedClientFromUsername(username);
            if (userFile == null || client == null) ConsoleCommandActions.ThrowUserNotFoundError();
            else
            {
                if (userFile.IsBanned) Printer.Warning($"User '{userFile.Username}' is already banned from the server");
                else
                {
                    userFile.UpdateBan(true);
                    client.Listener.DisconnectNow();
                    Printer.Warning($"User '{userFile.Username}' has been banned from the server");
                }
            }
        }

        public static void PardonPlayerFromName(string username)
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(username);
            if (userFile == null) ConsoleCommandActions.ThrowUserNotFoundError();
            else
            {
                if (!userFile.IsBanned) Printer.Warning($"User '{userFile.Username}' is not banned from the server");
                else
                {
                    userFile.UpdateBan(false);
                    Printer.Warning($"User '{userFile.Username}' has been pardoned from the server");
                }
            }
        }
    }

    public static class UserManagerH
    {
        public static UserFile GetUserFile(ServerClient client)
        {
            string[] userFiles = Directory.GetFiles(Master.UsersPath);

            foreach (string userFile in userFiles)
            {
                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Username == client.UserFile.Username) return file;
            }

            return null;
        }

        public static UserFile GetUserFileFromName(string username)
        {
            string[] userFiles = Directory.GetFiles(Master.UsersPath);

            foreach (string userFile in userFiles)
            {
                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Username == username) return file;
            }

            return null;
        }

        public static UserFile[] GetAllUserFiles()
        {
            List<UserFile> userFiles = new List<UserFile>();

            string[] existingUsers = Directory.GetFiles(Master.UsersPath);
            foreach (string user in existingUsers) userFiles.Add(Serializer.SerializeFromFile<UserFile>(user));
            return userFiles.ToArray();
        }

        public static bool CheckIfUserIsConnected(string username)
        {
            ServerClient toGet = ServerNetwork.Instance.GetConnectedClientFromUsername(username);
            if (toGet != null) return true;
            else return false;
        }

        public static bool CheckIfUserExists(ServerClient client, LoginData data)
        {
            UserFile toFind = GetAllUserFiles().FirstOrDefault(fetch => fetch.Username.ToLower() == data._username.ToLower());
            if (toFind != null) return true;
            else return false;
        }

        public static bool CheckIfUserAuthCorrect(ServerClient client, LoginData data)
        {
            UserFile toFind = GetAllUserFiles().FirstOrDefault(fetch => fetch.Username == data._username && fetch.Password == data._password);
            if (toFind != null) return true;
            else
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.Invalid);
                return false;

            }
        }

        public static bool CheckIfUserBanned(ServerClient client)
        {
            if (!client.UserFile.IsBanned) return false;
            else
            {
                Printer.Message($"Banned user '{client.UserFile.Username}' tried to join the server");
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.Ban);
                return true;
            }
        }

        public static bool CheckWhitelist(ServerClient client)
        {
            if (!Master.Whitelist.UseWhitelist) return true;
            else if (Master.Whitelist.WhitelistedUsers.ToArray().First(fetch => fetch == client.UserFile.Username) != null) return true;
            else
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.Whitelist);
                return false;
            }
        }

        public static int[] GetUserStructuresTilesFromUsername(string username)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements().ToList().FindAll(x => x.Username == username).ToArray();
            SiteFile[] sites = SiteManagerHelper.GetAllSites().ToList().FindAll(x => x.Username == username).ToArray();

            List<int> tilesToExclude = new List<int>();
            foreach (SettlementFile settlement in settlements) tilesToExclude.Add(settlement.Tile);
            foreach (SiteFile site in sites) tilesToExclude.Add(site.Tile);

            return tilesToExclude.ToArray();
        }
    }
}
