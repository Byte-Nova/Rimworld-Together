using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Verse.Profile;
using Verse;

namespace RimworldTogether.GameClient.Managers
{
    public static class DisconnectionManager
    {
        public static void DisconnectToMenu()
        {
            ChatManager.ClearChat();
            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ClientValues.ToggleDisconnecting(false);

            SceneManager.LoadScene(0, LoadSceneMode.Single);
            Current.ProgramState = ProgramState.Entry;
        }

        public static void QuitGame()
        {
            ClientValues.ToggleQuiting(false);
            Root.Shutdown();
        }

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
