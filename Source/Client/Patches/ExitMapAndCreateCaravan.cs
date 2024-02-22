using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using Verse;


namespace RimworldTogether.GameClient.Patches
{
    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan", new[] { typeof(IEnumerable < Pawn >), typeof(Faction) , typeof(int), typeof(int), typeof(int), typeof(bool) } )]
    internal class ExitMapAndCreateCaravan
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            //for every human pawn in the player's faction contained in the map
            List<Pawn> playersPawns = VisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

            Logs.Message($"There are {playersPawns.Count} colonists still visiting");
            //if pawns still exist on the map, then don't stop the visit.
            if (playersPawns.Count > 0 ) { return; }

            Current.Game.DeinitAndRemoveMap(VisitManager.visitMap);
            VisitManager.StopVisit();
        }
    }
}
