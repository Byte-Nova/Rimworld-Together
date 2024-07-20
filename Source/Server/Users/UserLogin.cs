using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class UserLogin
    {
        public static void TryLoginUser(ServerClient client, Packet packet)
        {
            LoginData loginData = Serializer.ConvertBytesToObject<LoginData>(packet.contents);

            if (!UserManager.CheckIfUserUpdated(client, loginData)) return;

            if (!UserManager.CheckLoginData(client, loginData, LoginMode.Login)) return;

            if (!UserManager.CheckIfUserExists(client, loginData, LoginMode.Login)) return;

            if (!UserManager.CheckIfUserAuthCorrect(client, loginData)) return;

            client.userFile.SetLoginDetails(loginData);

            client.LoadFromUserFile();

            Logger.Message($"[Handshake] > {client.userFile.SavedIP} | {client.userFile.Username}");

            if (UserManager.CheckIfUserBanned(client)) return;

            if (!UserManager.CheckWhitelist(client)) return;

            if (ModManager.CheckIfModConflict(client, loginData)) return;

            RemoveOldClientIfAny(client);

            PostLogin(client);
        }

        private static void PostLogin(ServerClient client)
        {
            UserManager.SendPlayerRecount();

            ServerGlobalDataManager.SendServerGlobalData(client);

            ChatManager.BroadcastSystemMessages(client, ChatManager.defaultJoinMessages);

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
                    if (cClient.userFile.Username == client.userFile.Username)
                    {
                        UserManager.SendLoginResponse(cClient, LoginResponse.ExtraLogin);
                    }
                }
            }
        }
    }
}
