using HarmonyLib;
using RimWorld;

namespace GameClient
{
    [HarmonyPatch(typeof(Dialog_Options), "DoWindowContents")]
    public static class PatchDevMode
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.isConnectedToServer) ClientValues.ManageDevOptions();
        }
    }

    [HarmonyPatch(typeof(Page_SelectStorytellerInGame), "DoWindowContents")]
    public static class PatchCustomDifficulty
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.isConnectedToServer) CustomDifficultyManager.EnforceCustomDifficulty();
        }
    }
}
