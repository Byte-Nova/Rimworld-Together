using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using static Shared.Misc.CommonEnumerators;

namespace RimworldTogether.GameServer.Managers
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
            string[] userFiles = Directory.GetFiles(Program.usersPath);

            foreach(string userFile in userFiles)
            {
                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.username == client.username) return file;
            }

            return null;
        }

        public static UserFile GetUserFileFromName(string username)
        {
            string[] userFiles = Directory.GetFiles(Program.usersPath);

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

            string[] paths = Directory.GetFiles(Program.usersPath);
            foreach (string path in paths) userFiles.Add(Serializer.SerializeFromFile<UserFile>(path));
            return userFiles.ToArray();
        }

        public static void SaveUserFile(ServerClient client, UserFile userFile)
        {
            string savePath = Path.Combine(Program.usersPath, client.username + ".json");
            Serializer.SerializeToFile(savePath, userFile);
        }

        public static void SaveUserFileFromName(string username, UserFile userFile)
        {
            string savePath = Path.Combine(Program.usersPath, username + ".json");
            Serializer.SerializeToFile(savePath, userFile);
        }

        public static void SendPlayerRecount()
        {
            PlayerRecountJSON playerRecountJSON = new PlayerRecountJSON();
            playerRecountJSON.currentPlayers = Network.Network.connectedClients.ToArray().Count().ToString();
            foreach(ServerClient client in Network.Network.connectedClients.ToArray()) playerRecountJSON.currentPlayerNames.Add(client.username);

            Packet packet = Packet.CreatePacketFromJSON("PlayerRecountPacket", playerRecountJSON);
            foreach (ServerClient client in Network.Network.connectedClients.ToArray()) client.clientListener.SendData(packet);
        }

        public static bool CheckIfUserIsConnected(string username)
        {
            List<ServerClient> connectedClients = Network.Network.connectedClients.ToList();

            ServerClient toGet = connectedClients.Find(x => x.username == username);
            if (toGet != null) return true;
            else return false;
        }

        public static ServerClient GetConnectedClientFromUsername(string username)
        {
            List<ServerClient> connectedClients = Network.Network.connectedClients.ToList();
            return connectedClients.Find(x => x.username == username);
        }

        public static bool CheckIfUserExists(ServerClient client)
        {
            string[] existingUsers = Directory.GetFiles(Program.usersPath);

            foreach (string user in existingUsers)
            {
                UserFile existingUser = Serializer.SerializeFromFile<UserFile>(user);
                if (existingUser.username != client.username) continue;
                else
                {
                    if (existingUser.password == client.password) return true;
                    else
                    {
                        UserManager_Joinings.SendLoginResponse(client, LoginResponse.InvalidLogin);

                        return false;
                    }
                }
            }

            UserManager_Joinings.SendLoginResponse(client, LoginResponse.InvalidLogin);

            return false;
        }

        public static bool CheckIfUserBanned(ServerClient client)
        {
            if (!client.isBanned) return false;
            else
            {
                UserManager_Joinings.SendLoginResponse(client, LoginResponse.BannedLogin);
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
    }

    public static class UserManager_Joinings
    {
        public static bool CheckLoginDetails(ServerClient client, LoginMode mode)
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace(client.username)) isInvalid = true;
            if (client.username.Any(Char.IsWhiteSpace)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(client.password)) isInvalid = true;
            if (client.username.Length > 32) isInvalid = true;

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

            if (response == LoginResponse.WrongMods) loginDetailsJSON.conflictingMods = (List<string>)extraDetails;

            Packet packet = Packet.CreatePacketFromJSON("LoginResponsePacket", loginDetailsJSON);
            client.clientListener.SendData(packet);

            client.disconnectFlag = true;
        }

        public static bool CheckWhitelist(ServerClient client)
        {
            if (!Program.whitelist.UseWhitelist) return true;
            else
            {
                foreach(string str in Program.whitelist.WhitelistedUsers)
                {
                    if (str == client.username) return true;
                }
            }

            SendLoginResponse(client, LoginResponse.Whitelist);

            return false;
        }
    }
}
