using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameClient.Managers
{
    public static class LoginManager
    {
        public enum LoginResponse 
        { 
            InvalidLogin, 
            BannedLogin, 
            RegisterSuccess, 
            RegisterInUse, 
            RegisterError, 
            ExtraLogin, 
            WrongMods, 
            ServerFull,
            Whitelist
        }

        public static void ReceiveLoginResponse(Packet packet)
        {
            DialogManager.PopWaitDialog();

            LoginDetailsJSON loginDetailsJSON = (LoginDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch(int.Parse(loginDetailsJSON.tryResponse))
            {
                case (int)LoginResponse.InvalidLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Login details are invalid! Please try again!"));
                    break;

                case (int)LoginResponse.BannedLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You are banned from this server!"));
                    break;

                case (int)LoginResponse.RegisterSuccess:
                    DialogShortcuts.ShowRegisteredDialog();
                    break;

                case (int)LoginResponse.RegisterInUse:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("That username is already in use! Please try again!"));
                    break;

                case (int)LoginResponse.RegisterError:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("There was an error registering! Please try again!"));
                    break;

                case (int)LoginResponse.ExtraLogin:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You connected from another place!"));
                    break;

                case (int)LoginResponse.WrongMods:
                    ModManager.GetConflictingMods(packet);
                    break;

                case (int)LoginResponse.ServerFull:
                    DialogManager.PopDialog(DialogManager.dialog2Button);
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Server is full!"));
                    break;

                case (int)LoginResponse.Whitelist:
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Server is whitelisted!"));
                    break;
            }
        }
    }
}
