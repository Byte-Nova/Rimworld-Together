using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Patches;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameClient.Dialogs
{
    public static class DialogShortcuts
    {
        public static void ShowRegisteredDialog()
        {
            DialogManager.PopWaitDialog();

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "You have been successfully registered!",
                "You are now able to login using your new account"});

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowLoginOrRegisterDialogs()
        {
            RT_Dialog_3Input a1 = new RT_Dialog_3Input(
                "New User",
                "Username",
                "Password",
                "Confirm Password",
                delegate { Parser.ParseRegisterUser(); },
                delegate { DialogManager.PushNewDialog(DialogManager.dialog2Button); },
                false, true, true);

            RT_Dialog_2Input a2 = new RT_Dialog_2Input(
                "Existing User",
                "Username",
                "Password",
                delegate { Parser.ParseLoginUser(); },
                delegate { DialogManager.PushNewDialog(DialogManager.dialog2Button); },
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
                delegate { Network.Network.DisconnectFromServer(); });

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowWorldGenerationDialogs()
        {
            RT_Dialog_OK d3 = new RT_Dialog_OK("This feature is not implemented yet!",
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Button d2 = new RT_Dialog_2Button("Game Mode", "Choose the way you want to play",
                "Separate colony", "Together with other players (TBA)", null, delegate { DialogManager.PushNewDialog(d3); },
                delegate { PersistentPatches.RestartGame(true); });

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "Welcome to the world view!",
                        "Please choose the way you would like to play", "This mode can't be changed upon choosing!" },
                delegate { DialogManager.PushNewDialog(d2); });

            DialogManager.PushNewDialog(d1);
        }

        public static void ShowConnectDialogs()
        {
            RT_Dialog_ListingWithButton a1 = new RT_Dialog_ListingWithButton("Server Browser", "List of reachable servers",
                ClientValues.serverBrowserContainer,
                delegate { Parser.ParseConnectionDetails(true); },
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Input a2 = new RT_Dialog_2Input(
                "Connection Details",
                "IP",
                "Port",
                delegate { Parser.ParseConnectionDetails(false); },
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });
            RT_Dialog_2Button newDialog = new RT_Dialog_2Button(
                "Play Online",
                "Choose the connection type",
                "Server Browser",
                "Direct Connect",
                delegate { DialogManager.PushNewDialog(a1); },
                delegate {
                    DialogManager.PushNewDialog(a2);
                    PreferenceManager.FetchConnectionDetails();
                },
                null);

            DialogManager.PushNewDialog(newDialog);
        }
    }
}
