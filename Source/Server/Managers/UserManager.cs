using GameServer.Commands;
using GameServer.Core;
using GameServer.Files;
using GameServer.Misc;
using TCPNetwork.Packets;
using TCPNetwork.Server;
using Shared;
using Shared.Files;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class UserManager
    {
        public static void SendPlayerRecount()
        {
            PlayerRecountData playerRecountData = new PlayerRecountData();
            playerRecountData._currentPlayerCount = ServerNetwork.Instance.GetConnectedClientsSafe().Count();
            foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe()) playerRecountData._currentPlayerNames.Add(client.UserFile.Label);

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.RecountManager, playerRecountData);
        }

        public static void BanPlayerFromName(string uid)
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(uid);
            ServerClient client = ServerNetwork.Instance.GetConnectedClientFromUid(uid);
            if (userFile == null || client == null) ConsoleCommandActions.ThrowUserNotFoundError();
            else
            {
                if (userFile.IsBanned) Printer.Warning($"User '{userFile.Label}' is already banned from the server");
                else
                {
                    userFile.UpdateBan(true);
                    client.Listener.DisconnectFlag = true;
                    Printer.Warning($"User '{userFile.Label}' has been banned from the server");
                }
            }
        }

        public static void PardonPlayerFromName(string uid)
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(uid);
            if (userFile == null) ConsoleCommandActions.ThrowUserNotFoundError();
            else
            {
                if (!userFile.IsBanned) Printer.Warning($"User '{userFile.Label}' is not banned from the server");
                else
                {
                    userFile.UpdateBan(false);
                    Printer.Warning($"User '{userFile.Label}' has been pardoned from the server");
                }
            }
        }
    }

    public static class UserManagerH
    {
        //Variables

        public readonly static string fileExtension = ".mpuser";

        public static UserFile GetUserFile(ServerClient client)
        {
            string[] userFiles = Directory.GetFiles(Master.UsersPath);

            foreach (string userFile in userFiles)
            {
                if (!userFile.EndsWith(fileExtension)) continue;

                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Uid == client.UserFile.Uid) return file;
            }

            return null;
        }

        public static UserFile GetUserFileFromName(string username)
        {
            string[] userFiles = Directory.GetFiles(Master.UsersPath);

            foreach (string userFile in userFiles)
            {
                if (!userFile.EndsWith(fileExtension)) continue;

                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Uid == username) return file;
            }

            return null;
        }

        public static UserFile[] GetAllUserFiles()
        {
            List<UserFile> userFiles = new List<UserFile>();

            string[] existingUsers = Directory.GetFiles(Master.UsersPath);
            foreach (string user in existingUsers)
            {
                if (!user.EndsWith(fileExtension)) continue;
                userFiles.Add(Serializer.SerializeFromFile<UserFile>(user));
            }
            return userFiles.ToArray();
        }

        public static bool CheckIfUserIsConnected(string username)
        {
            ServerClient toGet = ServerNetwork.Instance.GetConnectedClientFromUid(username);
            if (toGet != null) return true;
            else return false;
        }

        public static bool CheckIfUserExists(ServerClient client, LoginData data)
        {
            UserFile toFind = GetAllUserFiles().FirstOrDefault(fetch => fetch.Uid == data._uid);
            if (toFind != null) return true;
            else return false;
        }

        public static bool CheckIfUserAuthCorrect(ServerClient client, LoginData data)
        {
            UserFile toFind = GetAllUserFiles().FirstOrDefault(fetch => fetch.Uid == data._uid);
            if (toFind != null) return true;
            else
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.InvalidLogin);
                return false;
            }
        }

        public static bool CheckIfUserBanned(ServerClient client)
        {
            if (!client.UserFile.IsBanned) return false;
            else
            {
                Printer.Message($"Banned user '{client.UserFile.Uid}' tried to join the server");
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.BannedLogin);
                return true;
            }
        }

        public static bool CheckLoginData(ServerClient client, LoginData data)
        {
            bool isInvalid = false;

            if (!StringChecker.CheckIfStringValid(data._uid)) isInvalid = true;
            if (!StringChecker.CheckIfStringValid(data._username)) isInvalid = true;

            if (data._username.Any(char.IsWhiteSpace)) isInvalid = true;
            if (data._username.Length > 32) isInvalid = true;
            if (data._uid.Length > 64) isInvalid = true;

            if (!isInvalid) return true;
            else
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.InvalidLogin);
                return false;
            }
        }

        public static bool CheckWhitelist(ServerClient client)
        {
            if (!Master.Whitelist.UseWhitelist) return true;
            else if (Master.Whitelist.WhitelistedUsers.ToArray().First(fetch => fetch == client.UserFile.Uid) != null) return true;
            else
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.Whitelist);
                return false;
            }
        }

        public static int[] GetUserStructuresTilesFromUsername(string username)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements().ToList().FindAll(x => x.UID == username).ToArray();
            SiteFile[] sites = SiteManagerHelper.GetAllSites().ToList().FindAll(x => x.UID == username).ToArray();

            List<int> tilesToExclude = new List<int>();
            foreach (SettlementFile settlement in settlements) tilesToExclude.Add(settlement.Tile);
            foreach (SiteFile site in sites) tilesToExclude.Add(site.Tile);

            return tilesToExclude.ToArray();
        }

        public static void SaveUserFile(UserFile userFile)
        {
            userFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.UsersPath, userFile.Uid + fileExtension), userFile); }
            catch (Exception e) { Printer.Error(e.ToString()); }

            userFile.SavingSemaphore.Release();
        }
    }
}
