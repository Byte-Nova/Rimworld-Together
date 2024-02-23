using UnityEngine.SceneManagement;
using Verse;

namespace GameClient
{
    //Class that contains all the disconnection functions that the mod uses

    public static class DisconnectionManager
    {
        //Kicks the client into the main menu

        public static void DisconnectToMenu()
        {
            ChatManager.ClearChat();
            Network.ClearAllValues();
            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ClientValues.ToggleDisconnecting(false);

            SceneManager.LoadScene(0, LoadSceneMode.Single);
            Current.ProgramState = ProgramState.Entry;
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
