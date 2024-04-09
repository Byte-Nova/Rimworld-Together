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
