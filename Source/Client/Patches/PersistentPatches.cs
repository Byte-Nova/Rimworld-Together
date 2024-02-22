using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Values;

namespace RimworldTogether.GameClient.Patches
{
    [HarmonyPatch(typeof(Dialog_Options), "DoWindowContents")]
    public static class PatchDevMode
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.Network.isConnectedToServer) ClientValues.ManageDevOptions();
        }
    }

    [HarmonyPatch(typeof(Page_SelectStorytellerInGame), "DoWindowContents")]
    public static class PatchCustomDifficulty
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.Network.isConnectedToServer) CustomDifficultyManager.EnforceCustomDifficulty();
        }
    }
}
