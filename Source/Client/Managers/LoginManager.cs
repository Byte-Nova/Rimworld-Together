using Shared;
using Verse;

namespace GameClient
{
    //Class that handles loging responses from the server

    public static class LoginManager
    {
        //Parses the received packet into an order

        public static void ReceiveLoginResponse(Packet packet)
        {
            DialogManager.PopWaitDialog();

            JoinDetailsJSON loginDetailsJSON = (JoinDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(loginDetailsJSON.tryResponse))
            {
                case (int)CommonEnumerators.LoginResponse.InvalidLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.LoginInvalid".Translate()));
                    break;

                case (int)CommonEnumerators.LoginResponse.BannedLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.PlayerBaned".Translate()));
                    break;

                case (int)CommonEnumerators.LoginResponse.RegisterSuccess:
                    DialogShortcuts.ShowRegisteredDialog();
                    break;

                case (int)CommonEnumerators.LoginResponse.RegisterInUse:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.UserNameOccupied".Translate()));
                    break;

                case (int)CommonEnumerators.LoginResponse.RegisterError:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.RegError".Translate()));
                    break;

                case (int)CommonEnumerators.LoginResponse.ExtraLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.DifferentPlaceNotice".Translate()));
                    break;

                case (int)CommonEnumerators.LoginResponse.WrongMods:
                    ModManager.GetConflictingMods(packet);
                    break;

                case (int)CommonEnumerators.LoginResponse.ServerFull:
                    DialogManager.PopDialog(DialogManager.dialog2Button);
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.ServerFull".Translate()));
                    break;

                case (int)CommonEnumerators.LoginResponse.Whitelist:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.ServerWhiteList".Translate()));
                    break;

                case (int)CommonEnumerators.LoginResponse.WrongVersion:
                    DialogManager.PushNewDialog(new RT_Dialog_Error($"ModVersionMismatch".Translate(loginDetailsJSON.extraDetails[0])));
                    break;
            }
        }
    }
}
