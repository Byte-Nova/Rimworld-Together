using Verse;

namespace GameClient
{
    //Class that contains all the disconnection functions that the mod uses

    public static class DisconnectionManager
    {
        //Useful disconnection variables

        public enum DCReason { None, SaveQuitToMenu, SaveQuitToOS, QuitToMenu, ConnectionLost }

        public static DCReason intentionalDisconnectReason;

        public static bool isIntentionalDisconnect;

        //Executes different actions depending on the disconnection mode

        public static void HandleDisconnect()
        {
            if (isIntentionalDisconnect)
            {
                string reason = "ERROR";

                switch (intentionalDisconnectReason)
                {
                    case DCReason.None:
                        reason = "No reason given";
                        DisconnectToMenu();
                        break;

                    case DCReason.QuitToMenu:
                        reason = "Quit to menu";
                        DisconnectToMenu();
                        break;

                    case DCReason.SaveQuitToMenu:
                        reason = "Save and Quit to Menu";
                        DialogManager.PushNewDialog(new RT_Dialog_OK("RTDisconnectProgressSaved".Translate(), delegate { DisconnectToMenu(); }));
                        break;

                    case DCReason.SaveQuitToOS:
                        reason = "Save and Quit to OS";
                        DialogManager.PushNewDialog(new RT_Dialog_OK("RTDisconnectProgressSaved".Translate(), delegate { QuitGame(); }));
                        break;

                    case DCReason.ConnectionLost:
                        reason = "Connection to server lost";
                        DisconnectToMenu();
                        break;

                    default:
                        reason = $"{intentionalDisconnectReason}";
                        DisconnectToMenu();
                        break;
                }

                Logger.Message($"Disconnected from server: {reason}");
            }

            else
            {
                Logger.Message($"Disconnected from server: Connection Lost");

                if (Current.ProgramState != ProgramState.Entry)
                {
                    DialogManager.PushNewDialog(new RT_Dialog_YesNo("RTDisconnectSave".Translate(),
                        delegate { SaveManager.ForceSave(); DisconnectToMenu(); }, delegate { DisconnectToMenu(); }));
                }
                else DisconnectToMenu();
            }
        }

        //Kicks the client into the main menu

        public static void DisconnectToMenu()
        {
            ClientValues.CleanValues();
            ServerValues.CleanValues();
            SessionValues.CleanValues();
            ChatManager.CleanChat();

            DialogManager.PopWaitDialog();

            if (Current.ProgramState != ProgramState.Entry)
            {
                LongEventHandler.QueueLongEvent(delegate { }, "Entry", "", doAsynchronously: false, null);
            }
        }

        //Kicks the client into closing the game

        public static void QuitGame() { Root.Shutdown(); }

        //Kicks the client into restarting the game

        public static void RestartGame() { GenCommandLine.Restart(); }
    }
}
