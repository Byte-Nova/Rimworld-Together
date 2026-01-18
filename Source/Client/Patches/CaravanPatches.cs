using GameClient.Managers;
using GameClient.Misc;
using HarmonyLib;
using RimWorld.Planet;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(Caravan), nameof(Caravan.PostAdd))]
    public static class PatchAddCaravan
    {
        [HarmonyPostfix]
        public static void DoPost(Caravan __instance)
        {
            CaravanManager.RequestCaravanAdd(__instance);
        }
    }

    [HarmonyPatch(typeof(Caravan), nameof(Caravan.PostRemove))]
    public static class PatchRemoveCaravan
    {
        [HarmonyPostfix]
        public static void DoPost(Caravan __instance)
        {
            CaravanManager.RequestCaravanRemove(__instance);
        }
    }

    [HarmonyPatch(typeof(Caravan_PathFollower), "TryEnterNextPathTile")]
    public static class PatchMoveCaravan
    {
        [HarmonyPrefix]
        public static bool DoPre(Caravan ___caravan)
        {
            CaravanManager.RequestCaravanUpdate(___caravan);
            return true;
        }
    }
}
