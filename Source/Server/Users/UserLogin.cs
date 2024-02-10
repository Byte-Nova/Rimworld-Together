using RimworldTogether.GameServer.Managers;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameServer.Users
{
    public static class UserLogin
    {
        public static void TryLoginUser(ServerClient client, Packet packet)
        {
            JoinDetailsJSON loginDetails = (JoinDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            client.username = loginDetails.username;
            client.password = loginDetails.password;

            if (!UserManager_Joinings.CheckWhitelist(client)) return;

            if (!UserManager_Joinings.CheckLoginDetails(client, CommonEnumerators.LoginMode.Login)) return;

            if (!UserManager.CheckIfUserExists(client)) return;

            UserManager.LoadDataFromFile(client);

            if (ModManager.CheckIfModConflict(client, loginDetails)) return;

            if (UserManager.CheckIfUserBanned(client)) return;

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
            foreach (ServerClient cClient in Network.Network.connectedClients.ToArray())
            {
                if (cClient == client) continue;
                else
                {
                    if (cClient.username == client.username)
                    {
                        UserManager_Joinings.SendLoginResponse(cClient, CommonEnumerators.LoginResponse.ExtraLogin);
                    }
                }
            }
        }
    }
}
