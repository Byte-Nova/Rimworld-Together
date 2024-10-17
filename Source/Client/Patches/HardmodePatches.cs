using HarmonyLib;
using Shared;
using Verse;

namespace GameClient
{
    public class HardmodePatches
    {
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static class Pawn_Kill_Patches
        {
            [HarmonyPostfix]
            public static void DoPost(Pawn __instance)
            {
                if (!SessionValues.actionValues.HardcoreMode) return;
                if (__instance.Faction != null && __instance.Faction.IsPlayer)
                    SaveManager.ForceSave();
            }
        }
    }
}