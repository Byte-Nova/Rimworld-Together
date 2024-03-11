using Verse;

namespace GameClient
{
    //Class that contains all the disconnection functions that the mod uses

    public static class DisconnectionManager
    {
        //Kicks the client into the main menu

        public static void DisconnectToMenu()
        {
            Network.CleanValues();
            ChatManager.CleanChat();
            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ClientValues.ToggleDisconnecting(false);

            if (Current.ProgramState != ProgramState.Entry)
            {
                LongEventHandler.QueueLongEvent(delegate { }, 
                    "Entry", "", doAsynchronously: false, null);
            }
        }

        //Kicks the client into closing the game

        public static void QuitGame()
        {
            ClientValues.ToggleQuiting(false);
            Root.Shutdown();
        }

        //Kicks the client into restarting the game

        public static void RestartGame(bool desync)
        {
            if (desync)
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("The game will restart to prevent save desyncs",
                    delegate { GenCommandLine.Restart(); }));
            }
            else GenCommandLine.Restart();
        }
    }
}
