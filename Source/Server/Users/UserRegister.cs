using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RimworldTogether;
using Shared.JSON;
using Shared.Misc;

namespace GameServer
{
    public static class UserRegister
    {
        public static void TryRegisterUser(Client client, Packet packet)
        {
            LoginDetailsJSON registerDetails = Serializer.SerializeFromString<LoginDetailsJSON>(packet.contents[0]);
            client.username = registerDetails.username;
            client.password = registerDetails.password;

            if (!UserManager_Joinings.CheckLoginDetails(client, UserManager_Joinings.CheckMode.Register)) return;

            if (TryFetchAlreadyRegistered(client)) return;
            else
            {
                UserFile userFile = new UserFile();
                userFile.uid = GetNewUIDForUser(client);
                userFile.username = client.username;
                userFile.password = client.password;

                try
                {
                    UserManager.SaveUserFile(client, userFile);

                    UserManager_Joinings.SendLoginResponse(client, UserManager_Joinings.LoginResponse.RegisterSuccess);

                    Logger.WriteToConsole($"[Registered] > {client.username}");
                }

                catch 
                {
                    UserManager_Joinings.SendLoginResponse(client, UserManager_Joinings.LoginResponse.RegisterError);

                    return;
                }
            }
        }

        private static bool TryFetchAlreadyRegistered(Client client)
        {
            string[] existingUsers = Directory.GetFiles(Program.usersPath);

            foreach (string user in existingUsers)
            {
                UserFile existingUser = Serializer.SerializeFromFile<UserFile>(user);
                if (existingUser.username.ToLower() != client.username.ToLower()) continue;
                else
                {
                    UserManager_Joinings.SendLoginResponse(client, UserManager_Joinings.LoginResponse.RegisterInUse);

                    return true;
                }
            }

            return false;
        }

        private static string GetNewUIDForUser(Client client)
        {
            return Hasher.GetHash(client.username);
        }
    }
}
