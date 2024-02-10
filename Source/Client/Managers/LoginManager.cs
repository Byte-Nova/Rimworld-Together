using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameClient.Managers
{
    public static class LoginManager
    {
        public static void ReceiveLoginResponse(Packet packet)
        {
            DialogManager.PopWaitDialog();

            JoinDetailsJSON loginDetailsJSON = (JoinDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch(int.Parse(loginDetailsJSON.tryResponse))
            {
                case (int)CommonEnumerators.LoginResponse.InvalidLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Login details are invalid! Please try again!"));
                    break;

                case (int)CommonEnumerators.LoginResponse.BannedLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You are banned from this server!"));
                    break;

                case (int)CommonEnumerators.LoginResponse.RegisterSuccess:
                    DialogShortcuts.ShowRegisteredDialog();
                    break;

                case (int)CommonEnumerators.LoginResponse.RegisterInUse:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("That username is already in use! Please try again!"));
                    break;

                case (int)CommonEnumerators.LoginResponse.RegisterError:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("There was an error registering! Please try again!"));
                    break;

                case (int)CommonEnumerators.LoginResponse.ExtraLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You connected from another place!"));
                    break;

                case (int)CommonEnumerators.LoginResponse.WrongMods:
                    ModManager.GetConflictingMods(packet);
                    break;

                case (int)CommonEnumerators.LoginResponse.ServerFull:
                    DialogManager.PopDialog(DialogManager.dialog2Button);
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Server is full!"));
                    break;

                case (int)CommonEnumerators.LoginResponse.Whitelist:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Server is whitelisted!"));
                    break;
            }
        }
    }
}
