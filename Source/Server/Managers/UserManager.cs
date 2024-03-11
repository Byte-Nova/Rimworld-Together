using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class UserManager
    {
        public static void LoadDataFromFile(ServerClient client)
        {
            UserFile file = GetUserFile(client);
            client.uid = file.uid;
            client.username = file.username;
            client.password = file.password;
            client.factionName = file.factionName;
            client.hasFaction = file.hasFaction;
            client.isAdmin = file.isAdmin;
            client.isBanned = file.isBanned;
            client.enemyPlayers = file.enemyPlayers;
            client.allyPlayers = file.allyPlayers;

            Logger.WriteToConsole($"[Handshake] > {client.username} | {client.SavedIP}");
        }

        public static UserFile GetUserFile(ServerClient client)
        {
            string[] userFiles = Directory.GetFiles(Master.usersPath);

            foreach(string userFile in userFiles)
            {
                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.username == client.username) return file;
            }

            return null;
        }

        public static UserFile GetUserFileFromName(string username)
        {
            string[] userFiles = Directory.GetFiles(Master.usersPath);

            foreach (string userFile in userFiles)
            {
                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.username == username) return file;
            }

            return null;
        }

        public static UserFile[] GetAllUserFiles()
        {
            List<UserFile> userFiles = new List<UserFile>();

            string[] paths = Directory.GetFiles(Master.usersPath);
            foreach (string path in paths) userFiles.Add(Serializer.SerializeFromFile<UserFile>(path));
            return userFiles.ToArray();
        }

        public static void SaveUserFile(ServerClient client, UserFile userFile)
        {
            string savePath = Path.Combine(Master.usersPath, client.username + ".json");
            Serializer.SerializeToFile(savePath, userFile);
        }

        public static void SaveUserFileFromName(string username, UserFile userFile)
        {
            string savePath = Path.Combine(Master.usersPath, username + ".json");
            Serializer.SerializeToFile(savePath, userFile);
        }

        public static void SendPlayerRecount()
        {
            PlayerRecountJSON playerRecountJSON = new PlayerRecountJSON();
            playerRecountJSON.currentPlayers = Network.connectedClients.ToArray().Count().ToString();
            foreach(ServerClient client in Network.connectedClients.ToArray()) playerRecountJSON.currentPlayerNames.Add(client.username);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.PlayerRecountPacket), playerRecountJSON);
            foreach (ServerClient client in Network.connectedClients.ToArray()) client.listener.EnqueuePacket(packet);
        }

        public static bool CheckIfUserIsConnected(string username)
        {
            List<ServerClient> connectedClients = Network.connectedClients.ToList();

            ServerClient toGet = connectedClients.Find(x => x.username == username);
            if (toGet != null) return true;
            else return false;
        }

        public static ServerClient GetConnectedClientFromUsername(string username)
        {
            List<ServerClient> connectedClients = Network.connectedClients.ToList();
            return connectedClients.Find(x => x.username == username);
        }

        public static bool CheckIfUserExists(ServerClient client, JoinDetailsJSON details, LoginMode mode)
        {
            string[] existingUsers = Directory.GetFiles(Master.usersPath);

            foreach (string user in existingUsers)
            {
                UserFile existingUser = Serializer.SerializeFromFile<UserFile>(user);
                if (existingUser.username.ToLower() == details.username.ToLower())
                {
                    if (mode == LoginMode.Register) SendLoginResponse(client, LoginResponse.RegisterInUse);
                    return true;
                }
            }

            if (mode == LoginMode.Login) SendLoginResponse(client, LoginResponse.InvalidLogin);
            return false;
        }

        public static bool CheckIfUserAuthCorrect(ServerClient client, JoinDetailsJSON details)
        {
            string[] existingUsers = Directory.GetFiles(Master.usersPath);

            foreach (string user in existingUsers)
            {
                UserFile existingUser = Serializer.SerializeFromFile<UserFile>(user);
                if (existingUser.username == details.username)
                {
                    if (existingUser.password == details.password) return true;
                    else break;
                }
            }

            SendLoginResponse(client, LoginResponse.InvalidLogin);
            return false;
        }

        public static bool CheckIfUserBanned(ServerClient client)
        {
            if (!client.isBanned) return false;
            else
            {
                SendLoginResponse(client, LoginResponse.BannedLogin);
                return true;
            }
        }

        public static void SaveUserIP(ServerClient client)
        {
            UserFile userFile = GetUserFile(client);
            userFile.SavedIP = client.SavedIP;
            SaveUserFile(client, userFile);
        }

        public static string[] GetUserStructuresTilesFromUsername(string username)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements().ToList().FindAll(x => x.owner == username).ToArray();
            SiteFile[] sites = SiteManager.GetAllSites().ToList().FindAll(x => x.owner == username).ToArray();

            List<string> tilesToExclude = new List<string>();
            foreach (SettlementFile settlement in settlements) tilesToExclude.Add(settlement.tile);
            foreach (SiteFile site in sites) tilesToExclude.Add(site.tile);

            return tilesToExclude.ToArray();
        }

        public static bool CheckLoginDetails(ServerClient client, JoinDetailsJSON details, LoginMode mode)
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace(details.username)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(details.password)) isInvalid = true;
            if (details.username.Any(Char.IsWhiteSpace)) isInvalid = true;
            if (details.username.Length > 32) isInvalid = true;
            if (details.password.Length > 64) isInvalid = true;

            if (!isInvalid) return true;
            else
            {
                if (mode == LoginMode.Login) SendLoginResponse(client, LoginResponse.InvalidLogin);
                else if (mode == LoginMode.Register) SendLoginResponse(client, LoginResponse.RegisterError);
                return false;
            }
        }

        public static void SendLoginResponse(ServerClient client, LoginResponse response, object extraDetails = null)
        {
            JoinDetailsJSON loginDetailsJSON = new JoinDetailsJSON();
            loginDetailsJSON.tryResponse = ((int)response).ToString();

            if (response == LoginResponse.WrongMods) loginDetailsJSON.extraDetails = (List<string>)extraDetails;
            else if (response == LoginResponse.WrongVersion) loginDetailsJSON.extraDetails = new List<string>() { CommonValues.executableVersion };

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.LoginResponsePacket), loginDetailsJSON);
            client.listener.EnqueuePacket(packet);
            client.listener.disconnectFlag = true;
        }

        public static bool CheckWhitelist(ServerClient client)
        {
            if (!Master.whitelist.UseWhitelist) return true;
            else
            {
                foreach (string str in Master.whitelist.WhitelistedUsers)
                {
                    if (str == client.username) return true;
                }
            }

            SendLoginResponse(client, LoginResponse.Whitelist);
            return false;
        }

        public static bool CheckIfUserUpdated(ServerClient client, JoinDetailsJSON loginDetails)
        {
            if (loginDetails.clientVersion == CommonValues.executableVersion) return true;
            else
            {
                Logger.WriteToConsole($"[Version Mismatch] > {client.username}", Logger.LogMode.Warning);
                SendLoginResponse(client, LoginResponse.WrongVersion);
                return false;
            }
        }
    }
}
