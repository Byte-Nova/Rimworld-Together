using HarmonyLib;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.TickManagerUpdate))]
    public static class PatchGameSpeed
    {
        [HarmonyPrefix]
        public static bool DoPre(TickManager __instance)
        {
            //Check if player is connected
            if (Network.state == ClientNetworkState.Disconnected) return true;

            //Check if feature is disabled
            if (SessionValues.actionValues.EnforcedGameSpeed == 0) return true;

            //Check if in activity
            if (SessionValues.currentRealTimeActivity != OnlineActivityType.None) return true;

            //Check if required speed is valid
            if (SessionValues.actionValues.EnforcedGameSpeed < 0 || SessionValues.actionValues.EnforcedGameSpeed > 4) return true;

            //Check if speed needs to be modified
            if (__instance.CurTimeSpeed != (TimeSpeed)SessionValues.actionValues.EnforcedGameSpeed)
            {
                if (__instance.CurTimeSpeed == TimeSpeed.Paused) return true;
                else __instance.CurTimeSpeed = (TimeSpeed)SessionValues.actionValues.EnforcedGameSpeed;
            }

            return true;
        }
    }
}