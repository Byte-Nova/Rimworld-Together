using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.Network;
using System.Linq;
using System;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using Verse;
using UnityEngine;
using System.Data;

namespace RimworldTogether.GameClient.Dialogs
{
    public static class DialogShortcuts
    {
        public static void ShowRegisteredDialog()
        {
            DialogManager.PopDialog();

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "You have been successfully registered!",
                "You are now able to login using your new account"});

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowLoginOrRegisterDialogs()
        {
            Log.Message("In showLoginOrRegisterDialog");
            Log.Message("ShowLoginORRegisterDialogs");
            RT_Dialog_3Input a1 = new RT_Dialog_3Input(
                "New User",
                "Username",
                "Password",
                "Confirm Password",
                delegate { ParseRegisterUser(); },
                DialogManager.PopDialog,
                false, true, true);

            RT_Dialog_2Input a2 = new RT_Dialog_2Input(
                "Existing User",
                "Username",
                "Password",
                delegate { ParseLoginUser(); },
                DialogManager.PopDialog,
                false, true);

            RT_Dialog_2Button d1 = new RT_Dialog_2Button(
                "Login Select",
                "Choose your login type",
                "New User",
                "Existing User",
                delegate { DialogManager.PushNewDialog(a1); },
                delegate {
                    DialogManager.PushNewDialog(a2);
                    PreferenceManager.FetchLoginDetails();
                },
                delegate { DialogManager.PopDialog(); Network.Network.serverListener.disconnectFlag = true; });

            DialogManager.PushNewDialog(d1);
            
        }

        public static void ShowWorldGenerationDialogs()
        {
            RT_Dialog_OK d3 = new RT_Dialog_OK("This feature is not implemented yet!",
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Button d2 = new RT_Dialog_2Button("Game Mode", "Choose the way you want to play",
                "Separate colony", "Together with other players (TBA)", null, delegate { DialogManager.PushNewDialog(d3); },
                delegate { DialogManager.PopDialog(); DisconnectionManager.RestartGame(true); });

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "Welcome to the world view!",
                        "Please choose the way you would like to play", "This mode can't be changed upon choosing!" },
                delegate { DialogManager.PushNewDialog(d2); });

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowConnectDialogs()
        {
            //Log.Message($"[Top window is: {Find.WindowStack[0].ToString()}]");
            RT_Dialog_ListingWithButton a1 = new RT_Dialog_ListingWithButton("Server Browser", "List of reachable servers",
                ClientValues.serverBrowserContainer,
                delegate { ParseConnectionDetails(true); },
                DialogManager.PopDialog);

            RT_Dialog_2Button newDialog = new RT_Dialog_2Button(
                "Play Online",
                "Choose the connection type",
                "Server Browser",
                "Direct Connect",
                delegate { DialogManager.PushNewDialog(a1); },
                delegate {
                    PreferenceManager.FetchConnectionDetails();
                },
                DialogManager.PopDialog);


            Log.Message($"Pushing connection type dialog {Find.WindowStack.Count}");
            DialogManager.PushNewDialog(newDialog);
        }

        public static void ParseConnectionDetails(bool throughBrowser)
        {
            Log.Message($"[Rimworld Together] > Parsing connection details.  throughBrowser : {throughBrowser}");
            bool isValid = true;

            string[] answerSplit = null;
            if (throughBrowser)
            {
                answerSplit = ClientValues.serverBrowserContainer
                        [(int)DialogManager.inputCache[0]].Split('|');
                Log.Message($"Using ip: {answerSplit[0]} and port: {answerSplit[1]}");
                if (string.IsNullOrWhiteSpace(answerSplit[0])) isValid = false;
                if (string.IsNullOrWhiteSpace(answerSplit[1])) isValid = false;
                if (answerSplit[1].Count() > 5) isValid = false;
                if (!answerSplit[1].All(Char.IsDigit)) isValid = false;
            }

            else
            {
                if (string.IsNullOrWhiteSpace((string)DialogManager.inputCache[0])) isValid = false;
                if (string.IsNullOrWhiteSpace((string)DialogManager.inputCache[1])) isValid = false;
                if (((string)DialogManager.inputCache[1]).Count() > 5) isValid = false;
                if (!((string)DialogManager.inputCache[1]).All(Char.IsDigit)) isValid = false;
            }

            if (isValid)
            {
                if (throughBrowser)
                {
                    Network.Network.ip = answerSplit[0];
                    Network.Network.port = answerSplit[1];
                    PreferenceManager.SaveConnectionDetails(answerSplit[0], answerSplit[1]);
                }

                else
                {
                    Network.Network.ip = (string)DialogManager.inputCache[0];
                    Network.Network.port = (string)DialogManager.inputCache[1];
                    PreferenceManager.SaveConnectionDetails((string)DialogManager.inputCache[0], (string)DialogManager.inputCache[1]);
                }

                Log.Message($"Trying to connect to server");
                DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                Network.Network.StartConnection();
            }

            else
            {
                Log.Message($"Invalid connection details");
                RT_Dialog_Error d1 = new RT_Dialog_Error("Server details are invalid! Please try again!", DialogManager.PopDialog);
                DialogManager.PushNewDialog(d1);
            }
        }

        public static void ParseLoginUser()
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace((string)DialogManager.inputCache[0])) isInvalid = true;
            if (((string)DialogManager.inputCache[0]).Any(Char.IsWhiteSpace)) isInvalid = true;
            if (string.IsNullOrWhiteSpace((string)DialogManager.inputCache[1])) isInvalid = true;

            if (!isInvalid)
            {
                JoinDetailsJSON loginDetails = new JoinDetailsJSON();
                loginDetails.username = (string)DialogManager.inputCache[0];
                loginDetails.password = Hasher.GetHash((string)DialogManager.inputCache[1]);
                loginDetails.clientVersion = ClientValues.versionCode;
                loginDetails.runningMods = ModManager.GetRunningModList().ToList();

                ChatManager.username = loginDetails.username;
                PreferenceManager.SaveLoginDetails((string)DialogManager.inputCache[0], (string)DialogManager.inputCache[1]);

                Packet packet = Packet.CreatePacketFromJSON("LoginClientPacket", loginDetails);
                Network.Network.serverListener.SendData(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for login response"));
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("Login details are invalid! Please try again!", DialogManager.PopDialog);

                DialogManager.PushNewDialog(d1);
            }
        }

        public static void ParseRegisterUser()
        {
            bool isInvalid = false;
            if (string.IsNullOrWhiteSpace((string)DialogManager.inputCache[0])) isInvalid = true;
            if (((string)DialogManager.inputCache[0]).Any(Char.IsWhiteSpace)) isInvalid = true;
            if (string.IsNullOrWhiteSpace((string)DialogManager.inputCache[1])) isInvalid = true;
            if (string.IsNullOrWhiteSpace((string)DialogManager.inputCache[2])) isInvalid = true;
            if ((string)DialogManager.inputCache[1] != (string)DialogManager.inputCache[2]) isInvalid = true;

            if (!isInvalid)
            {
                JoinDetailsJSON registerDetails = new JoinDetailsJSON();
                registerDetails.username = (string)DialogManager.inputCache[0];
                registerDetails.password = Hasher.GetHash((string)DialogManager.inputCache[1]);
                registerDetails.clientVersion = ClientValues.versionCode;
                registerDetails.runningMods = ModManager.GetRunningModList().ToList();

                Packet packet = Packet.CreatePacketFromJSON("RegisterClientPacket", registerDetails);
                Network.Network.serverListener.SendData(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for register response"));
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("Register details are invalid! Please try again!", DialogManager.PopDialog);

                DialogManager.PushNewDialog(d1);
            }
        }
    }
}
