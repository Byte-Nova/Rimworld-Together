using System;
using System.Linq;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Verse;

namespace RimworldTogether.GameClient.Misc
{
    public static class Parser
    {
        public static void ParseConnectionDetails(bool throughBrowser)
        {
            bool isInvalid = false;

            string[] answerSplit = null;
            if (throughBrowser)
            {
                answerSplit = ClientValues.serverBrowserContainer
                    [DialogManager.dialogListingWithButtonResult].Split('|');

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
                    Network.Network.ip = answerSplit[0];
                    Network.Network.port = answerSplit[1];
                    Saver.SaveConnectionDetails(answerSplit[0], answerSplit[1]);
                }

                else
                {
                    Network.Network.ip = DialogManager.dialog2ResultOne;
                    Network.Network.port = DialogManager.dialog2ResultTwo;
                    Saver.SaveConnectionDetails(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);
                }

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                Threader.GenerateThread(Threader.Mode.Start);
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
                LoginDetailsJSON loginDetails = new LoginDetailsJSON();
                loginDetails.username = DialogManager.dialog2ResultOne;
                loginDetails.password = Hasher.GetHash(DialogManager.dialog2ResultTwo);
                loginDetails.clientVersion = ClientValues.versionCode;
                loginDetails.runningMods = ModManager.GetRunningModList().ToList();

                ChatManager.username = loginDetails.username;
                Saver.SaveLoginDetails(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);

                Packet packet = Packet.CreatePacketFromJSON("LoginClientPacket", loginDetails);
                Network.Network.serverListener.SendData(packet);

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
                LoginDetailsJSON registerDetails = new LoginDetailsJSON();
                registerDetails.username = DialogManager.dialog3ResultOne;
                registerDetails.password = Hasher.GetHash(DialogManager.dialog3ResultTwo);
                registerDetails.clientVersion = ClientValues.versionCode;
                registerDetails.runningMods = ModManager.GetRunningModList().ToList();

                Packet packet = Packet.CreatePacketFromJSON("RegisterClientPacket", registerDetails);
                Network.Network.serverListener.SendData(packet);

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