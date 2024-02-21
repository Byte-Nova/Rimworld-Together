using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using GameClient;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace GameClient
{
    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan", new[] { typeof(IEnumerable < Pawn >), typeof(Faction) , typeof(int), typeof(int), typeof(int), typeof(bool) } )]
    internal class ExitMapAndCreateCaravan
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            //for every human pawn in the player's faction contained in the map
            List<Pawn> playersPawns = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

            Logs.Message($"There are {playersPawns.Count} colonists still visiting");
            //if pawns still exist on the map, then don't stop the visit.
            if (playersPawns.Count > 0 ) { return; }

            Current.Game.DeinitAndRemoveMap(OnlineVisitManager.visitMap);
            OnlineVisitManager.StopVisit();
        }
    }
}
