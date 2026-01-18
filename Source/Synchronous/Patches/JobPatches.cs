using GameClient.Misc;
using HarmonyLib;
using Synchronous.Core;
using Synchronous.Managers;
using Synchronous.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Synchronous.Patches
{
    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class P_Pawn_JobTracker_StartJob
    {
        [HarmonyPrefix]
        public static bool StartJob(Pawn ___pawn, Job newJob)
        {
            if (PatchHandler.BypassFlag) return true;
            else if (!Main_.CheckIfShouldPatch(___pawn.MapHeld)) return true;
            else if (___pawn.Faction == SessionHandler.NeutralFaction) return false;
            else
            {
                SJobManager.Ask(newJob, ___pawn);
                return false;
            }
        }
    }
}
