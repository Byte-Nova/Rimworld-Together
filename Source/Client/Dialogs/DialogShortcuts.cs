﻿using System.Linq;
using System;
using Shared;
using Verse;

namespace GameClient
{
    public static class DialogShortcuts
    {
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
                    DialogManager.dialog2Input.inputOneResult = loginData.username;
                    DialogManager.dialog2Input.inputTwoResult = loginData.password;
                },
                delegate 
                {
                    ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.QuitToMenu);
                    if (Network.listener != null) Network.listener.disconnectFlag = true; 
                });

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowConnectDialogs()
        {
            RT_Dialog_2Input dialog = new RT_Dialog_2Input(
            "Connection Details", "IP", "Port",
            delegate { ParseConnectionDetails(false); },
            null);

            ConnectionDataFile connectionData = PreferenceManager.LoadConnectionData();
            DialogManager.dialog2Input.inputOneResult = connectionData.ip;
            DialogManager.dialog2Input.inputTwoResult = connectionData.port;

            DialogManager.PushNewDialog(dialog);
        }

        public static void ParseConnectionDetails(bool throughBrowser)
        {
            bool isInvalid = false;

            string[] answerSplit = null;
            if (throughBrowser)
            {
                answerSplit = ClientValues.serverBrowserContainer[DialogManager.dialogButtonListingResultInt].Split('|');

                if (string.IsNullOrWhiteSpace(answerSplit[0])) isInvalid = true;
                if (string.IsNullOrWhiteSpace(answerSplit[1])) isInvalid = true;
                if (answerSplit[1].Count() > 5) isInvalid = true;
                if (!answerSplit[1].All(Char.IsDigit)) isInvalid = true;
            }

            else
            {
                if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultOne)) isInvalid = true;
                if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultTwo)) isInvalid = true;
                if (DialogManager.dialog2ResultTwo.Count() > 5) isInvalid = true;
                if (!DialogManager.dialog2ResultTwo.All(Char.IsDigit)) isInvalid = true;
            }

            if (!isInvalid)
            {
                if (throughBrowser)
                {
                    Network.ip = answerSplit[0];
                    Network.port = answerSplit[1];
                    PreferenceManager.SaveConnectionData(answerSplit[0], answerSplit[1]);
                }

                else
                {
                    Network.ip = DialogManager.dialog2ResultOne;
                    Network.port = DialogManager.dialog2ResultTwo;
                    PreferenceManager.SaveConnectionData(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);
                }

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                Network.StartConnection();
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("Server details are invalid! Please try again!");
                DialogManager.PushNewDialog(d1);
            }
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
                loginData._runningMods = ModManager.GetRunningModList();

                ClientValues.username = loginData._username;
                PreferenceManager.SaveLoginData(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.LoginClientPacket), loginData);
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
                loginData._runningMods = ModManager.GetRunningModList();

                ClientValues.username = loginData._username;
                PreferenceManager.SaveLoginData(DialogManager.dialog3ResultOne, DialogManager.dialog3ResultTwo);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.RegisterClientPacket), loginData);
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
