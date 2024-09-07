using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class PatchStartNewJob
    {
        [HarmonyPrefix]
        public static bool DoPre(Job newJob, Pawn ___pawn)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            //Don't execute patch if map doesn't contain the pawn
            bool shouldPatch = false;
            if (OnlineActivityManagerHelper.factionPawns.Contains(___pawn)) shouldPatch = true;
            else if (OnlineActivityManagerHelper.nonFactionPawns.Contains(___pawn)) shouldPatch = true;

            if (!shouldPatch) return true;
            else
            {
                if (OnlineActivityManagerHelper.factionPawns.Contains(___pawn))
                {
                    //This is our pawn and we prepare the packet for the other player

                    return true;
                }

                else
                {
                    //This is not our pawn and we shouldn't handle him from here

                    return false;
                }
            }
        }
    }
}
