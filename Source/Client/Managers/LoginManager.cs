using Shared;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles loging responses from the server

    public static class LoginManager
    {
        //Parses the received packet into an order

        public static void ReceiveLoginResponse(Packet packet)
        {
            LoginData loginData = (LoginData)Serializer.ConvertBytesToObject(packet.contents);

            switch(loginData.tryResponse)
            {
                case LoginResponse.InvalidLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Login details are invalid! Please try again!"));
                    break;

                case LoginResponse.BannedLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You are banned from this server!"));
                    break;

                case LoginResponse.RegisterInUse:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("That username is already in use! Please try again!"));
                    break;

                case LoginResponse.RegisterError:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("There was an error registering! Please try again!"));
                    break;

                case LoginResponse.ExtraLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You connected from another place!"));
                    break;

                case LoginResponse.WrongMods:
                    ModManager.GetConflictingMods(packet);
                    break;

                case LoginResponse.ServerFull:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Server is full!"));
                    break;

                case LoginResponse.Whitelist:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Server is whitelisted!"));
                    break;

                case LoginResponse.WrongVersion:
                    DialogManager.PushNewDialog(new RT_Dialog_Error($"Mod version mismatch! Expected version {loginData.extraDetails[0]}"));
                    break;

                case LoginResponse.NoWorld:
                    DialogManager.PushNewDialog(new RT_Dialog_Error($"Server is currently being set up! Join again later!"));
                    break;
            }
        }
    }
}
