using HarmonyLib;
using RimWorld.Planet;
using System.Linq;
using Verse;
using Verse.AI;

namespace GameClient
{
    [HarmonyPatch(typeof(SettlementDefeatUtility), "CheckDefeated")]
    public static class PatchSettlementJoin
    {
        [HarmonyPrefix]
        public static bool DoPre(Settlement factionBase)
        {
            if (!Network.isConnectedToServer) return true;

            if (FactionValues.playerFactions.Contains(factionBase.Faction)) return false;

            else return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public static class PatchJobInformation
    {
        [HarmonyPrefix]
        public static bool DoPre(Job newJob, Pawn ___pawn)
        {
            if (Network.isConnectedToServer)
            {
                if (ClientValues.isInVisit)
                {
                    if (OnlineVisitManager.nonFactionPawns.Contains(___pawn))
                    {
                        if (newJob.exitMapOnArrival) return false;
                    }
                }
            }

            return true;
        }
    }

    //TODO
    //Check online sites working without this patch

    //[HarmonyPatch(typeof(SitePartWorker_Outpost), "GetEnemiesCount")]
    //public static class PatchPawnGroupMakerDisplay
    //{
    //    [HarmonyPrefix]
    //    public static bool DoPre(ref int __result)
    //    {
    //        if (Network.isConnectedToServer)
    //        {
    //            __result = 25;

    //            return false;
    //        }
    //        else return true;
    //    }
    //}
}
