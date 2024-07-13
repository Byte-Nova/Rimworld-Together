using HarmonyLib;
using RimWorld.Planet;
using Shared;

namespace GameClient
{
    [HarmonyPatch(typeof(Caravan), nameof(Caravan.PostAdd))]
    public static class PatchAddCaravan
    {
        [HarmonyPostfix]
        public static void DoPost(Caravan __instance)
        {
            if (Network.state == NetworkState.Disconnected) return;

            CaravanManager.RequestCaravanAdd(__instance);
        }
    }

    [HarmonyPatch(typeof(Caravan), nameof(Caravan.PostRemove))]
    public static class PatchRemoveCaravan
    {
        [HarmonyPostfix]
        public static void DoPost(Caravan __instance)
        {
            if (Network.state == NetworkState.Disconnected) return;

            CaravanManager.RequestCaravanRemove(__instance);
        }
    }

    [HarmonyPatch(typeof(Caravan_PathFollower), "TryEnterNextPathTile")]
    public static class PatchMoveCaravan
    {
        [HarmonyPrefix]
        public static bool DoPre(Caravan ___caravan)
        {
            if (Network.state == NetworkState.Disconnected) return true;

            CaravanManager.ModifyDetailsTile(___caravan);
            CaravanManager.RequestCaravanMove(___caravan);
            return true;
        }
    }

    //TODO
    //CHECK HOW TO HOOK INTO TELEPORTER

    [HarmonyPatch(typeof(Caravan_PathFollower), nameof(Caravan_PathFollower.StopDead))]
    public static class PatchTeleportCaravan
    {
        [HarmonyPostfix]
        public static void DoPost(Caravan ___caravan)
        {
            if (Network.state == NetworkState.Disconnected) return;

            CaravanManager.ModifyDetailsTile(___caravan);
            CaravanManager.RequestCaravanMove(___caravan);
            return;
        }
    }
}
