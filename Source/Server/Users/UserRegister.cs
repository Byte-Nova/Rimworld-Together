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

            client.uid = GetNewUIDForUser(client);
            client.username = loginData.username;
            client.password = loginData.password;

            try
            {
                client.SaveToUserFile();

                UserLogin.TryLoginUser(client, packet);

                Logger.Message($"[Registered] > {client.username}");
            }
            catch { UserManager.SendLoginResponse(client, LoginResponse.RegisterError); }
        }

        private static string GetNewUIDForUser(ServerClient client)
        {
            return Hasher.GetHashFromString(client.username);
        }
    }
}
