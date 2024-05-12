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
            if (Network.state == NetworkState.Connected) ClientValues.ManageDevOptions();
            else return;
        }
    }

    [HarmonyPatch(typeof(Page_SelectStorytellerInGame), "DoWindowContents")]
    public static class PatchCustomDifficulty
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Connected) CustomDifficultyManager.EnforceCustomDifficulty();
            else return;
        }
    }
}
