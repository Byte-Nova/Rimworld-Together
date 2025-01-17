using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class LoginManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            LoginData data = Serializer.ConvertBytesToObject<LoginData>(packet.contents);
            HandleUser(client, data);
        }

        public static void HandleUser(ServerClient client, LoginData data)
        {
            if (!UserManagerH.CheckIfUserUpdated(client, data)) return;

            if (!UserManagerH.CheckLoginData(client, data)) return;

            if (UserManagerH.CheckIfUserExists(client, data)) LoginUser(client, data);
            else RegisterUser(client, data);
        }

        public static void LoginUser(ServerClient client, LoginData data)
        {
            if (!UserManagerH.CheckIfUserAuthCorrect(client, data)) return;

            client.userFile.SetLoginDetails(data);

            client.LoadUserFromFile();

            if (UserManagerH.CheckIfUserBanned(client)) return;

            if (!UserManagerH.CheckWhitelist(client)) return;

            if (WorldManager.CheckIfWorldExists() && ModManager.CheckIfModConflict(client, data)) return;

            LoginManagerH.RemoveOldClientSessions(client);

            InformationDisplayer.DisplayLogin(client);

            PostLogin(client);
        }

        public static void RegisterUser(ServerClient client, LoginData data)
        {
            try
            {
                client.userFile.SetLoginDetails(data);

                UserManagerH.SaveUserFile(client.userFile);

                InformationDisplayer.DisplayRegister(client);

                LoginUser(client, data);
            }
            catch { LoginManagerH.SendLoginResponse(client, LoginResponse.RegisterError); }
        }

        private static void PostLogin(ServerClient client)
        {
            SiteManager.SetSiteInfoForClient(client);

            UserManager.SendPlayerRecount();

            GlobalDataManager.SendServerGlobalData(client);

            foreach (string str in ChatManager.defaultJoinMessages) ChatManager.SendConsoleMessage(client, str);

            if (Master.chatConfig.EnableMoTD) ChatManager.SendServerMessage(client, $"MoTD > {Master.chatConfig.MessageOfTheDay}");

            if (Master.chatConfig.LoginNotifications) ChatManager.BroadcastServerNotification($"{client.userFile.Uid} has joined the server!");

            if (WorldManager.CheckIfWorldExists())
            {
                if (SaveManager.CheckIfUserHasSave(client)) SaveManager.SendSavePartToClient(client);
                else WorldManager.SendWorldFile(client);
            }
            else WorldManager.RequireWorldFile(client);
        }
    }

    public static class LoginManagerH
    {
        public static void RemoveOldClientSessions(ServerClient client)
        {
            foreach (ServerClient toFind in NetworkHelper.GetConnectedClientsSafe())
            {
                if (toFind == client) continue;
                else
                {
                    if (toFind.userFile.Uid == client.userFile.Uid)
                    {
                        SendLoginResponse(toFind, LoginResponse.ExtraLogin);
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