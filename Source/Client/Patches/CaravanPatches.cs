using HarmonyLib;
using RimWorld.Planet;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(Caravan), nameof(Caravan.PostAdd))]
    public static class PatchAddCaravan
    {
        [HarmonyPostfix]
        public static void DoPost(Caravan __instance)
        {
            if (Network.state == ClientNetworkState.Disconnected) return;

            CaravanManager.RequestCaravanAdd(__instance);
        }
    }

    [HarmonyPatch(typeof(Caravan), nameof(Caravan.PostRemove))]
    public static class PatchRemoveCaravan
    {
        [HarmonyPostfix]
        public static void DoPost(Caravan __instance)
        {
            if (Network.state == ClientNetworkState.Disconnected) return;

            CaravanManager.RequestCaravanRemove(__instance);
        }
    }

    [HarmonyPatch(typeof(Caravan_PathFollower), "TryEnterNextPathTile")]
    public static class PatchMoveCaravan
    {
        [HarmonyPrefix]
        public static bool DoPre(Caravan_PathFollower __instance, Caravan ___caravan)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;

            CaravanManager.ModifyDetailsTile(___caravan, __instance.nextTile);
            CaravanManager.RequestCaravanMove(___caravan);
            return true;
        }
    }
}
