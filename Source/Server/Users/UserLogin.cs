using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class UserLogin
    {
        public static void TryLoginUser(Client client, Packet packet)
        {
            LoginDetailsJSON loginDetails = Serializer.SerializeFromString<LoginDetailsJSON>(packet.contents[0]);
            client.username = loginDetails.username;
            client.password = loginDetails.password;

            if (!UserManager_Joinings.CheckWhitelist(client)) return;

            if (!UserManager_Joinings.CheckLoginDetails(client, UserManager_Joinings.CheckMode.Login)) return;

            if (!UserManager.CheckIfUserExists(client)) return;

            UserManager.LoadDataFromFile(client);

            if (ModManager.CheckIfModConflict(client, loginDetails)) return;

            if (UserManager.CheckIfUserBanned(client)) return;

            RemoveOldClientIfAny(client);

            PostLogin(client);
        }

        private static void PostLogin(Client client)
        {
            UserManager.SaveUserIP(client);

            UserManager.SendPlayerRecount();

            ServerOverallManager.SendServerOveralls(client);

            ChatManager.SendMessagesToClient(client, ChatManager.defaultJoinMessages);

            if (WorldManager.CheckIfWorldExists())
            {
                if (SaveManager.CheckIfUserHasSave(client)) SaveManager.LoadUserGame(client);
                else WorldManager.SendWorldFile(client);
            }
            else WorldManager.RequireWorldFile(client);
        }

        private static void RemoveOldClientIfAny(Client client)
        {
            foreach (Client cClient in Network.connectedClients.ToArray())
            {
                if (cClient == client) continue;
                else
                {
                    if (cClient.username == client.username)
                    {
                        UserManager_Joinings.SendLoginResponse(cClient, UserManager_Joinings.LoginResponse.ExtraLogin);
                    }
                }
            }
        }
    }
}
