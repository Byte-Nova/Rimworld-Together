using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Profile;

namespace RimworldTogether
{
    public class PersistentPatches
    {
        public static void ForcePermadeath() { Current.Game.Info.permadeathMode = true; }

        public static void ManageGameDifficulty() { DifficultyValues.ForceCustomDifficulty(); }

        public static void ManageDevOptions()
        {
            if (ServerValues.isAdmin) return;
            else Prefs.DevMode = false;
        }

        public static void DisconnectToMenu()
        {
            LongEventHandler.QueueLongEvent(delegate { MemoryUtility.ClearAllMapsAndWorld(); },
            "Entry", "SavingLongEvent", doAsynchronously: false, null, showExtraUIInfo: false);

            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ChatManager.ClearChat();

            ClientValues.ToggleDisconnecting(false);
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
            if (Network.isConnectedToServer) PersistentPatches.ManageDevOptions();
        }
    }

    [HarmonyPatch(typeof(Page_SelectStorytellerInGame), "DoWindowContents")]
    public static class PatchCustomDifficulty
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.isConnectedToServer) PersistentPatches.ManageGameDifficulty();
        }
    }
}
