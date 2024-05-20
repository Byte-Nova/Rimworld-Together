using Verse;

namespace GameClient
{
    //Class that contains all the disconnection functions that the mod uses

    public static class DisconnectionManager
    {
        //Useful disconnection variables

        public enum DCReason
        {
            None,
            SaveQuitToMenu,
            SaveQuitToOS,
            QuitToMenu
        }

        public static bool isIntentionalDisconnect;
        public static DCReason intentionalDisconnectReason;

        //Executes different actions depending on the disconnection mode

        public static void HandleDisconnect()
        {
            if (isIntentionalDisconnect)
            {
                string reason = "ERROR";

                switch (intentionalDisconnectReason)
                {
                    case DCReason.None:
                        reason = "No reason given.";
                        DisconnectToMenu();
                        break;

                    case DCReason.QuitToMenu:
                        reason = "Quit to menu.";
                        DialogManager.PushNewDialog(new RT_Dialog_OK("Returning to main menu.", delegate { DisconnectToMenu(); }));
                        break;

                    case DCReason.SaveQuitToMenu:
                        reason = "Save and Quit to Menu.";
                        DialogManager.PushNewDialog(new RT_Dialog_OK("Your progress has been saved!", delegate { DisconnectToMenu(); }));
                        break;

                    case DCReason.SaveQuitToOS:
                        reason = "Save and Quit to OS.";
                        DialogManager.PushNewDialog(new RT_Dialog_OK("Your progress has been saved!", delegate { QuitGame(); }));
                        break;

                    default:
                        reason = $"{intentionalDisconnectReason}";
                        DisconnectToMenu();
                        break;
                }

                Logger.Message($"Disconnected from server: {reason}");
                return;
            }
            
            Logger.Message($"Disconnected from server: Connection Lost");
            if ( Current.ProgramState == ProgramState.Playing )
            {
                DialogManager.PushNewDialog( new RT_Dialog_YesNo( "Your connection to the server has been lost. Would you like to save your game locally before returning to the menu?", 
                  delegate { SaveManager.ForceSave(); DisconnectToMenu(); }, delegate { DisconnectToMenu(); }));
                return;
            }

            /// I don't think we need a dialogue saying you were disconnected from the server if you aren't playing the game, but if you want one it'd go here:
            //DialogManager.PushNewDialog( new RT_Dialog_Error( "Your connection to the server has been lost.", delegate { DisconnectToMenu(); }));
            
            /// Otherwise, all you need is this:
            DisconnectToMenu();
        }

        //Kicks the client into the main menu

        public static void DisconnectToMenu()
        {
            Network.state = NetworkState.Disconnected;
            OnlineChatManager.CleanChat();
            ClientValues.CleanValues();
            ServerValues.CleanValues();

            DialogManager.PopWaitDialog();

            if (Current.ProgramState != ProgramState.Entry)
            {
                LongEventHandler.QueueLongEvent(delegate { }, 
                    "Entry", "", doAsynchronously: false, null);
            }
        }

        //Kicks the client into closing the game

        public static void QuitGame() { Root.Shutdown(); }

        //Kicks the client into restarting the game

        public static void RestartGame() { GenCommandLine.Restart(); }
    }
}
