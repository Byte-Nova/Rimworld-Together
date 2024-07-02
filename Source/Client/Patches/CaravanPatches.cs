using HarmonyLib;
using RimWorld.Planet;

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
}
