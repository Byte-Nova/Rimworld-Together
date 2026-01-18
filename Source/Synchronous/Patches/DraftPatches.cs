using HarmonyLib;
using RimWorld;
using Synchronous.Core;
using Synchronous.Managers;
using Synchronous.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronous.Patches
{
    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
    public static class P_Pawn_DraftController_Drafted
    {
        [HarmonyPrefix]
        public static bool Drafted(Pawn_DraftController __instance, bool value)
        {
            if (PatchHandler.BypassFlag) return true;
            else if (!Main_.CheckIfShouldPatch(__instance.pawn.MapHeld)) return true;
            else
            {
                SDraftManager.Ask(__instance.pawn, value);
                return false;
            }
        }
    }
}
