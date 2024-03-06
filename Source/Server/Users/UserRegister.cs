using Shared;

namespace GameServer
{
    public static class UserRegister
    {
        public static void TryRegisterUser(ServerClient client, Packet packet)
        {
            JoinDetailsJSON registerDetails = (JoinDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);
            client.username = registerDetails.username;
            client.password = registerDetails.password;

            if (!UserManager_Joinings.CheckLoginDetails(client, CommonEnumerators.LoginMode.Register)) return;

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

                    UserManager_Joinings.SendLoginResponse(client, CommonEnumerators.LoginResponse.RegisterSuccess);

                    Logger.WriteToConsole($"[Registered] > {client.username}");
                }

                catch 
                {
                    UserManager_Joinings.SendLoginResponse(client, CommonEnumerators.LoginResponse.RegisterError);

                    return;
                }
            }
        }

        private static bool TryFetchAlreadyRegistered(ServerClient client)
        {
            string[] existingUsers = Directory.GetFiles(Master.usersPath);

            foreach (string user in existingUsers)
            {
                UserFile existingUser = Serializer.SerializeFromFile<UserFile>(user);
                if (existingUser.username.ToLower() != client.username.ToLower()) continue;
                else
                {
                    UserManager_Joinings.SendLoginResponse(client, CommonEnumerators.LoginResponse.RegisterInUse);

                    return true;
                }
            }

            return false;
        }

        private static string GetNewUIDForUser(ServerClient client)
        {
            return Hasher.GetHashFromString(client.username);
        }
    }
}
