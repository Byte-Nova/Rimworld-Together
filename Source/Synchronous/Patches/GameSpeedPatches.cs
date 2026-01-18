using GameClient.Misc;
using HarmonyLib;
using Synchronous.Managers;
using Synchronous.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Synchronous.Patches
{
    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.CurTimeSpeed), MethodType.Setter)]
    public static class P_TickManager_CurTimeSpeed
    {
        [HarmonyPrefix]
        public static bool CurTimeSpeed(TimeSpeed value)
        {
            if (PatchHandler.BypassFlag) return true;
            else if (!SessionHandler.IsSynchronousHost) return false;
            else
            {
                SGameSpeedManager.Ask(value);
                return false;
            }
        }
    }

    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.TogglePaused))]
    public static class P_TickManager_TogglePaused
    {
        [HarmonyPrefix]
        public static bool TogglePaused()
        {
            if (PatchHandler.BypassFlag) return false;
            else if (!SessionHandler.IsSynchronousHost) return false;
            else
            {
                SGameSpeedManager.Ask(TimeSpeed.Paused);
                return false;
            }
        }
    }

    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.Paused), MethodType.Getter)]
    public static class P_TickManager_Paused
    {
        [HarmonyPrefix]
        public static bool Paused(ref bool __result)
        {
            if (LongEventHandler.ForcePause) __result = true;
            else if (Find.TickManager.CurTimeSpeed != 0) __result = Find.TilePicker.Active;
            else __result = false;

            return false;
        }
    }
}
