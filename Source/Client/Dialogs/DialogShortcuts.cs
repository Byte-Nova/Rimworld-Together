using System.Linq;
using System;
using Shared;
using Verse;
using UnityEngine.SceneManagement;

namespace GameClient
{
    public static class DialogShortcuts
    {
        public static void ShowRegisteredDialog()
        {
            DialogManager.PopWaitDialog();

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "RimworldTogether.RegistrationSuccess".Translate(),
                "RimworldTogether.AbleLogin".Translate()});

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowLoginOrRegisterDialogs()
        {
            RT_Dialog_3Input a1 = new RT_Dialog_3Input(
                "RimworldTogether.NewUser".Translate(),
                "RimworldTogether.Username".Translate(),
                "RimworldTogether.Password".Translate(),
                "RimworldTogether.ConfirmPassword".Translate(),
                delegate { ParseRegisterUser(); },
                delegate { DialogManager.PushNewDialog(DialogManager.dialog2Button); },
                false, true, true);

            RT_Dialog_2Input a2 = new RT_Dialog_2Input(
                "RimworldTogether.ExistingUser".Translate(),
                "RimworldTogether.Username".Translate(),
                "RimworldTogether.Password".Translate(),
                delegate { ParseLoginUser(); },
                delegate { DialogManager.PushNewDialog(DialogManager.dialog2Button); },
                false, true);

            RT_Dialog_2Button d1 = new RT_Dialog_2Button(
                "RimworldTogether.LoginSelect".Translate(),
                "RimworldTogether.LoginType".Translate(),
                "RimworldTogether.NewUser".Translate(),
                "RimworldTogether.ExistingUser".Translate(),
                delegate { DialogManager.PushNewDialog(a1); },
                delegate {
                    DialogManager.PushNewDialog(a2);
                    PreferenceManager.LoadLoginDetails();
                },
                delegate { Network.listener.disconnectFlag = true; });

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowWorldGenerationDialogs()
        {
            RT_Dialog_OK d3 = new RT_Dialog_OK("RimworldTogether.FeatureUnavailable".Translate(),
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Button d2 = new RT_Dialog_2Button("RimworldTogether.GameMode".Translate(), "RimworldTogether.ChooseWay".Translate(),
                "RimworldTogether.SeparateColony".Translate(), "RimworldTogether.Cooperative".Translate(), null, delegate { DialogManager.PushNewDialog(d3); },
                delegate
                {
                    SceneManager.LoadScene(0);
                    Network.listener.disconnectFlag = true;
                });

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "RimworldTogether.WorldView".Translate(),
                        "RimworldTogether.GameType".Translate(), "RimworldTogether.GameModeChange".Translate() },
                delegate { DialogManager.PushNewDialog(d2); });

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowConnectDialogs()
        {
            RT_Dialog_ListingWithButton a1 = new RT_Dialog_ListingWithButton("RimworldTogether.ServerBrowser".Translate(), "RimworldTogether.ServersList".Translate(),
                ClientValues.serverBrowserContainer,
                delegate { ParseConnectionDetails(true); },
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Input a2 = new RT_Dialog_2Input(
                "RimworldTogether.ConnectionDetails".Translate(),
                "IP",
                "Port",
                delegate { ParseConnectionDetails(false); },
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Button newDialog = new RT_Dialog_2Button(
                "RimworldTogether.PlayOnline".Translate(),
                "RimworldTogether.ConnectionType".Translate(),
                "RimworldTogether.ServerBrowser".Translate(),
                "RimworldTogether.DirectConnect".Translate(),
                delegate { DialogManager.PushNewDialog(a1); },
                delegate {
                    DialogManager.PushNewDialog(a2);
                    PreferenceManager.LoadConnectionDetails();
                }, null);

            DialogManager.PushNewDialog(newDialog);
        }

        public static void ParseConnectionDetails(bool throughBrowser)
        {
            bool isInvalid = false;

            string[] answerSplit = null;
            if (throughBrowser)
            {
                answerSplit = ClientValues.serverBrowserContainer[DialogManager.dialogListingWithButtonResult].Split('|');

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
                    PreferenceManager.SaveConnectionDetails(answerSplit[0], answerSplit[1]);
                }

                else
                {
                    Network.ip = DialogManager.dialog2ResultOne;
                    Network.port = DialogManager.dialog2ResultTwo;
                    PreferenceManager.SaveConnectionDetails(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);
                }

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.ServerConnect".Translate()));
                Network.StartConnection();
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("RimworldTogether.ServerInvalid".Translate());
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
                JoinDetailsJSON loginDetails = new JoinDetailsJSON();
                loginDetails.username = DialogManager.dialog2ResultOne;
                loginDetails.password = Hasher.GetHashFromString(DialogManager.dialog2ResultTwo);
                loginDetails.clientVersion = CommonValues.executableVersion;
                loginDetails.runningMods = ModManager.GetRunningModList().ToList();

                ChatManager.username = loginDetails.username;
                PreferenceManager.SaveLoginDetails(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.LoginClientPacket), loginDetails);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.LoginResponse".Translate()));
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("RimworldTogether.LoginInvalid".Translate(),
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
                JoinDetailsJSON registerDetails = new JoinDetailsJSON();
                registerDetails.username = DialogManager.dialog3ResultOne;
                registerDetails.password = Hasher.GetHashFromString(DialogManager.dialog3ResultTwo);
                registerDetails.clientVersion = CommonValues.executableVersion;
                registerDetails.runningMods = ModManager.GetRunningModList().ToList();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RegisterClientPacket), registerDetails);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.RegisterResponse".Translate()));
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("RimworldTogether.RegisterInvalid".Translate(),
                    delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

                DialogManager.PushNewDialog(d1);
            }
        }
    }
}
