using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class UserRegister
    {
        public static void TryRegisterUser(ServerClient client, Packet packet)
        {
            LoginData loginData = (LoginData)Serializer.ConvertBytesToObject(packet.contents);

            if (!UserManager.CheckIfUserUpdated(client, loginData)) return;

            if (!UserManager.CheckLoginData(client, loginData, LoginMode.Register)) return;

            if (UserManager.CheckIfUserExists(client, loginData, LoginMode.Register)) return;

            client.username = loginData.username;
            client.password = loginData.password;

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
            catch { UserManager.SendLoginResponse(client, LoginResponse.RegisterError); }
        }

        private static string GetNewUIDForUser(ServerClient client)
        {
            return Hasher.GetHashFromString(client.username);
        }
    }
}
