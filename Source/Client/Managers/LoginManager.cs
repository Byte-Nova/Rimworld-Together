using System;
using System.Linq;
using Shared;
using static Shared.CommonEnumerators;
using Verse;

namespace GameClient
{
    //Class that handles loging responses from the server

    public static class LoginManager
    {
        //Parses the received packet into an order

        public static void ParsePacket(Packet packet)
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
                    ModManagerHelper.GetConflictingMods(packet);
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

        public static void ShowLoginOrRegisterDialogs()
        {
            RT_Dialog_3Input a1 = new RT_Dialog_3Input(
                "New User",
                "Username",
                "Password",
                "Confirm Password",
                delegate { ParseRegisterUser(); },
                delegate { DialogManager.PushNewDialog(DialogManager.dialog2Button); },
                false, true, true);

            RT_Dialog_2Input a2 = new RT_Dialog_2Input(
                "Existing User",
                "Username",
                "Password",
                delegate { ParseLoginUser(); },
                delegate { DialogManager.PushNewDialog(DialogManager.dialog2Button); },
                false, true);

            RT_Dialog_2Button d1 = new RT_Dialog_2Button(
                "Login Select",
                "Choose your login type",
                "New User",
                "Existing User",
                delegate { DialogManager.PushNewDialog(a1); },
                delegate 
                {
                    DialogManager.PushNewDialog(a2);
                    
                    LoginDataFile loginData = PreferenceManager.LoadLoginData();
                    DialogManager.dialog2Input.inputOneResult = loginData.Username;
                    DialogManager.dialog2Input.inputTwoResult = loginData.Password;
                },
                delegate 
                {
                    ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.QuitToMenu);
                    if (Network.listener != null) Network.listener.disconnectFlag = true; 
                });

            DialogManager.PushNewDialog(d1);
        }

        public static void ParseLoginUser()
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultOne)) isInvalid = true;
            if (DialogManager.dialog2ResultOne.Any(Char.IsWhiteSpace)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultTwo)) isInvalid = true;

            if (!isInvalid)
            {
                LoginData loginData = new LoginData();
                loginData._username = DialogManager.dialog2ResultOne;
                loginData._password = Hasher.GetHashFromString(DialogManager.dialog2ResultTwo);
                loginData._version = CommonValues.executableVersion;
                loginData._runningMods = ModManagerHelper.GetRunningModList();
                loginData.joinType = JoinType.Login;

                ClientValues.username = loginData._username;
                PreferenceManager.SaveLoginData(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);

                Packet packet = Packet.CreatePacketFromObject(nameof(LoginManager), loginData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for login response"));
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("Login details are invalid! Please try again!",
                    delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

                DialogManager.PushNewDialog(d1);
            }
        }

        public static void ParseRegisterUser()
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace(DialogManager.dialog3ResultOne)) isInvalid = true;
            if (DialogManager.dialog3ResultOne.Any(Char.IsWhiteSpace)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(DialogManager.dialog3ResultTwo)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(DialogManager.dialog3ResultThree)) isInvalid = true;
            if (DialogManager.dialog3ResultTwo != DialogManager.dialog3ResultThree) isInvalid = true;

            if (!isInvalid)
            {
                LoginData loginData = new LoginData();
                loginData._username = DialogManager.dialog3ResultOne;
                loginData._password = Hasher.GetHashFromString(DialogManager.dialog3ResultTwo);
                loginData._version = CommonValues.executableVersion;
                loginData._runningMods = ModManagerHelper.GetRunningModList();
                loginData.joinType = JoinType.Register;

                ClientValues.username = loginData._username;
                PreferenceManager.SaveLoginData(DialogManager.dialog3ResultOne, DialogManager.dialog3ResultTwo);

                Packet packet = Packet.CreatePacketFromObject(nameof(LoginManager), loginData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for register response"));
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("Register details are invalid! Please try again!",
                    delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

                DialogManager.PushNewDialog(d1);
            }
        }
    }
}
