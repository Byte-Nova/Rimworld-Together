using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class UserManager
    {
        public static void SendPlayerRecount()
        {
            PlayerRecountData playerRecountData = new PlayerRecountData();
            playerRecountData._currentPlayers = NetworkHelper.GetConnectedClientsSafe().Count().ToString();
            foreach(ServerClient client in NetworkHelper.GetConnectedClientsSafe()) playerRecountData._currentPlayerNames.Add(client.userFile.Username);

            Packet packet = Packet.CreatePacketFromObject(nameof(PlayerRecountManager), playerRecountData);
            NetworkHelper.SendPacketToAllClients(packet);
        }
    }

    public static class UserManagerHelper
    {
        //Variables

        public readonly static string fileExtension = ".mpuser";

        public static UserFile GetUserFile(ServerClient client)
        {
            string[] userFiles = Directory.GetFiles(Master.usersPath);

            foreach(string userFile in userFiles)
            {
                if (!userFile.EndsWith(fileExtension)) continue;

                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Username == client.userFile.Username) return file;
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
                if (file.Username == username) return file;
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
            List<ServerClient> connectedClients = Network.connectedClients.ToList();

            ServerClient toGet = connectedClients.Find(x => x.userFile.Username == username);
            if (toGet != null) return true;
            else return false;
        }

        public static ServerClient GetConnectedClientFromUsername(string username)
        {
            List<ServerClient> connectedClients = Network.connectedClients.ToList();
            return connectedClients.Find(x => x.userFile.Username == username);
        }

        public static bool CheckIfUserExists(ServerClient client, LoginData data, LoginMode mode)
        {
            string[] existingUsers = Directory.GetFiles(Master.usersPath);

            foreach (string user in existingUsers)
            {
                if (!user.EndsWith(fileExtension)) continue;

                UserFile existingUser = Serializer.SerializeFromFile<UserFile>(user);
                if (existingUser.Username.ToLower() == data._username.ToLower())
                {
                    if (mode == LoginMode.Register) LoginManager.SendLoginResponse(client, LoginResponse.RegisterInUse);
                    return true;
                }
            }

            if (mode == LoginMode.Login) LoginManager.SendLoginResponse(client, LoginResponse.InvalidLogin);
            return false;
        }

        public static bool CheckIfUserAuthCorrect(ServerClient client, LoginData data)
        {
            string[] existingUsers = Directory.GetFiles(Master.usersPath);

            foreach (string user in existingUsers)
            {
                if (!user.EndsWith(fileExtension)) continue;
                UserFile existingUser = Serializer.SerializeFromFile<UserFile>(user);
                if (existingUser.Username == data._username)
                {
                    if (existingUser.Password == data._password) return true;
                    else break;
                }
            }

            LoginManager.SendLoginResponse(client, LoginResponse.InvalidLogin);
            return false;
        }

        public static bool CheckIfUserBanned(ServerClient client)
        {
            if (!client.userFile.IsBanned) return false;
            else
            {
                LoginManager.SendLoginResponse(client, LoginResponse.BannedLogin);
                return true;
            }
        }

        public static bool CheckLoginData(ServerClient client, LoginData data, LoginMode mode)
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace(data._username)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(data._password)) isInvalid = true;
            if (data._username.Any(Char.IsWhiteSpace)) isInvalid = true;
            if (data._username.Length > 32) isInvalid = true;
            if (data._password.Length > 64) isInvalid = true;

            if (!isInvalid) return true;
            else
            {
                if (mode == LoginMode.Login) LoginManager.SendLoginResponse(client, LoginResponse.InvalidLogin);
                else if (mode == LoginMode.Register) LoginManager.SendLoginResponse(client, LoginResponse.RegisterError);
                return false;
            }
        }

        public static bool CheckWhitelist(ServerClient client)
        {
            if (!Master.whitelist.UseWhitelist) return true;
            else
            {
                foreach (string str in Master.whitelist.WhitelistedUsers)
                {
                    if (str == client.userFile.Username) return true;
                }
            }

            LoginManager.SendLoginResponse(client, LoginResponse.Whitelist);
            return false;
        }

        public static bool CheckIfUserUpdated(ServerClient client, LoginData loginData)
        {
            if (loginData._version == CommonValues.executableVersion) return true;
            else
            {
                Logger.Warning($"[Version Mismatch] > {client.userFile.Username}");
                LoginManager.SendLoginResponse(client, LoginResponse.WrongVersion);
                return false;
            }
        }

        public static int[] GetUserStructuresTilesFromUsername(string username)
        {
            SettlementFile[] settlements = PlayerSettlementManager.GetAllSettlements().ToList().FindAll(x => x.Owner == username).ToArray();
            SiteIdendity[] sites = SiteManagerHelper.GetAllSites().ToList().FindAll(x => x.Owner == username).ToArray();

            List<int> tilesToExclude = new List<int>();
            foreach (SettlementFile settlement in settlements) tilesToExclude.Add(settlement.Tile);
            foreach (SiteIdendity site in sites) tilesToExclude.Add(site.Tile);

            return tilesToExclude.ToArray();
        }

        public static void SaveUserFile(UserFile userFile)
        {
            userFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.usersPath, userFile.Username + fileExtension), userFile); }
            catch (Exception e) { Logger.Error(e.ToString()); }
            
            userFile.SavingSemaphore.Release();
        }
    }
}
