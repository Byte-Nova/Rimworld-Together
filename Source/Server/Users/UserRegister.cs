using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class UserRegister
    {
        public static void TryRegisterUser(ServerClient client, Packet packet)
        {
            LoginData loginData = Serializer.ConvertBytesToObject<LoginData>(packet.contents);

            if (!UserManager.CheckIfUserUpdated(client, loginData)) return;

            if (!UserManager.CheckLoginData(client, loginData, LoginMode.Register)) return;

            if (UserManager.CheckIfUserExists(client, loginData, LoginMode.Register)) return;

            try
            {
                client.userFile.SetLoginDetails(loginData);

                client.userFile.SaveUserFile();

                UserLogin.TryLoginUser(client, packet);

                Logger.Message($"[Registered] > {client.userFile.Username}");
            }
            catch { UserManager.SendLoginResponse(client, LoginResponse.RegisterError); }
        }
    }
}
