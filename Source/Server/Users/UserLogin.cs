using Shared;

namespace GameServer
{
    public static class UserLogin
    {
        public static void TryLoginUser(ServerClient client, Packet packet)
        {
            LoginData loginData = (LoginData)Serializer.ConvertBytesToObject(packet.contents);

            if (!UserManager.CheckIfUserUpdated(client, loginData)) return;

            if (!UserManager.CheckLoginData(client, loginData, CommonEnumerators.LoginMode.Login)) return;

            if (!UserManager.CheckIfUserExists(client, loginData, CommonEnumerators.LoginMode.Login)) return;

            if (!UserManager.CheckIfUserAuthCorrect(client, loginData)) return;

            client.username = loginData.username;
            client.password = loginData.password;

            UserManager.LoadDataFromFile(client);

            if (UserManager.CheckIfUserBanned(client)) return;

            if (!UserManager.CheckWhitelist(client)) return;

            if (ModManager.CheckIfModConflict(client, loginData)) return;

            RemoveOldClientIfAny(client);

            PostLogin(client);
        }

        private static void PostLogin(ServerClient client)
        {
            UserManager.SaveUserIP(client);

            UserManager.SendPlayerRecount();

            ServerOverallManager.SendServerOveralls(client);

            ChatManager.BroadcastSystemMessage(client, ChatManager.defaultJoinMessages);

            if (WorldManager.CheckIfWorldExists())
            {
                if (SaveManager.CheckIfUserHasSave(client)) SaveManager.SendSavePartToClient(client);
                else WorldManager.SendWorldFile(client);
            }
            else WorldManager.RequireWorldFile(client);
        }

        private static void RemoveOldClientIfAny(ServerClient client)
        {
            foreach (ServerClient cClient in Network.connectedClients.ToArray())
            {
                if (cClient == client) continue;
                else
                {
                    if (cClient.username == client.username)
                    {
                        UserManager.SendLoginResponse(cClient, CommonEnumerators.LoginResponse.ExtraLogin);
                    }
                }
            }
        }
    }
}
