using GameServer.Core;
using GameServer.Files;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class UserManager
    {
        public static void SendPlayerRecount()
        {
            PlayerRecountData playerRecountData = new PlayerRecountData();
            playerRecountData._currentPlayers = NetworkHelper.GetConnectedClientsSafe().Count().ToString();
            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe()) playerRecountData._currentPlayerNames.Add(client.userFile.Label);

            Packet packet = Packet.CreatePacketFromObject(nameof(PlayerRecountManager), playerRecountData);
            NetworkHelper.SendPacketToAllClients(packet);
        }

        public static void BanPlayerFromName(string uid)
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(uid);
            ServerClient client = NetworkHelper.GetConnectedClientFromUid(uid);
            if (userFile == null || client == null) Printer.Warning($"User '{uid}' couldn't be found");
            else
            {
                if (userFile.IsBanned) Printer.Warning($"User '{userFile.Label}' is already banned from the server");
                else
                {
                    userFile.UpdateBan(true);
                    client.listener.disconnectFlag = true;
                    Printer.Warning($"User '{userFile.Label}' has been banned from the server");
                }
            }
        }

        public static void PardonPlayerFromName(string uid)
        {
            UserFile userFile = UserManagerH.GetUserFileFromName(uid);
            if (userFile == null) Printer.Warning($"User '{uid}' couldn't be found");
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
            string[] userFiles = Directory.GetFiles(Master.usersPath);

            foreach (string userFile in userFiles)
            {
                if (!userFile.EndsWith(fileExtension)) continue;

                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Uid == client.userFile.Uid) return file;
            }

            return null;
        }

        public static UserFile GetUserFileFromName(string username)
        {
            string[] userFiles = Directory.GetFiles(Master.usersPath);

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

            string[] existingUsers = Directory.GetFiles(Master.usersPath);
            foreach (string user in existingUsers)
            {
                if (!user.EndsWith(fileExtension)) continue;
                userFiles.Add(Serializer.SerializeFromFile<UserFile>(user));
            }
            return userFiles.ToArray();
        }

        public static bool CheckIfUserIsConnected(string username)
        {
            ServerClient toGet = NetworkHelper.GetConnectedClientFromUid(username);
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
                LoginManagerH.SendLoginResponse(client, LoginResponse.InvalidLogin);
                return false;
            }
        }

        public static bool CheckIfUserBanned(ServerClient client)
        {
            if (!client.userFile.IsBanned) return false;
            else
            {
                Printer.Message($"Banned user '{client.userFile.Uid}' tried to join the server");
                LoginManagerH.SendLoginResponse(client, LoginResponse.BannedLogin);
                return true;
            }
        }

        public static bool CheckLoginData(ServerClient client, LoginData data)
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace(data._uid)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(data._username)) isInvalid = true;
            if (data._username.Any(char.IsWhiteSpace)) isInvalid = true;
            if (data._username.Length > 32) isInvalid = true;
            if (data._uid.Length > 128) isInvalid = true;

            if (!isInvalid) return true;
            else
            {
                LoginManagerH.SendLoginResponse(client, LoginResponse.InvalidLogin);
                return false;
            }
        }

        public static bool CheckWhitelist(ServerClient client)
        {
            if (!Master.whitelist.UseWhitelist) return true;
            else if (Master.whitelist.WhitelistedUsers.ToArray().First(fetch => fetch == client.userFile.Uid) != null) return true;
            else
            {
                LoginManagerH.SendLoginResponse(client, LoginResponse.Whitelist);
                return false;
            }
        }

        public static bool CheckIfUserUpdated(ServerClient client, LoginData loginData)
        {
            if (loginData._version == CommonValues.executableVersion) return true;
            else
            {
                InformationDisplayer.DisplayVersionMismatch(client.userFile.Label);
                LoginManagerH.SendLoginResponse(client, LoginResponse.WrongVersion);
                return false;
            }
        }

        public static int[] GetUserStructuresTilesFromUsername(string username)
        {
            SettlementFile[] settlements = PlayerSettlementManager.GetAllSettlements().ToList().FindAll(x => x.UID == username).ToArray();
            SiteFile[] sites = SiteManagerHelper.GetAllSites().ToList().FindAll(x => x.UID == username).ToArray();

            List<int> tilesToExclude = new List<int>();
            foreach (SettlementFile settlement in settlements) tilesToExclude.Add(settlement.Tile);
            foreach (SiteFile site in sites) tilesToExclude.Add(site.Tile);

            return tilesToExclude.ToArray();
        }

        public static void SaveUserFile(UserFile userFile)
        {
            userFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.usersPath, userFile.Uid + fileExtension), userFile); }
            catch (Exception e) { Printer.Error(e.ToString()); }

            userFile.SavingSemaphore.Release();
        }
    }
}
