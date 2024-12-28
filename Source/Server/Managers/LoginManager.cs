using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class LoginManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            LoginData data = Serializer.ConvertBytesToObject<LoginData>(packet.contents);

            switch (data.joinType)
            {
                case JoinType.Login:
                    LoginUser(client, data);
                    break;

                case JoinType.Register:
                    RegisterUser(client, data);
                    break;
            }
        }

        public static void LoginUser(ServerClient client, LoginData data)
        {
            if (!UserManagerHelper.CheckIfUserUpdated(client, data)) return;

            if (!UserManagerHelper.CheckLoginData(client, data, LoginMode.Login)) return;

            if (!UserManagerHelper.CheckIfUserExists(client, data, LoginMode.Login)) return;

            if (!UserManagerHelper.CheckIfUserAuthCorrect(client, data)) return;

            client.userFile.SetLoginDetails(data);

            client.LoadUserFromFile();

            Logger.Message($"[Handshake] > {client.userFile.SavedIP} | {client.userFile.Username}");

            if (UserManagerHelper.CheckIfUserBanned(client)) return;

            if (!UserManagerHelper.CheckWhitelist(client)) return;

            if (WorldManager.CheckIfWorldExists())
            {
                if (ModManager.CheckIfModConflict(client, data)) return;
            }

            RemoveOldClientIfAny(client);

            PostLogin(client);
        }

        public static void RegisterUser(ServerClient client, LoginData data)
        {
            if (!UserManagerHelper.CheckIfUserUpdated(client, data)) return;

            if (!UserManagerHelper.CheckLoginData(client, data, LoginMode.Register)) return;

            if (UserManagerHelper.CheckIfUserExists(client, data, LoginMode.Register)) return;

            try
            {
                client.userFile.SetLoginDetails(data);

                UserManagerHelper.SaveUserFile(client.userFile);

                LoginUser(client, data);

                Logger.Message($"[Registered] > {client.userFile.Username}");
            }
            catch { SendLoginResponse(client, LoginResponse.RegisterError); }
        }

        private static void PostLogin(ServerClient client)
        {
            SiteManager.SetSiteInfoForClient(client);

            UserManager.SendPlayerRecount();

            GlobalDataManager.SendServerGlobalData(client);

            foreach(string str in ChatManager.defaultJoinMessages) ChatManager.SendConsoleMessage(client, str);
            
            if (Master.chatConfig.EnableMoTD) ChatManager.SendServerMessage(client, $"MoTD > {Master.chatConfig.MessageOfTheDay}");
            
            if (Master.chatConfig.LoginNotifications) ChatManager.BroadcastServerNotification($"{client.userFile.Username} has joined the server!");

            if (WorldManager.CheckIfWorldExists())
            {
                if (SaveManager.CheckIfUserHasSave(client)) SaveManager.SendSavePartToClient(client);
                else WorldManager.SendWorldFile(client);
            }
            else WorldManager.RequireWorldFile(client);
        }

        private static void RemoveOldClientIfAny(ServerClient client)
        {
            foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
            {
                if (cClient == client) continue;
                else
                {
                    if (cClient.userFile.Username == client.userFile.Username)
                    {
                        SendLoginResponse(cClient, LoginResponse.ExtraLogin);
                    }
                }
            }
        }

        public static void SendLoginResponse(ServerClient client, LoginResponse response, object extraDetails = null)
        {
            LoginData loginData = new LoginData();
            loginData._tryResponse = response;

            if (response == LoginResponse.WrongMods) loginData._extraDetails = (List<string>)extraDetails;
            else if (response == LoginResponse.WrongVersion) loginData._extraDetails = new List<string>() { CommonValues.executableVersion };

            Packet packet = Packet.CreatePacketFromObject(nameof(LoginManager), loginData);
            client.listener.EnqueuePacket(packet);
            client.listener.disconnectFlag = true;
        }
    }
}