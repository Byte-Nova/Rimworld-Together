using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using Verse;
using Verse.AI;

namespace RimworldTogether.GameClient.Patches
{
    [HarmonyPatch(typeof(SettlementDefeatUtility), "CheckDefeated")]
    public static class PatchSettlementJoin
    {
        [HarmonyPrefix]
        public static bool DoPre(Settlement factionBase)
        {
            if (!Network.Network.isConnectedToServer) return true;

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
            if (Network.Network.isConnectedToServer)
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

    //TODO
    //Check online sites working without this patch

    //[HarmonyPatch(typeof(SitePartWorker_Outpost), "GetEnemiesCount")]
    //public static class PatchPawnGroupMakerDisplay
    //{
    //    [HarmonyPrefix]
    //    public static bool DoPre(ref int __result)
    //    {
    //        if (Network.Network.isConnectedToServer)
    //        {
    //            __result = 25;

    //            return false;
    //        }
    //        else return true;
    //    }
    //}
}
