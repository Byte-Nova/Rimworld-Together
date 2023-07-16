using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimworldTogether
{
    [HarmonyPatch(typeof(SettlementDefeatUtility), "CheckDefeated")]
    public static class PatchSettlementJoin
    {
        [HarmonyPrefix]
        public static bool DoPre(Settlement factionBase)
        {
            if (!Network.isConnectedToServer) return true;

            if (PlanetFactions.playerFactions.Contains(factionBase.Faction)) return false;

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
                    if (VisitManager.otherPlayerPawns.Contains(___pawn))
                    {
                        if (newJob.exitMapOnArrival) return false;
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SitePartWorker_Outpost), "GetEnemiesCount")]
    public static class PatchPawnGroupMakerDisplay
    {
        [HarmonyPrefix]
        public static bool DoPre(ref int __result)
        {
            if (Network.isConnectedToServer)
            {
                __result = 25;

                return false;
            }
            else return true;
        }
    }
}
