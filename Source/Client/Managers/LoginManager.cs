using Shared;
using static Shared.CommonEnumerators;
using Verse;

namespace GameClient
{
    //Class that handles loging responses from the server

    public static class LoginManager
    {
        //Parses the received packet into an order

        public static void ReceiveLoginResponse(Packet packet)
        {
            LoginData loginData = Serializer.ConvertBytesToObject<LoginData>(packet.contents);

            switch(loginData._tryResponse)
            {
                case LoginResponse.InvalidLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTLoginInvalid".Translate()));
                    break;

                case LoginResponse.BannedLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogBanned".Translate()));
                    break;

                case LoginResponse.RegisterInUse:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogUsernameAlreadyUsed".Translate()));
                    break;

                case LoginResponse.RegisterError:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTRegisterError".Translate()));
                    break;

                case LoginResponse.ExtraLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogAlreadyConnected".Translate()));
                    break;

                case LoginResponse.WrongMods:
                    ModManager.GetConflictingMods(packet);
                    break;

                case LoginResponse.ServerFull:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogServerFull".Translate()));
                    break;

                case LoginResponse.Whitelist:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogWhitelisted".Translate()));
                    break;

                case LoginResponse.WrongVersion:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogVersionMismatch".Translate(loginData._extraDetails[0])));
                    break;

                case LoginResponse.NoWorld:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogServerBeingSetup".Translate()));
                    break;
            }
        }
    }
}
