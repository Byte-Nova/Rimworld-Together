using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using UnityEngine.SceneManagement;
using Verse;
using Verse.Profile;

namespace RimworldTogether.GameClient.Patches
{
    public class PersistentPatches
    {
        public static void ForcePermadeath() { Current.Game.Info.permadeathMode = true; }

        public static void ManageGameDifficulty() { CustomDifficultyManager.EnforceCustomDifficulty(); }

        public static void ManageDevOptions()
        {
            if (ServerValues.isAdmin) return;
            else Prefs.DevMode = false;
        }

        public static void DisconnectToMenu()
        {
            //FIXME
            //Check on this function, randomly causes games to go black after disconnecting but is useful if not wanting to wait for GC
            //MemoryUtility.ClearAllMapsAndWorld();

            MemoryUtility.ClearAllMapsAndWorld();
            ClientValues.ToggleDisconnecting(false);
            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ChatManager.ClearChat();

            SceneManager.LoadScene(0, LoadSceneMode.Single);
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

    [HarmonyPatch(typeof(Dialog_Options), "DoWindowContents")]
    public static class PatchDevMode
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.Network.isConnectedToServer) PersistentPatches.ManageDevOptions();
        }
    }

    [HarmonyPatch(typeof(Page_SelectStorytellerInGame), "DoWindowContents")]
    public static class PatchCustomDifficulty
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.Network.isConnectedToServer) PersistentPatches.ManageGameDifficulty();
        }
    }
}
