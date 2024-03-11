using Shared;

namespace GameServer
{
    public static class UserLogin
    {
        public static void TryLoginUser(ServerClient client, Packet packet)
        {
            JoinDetailsJSON loginDetails = (JoinDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            if (!UserManager.CheckIfUserUpdated(client, loginDetails)) return;

            if (!UserManager.CheckLoginDetails(client, loginDetails, CommonEnumerators.LoginMode.Login)) return;

            if (!UserManager.CheckIfUserExists(client, loginDetails, CommonEnumerators.LoginMode.Login)) return;

            if (!UserManager.CheckIfUserAuthCorrect(client, loginDetails)) return;

            client.username = loginDetails.username;
            client.password = loginDetails.password;

            UserManager.LoadDataFromFile(client);

            if (UserManager.CheckIfUserBanned(client)) return;

            if (!UserManager.CheckWhitelist(client)) return;

            if (ModManager.CheckIfModConflict(client, loginDetails)) return;

            RemoveOldClientIfAny(client);

            PostLogin(client);
        }

        private static void PostLogin(ServerClient client)
        {
            UserManager.SaveUserIP(client);

            UserManager.SendPlayerRecount();

            ServerOverallManager.SendServerOveralls(client);

            ChatManager.SendMessagesToClient(client, ChatManager.defaultJoinMessages);

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
