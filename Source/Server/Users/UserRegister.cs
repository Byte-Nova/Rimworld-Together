﻿using Shared;

namespace GameServer
{
    public static class UserRegister
    {
        public static void TryRegisterUser(ServerClient client, Packet packet)
        {
            JoinDetailsJSON registerDetails = (JoinDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            if (!UserManager.CheckIfUserUpdated(client, registerDetails)) return;

            if (!UserManager.CheckLoginDetails(client, registerDetails, CommonEnumerators.LoginMode.Register)) return;

            if (UserManager.CheckIfUserExists(client, registerDetails, CommonEnumerators.LoginMode.Register)) return;

            client.username = registerDetails.username;
            client.password = registerDetails.password;

            try
            {
                UserFile userFile = new UserFile();
                userFile.uid = GetNewUIDForUser(client);
                userFile.username = client.username;
                userFile.password = client.password;

                UserManager.SaveUserFile(client, userFile);

                UserLogin.TryLoginUser(client, packet);

                Logger.WriteToConsole($"[Registered] > {client.username}");
            }
            catch { UserManager.SendLoginResponse(client, CommonEnumerators.LoginResponse.RegisterError); }
        }

        private static string GetNewUIDForUser(ServerClient client)
        {
            return Hasher.GetHashFromString(client.username);
        }
    }
}
