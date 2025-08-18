using GameClient.Values;
using HarmonyLib;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.TickManagerUpdate))]
    public static class PatchGameSpeed
    {
        [HarmonyPrefix]
        public static bool DoPre(TickManager __instance)
        {
            //Check if player is connected
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;

            //Check if feature is disabled
            if (SessionValues.ActionValues.EnforcedGameSpeed == 0) return true;

            //Check if required speed is valid
            if (SessionValues.ActionValues.EnforcedGameSpeed < 0 || SessionValues.ActionValues.EnforcedGameSpeed > 4) return true;

            //Check if speed needs to be modified
            if (__instance.CurTimeSpeed != (TimeSpeed)SessionValues.ActionValues.EnforcedGameSpeed)
            {
                if (__instance.CurTimeSpeed == TimeSpeed.Paused) return true;
                else __instance.CurTimeSpeed = (TimeSpeed)SessionValues.ActionValues.EnforcedGameSpeed;
            }

            return true;
        }
    }
}