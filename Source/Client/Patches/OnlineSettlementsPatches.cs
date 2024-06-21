using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(SettlementDefeatUtility), "CheckDefeated")]
    public static class PatchSettlementJoin
    {
        [HarmonyPrefix]
        public static bool DoPre(Settlement factionBase)
        {
            if (Network.state == NetworkState.Disconnected) return true;

            if (FactionValues.playerFactions.Contains(factionBase.Faction)) return false;
            else return true;
        }
    }

    [HarmonyPatch(typeof(CaravanEnterMapUtility), "Enter", new[] { typeof(Caravan), typeof(Map), typeof(Func<Pawn, IntVec3>), typeof(CaravanDropInventoryMode), typeof(bool) })]
    public static class PatchCaravanEnterMapUtility1
    {
        [HarmonyPostfix]
        public static void DoPost(Map map)
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (!FactionValues.playerFactions.Contains(map.Parent.Faction)) return;

            FloodFillerFog.DebugRefogMap(map);
        }
    }

    [HarmonyPatch(typeof(CaravanEnterMapUtility), "Enter", new[] { typeof(Caravan), typeof(Map), typeof(CaravanEnterMode), typeof(CaravanDropInventoryMode), typeof(bool), typeof(Predicate<IntVec3>) })]
    public static class PatchCaravanEnterMapUtility2
    {
        [HarmonyPostfix]
        public static void DoPost(Map map)
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (!FactionValues.playerFactions.Contains(map.Parent.Faction)) return;

            FloodFillerFog.DebugRefogMap(map);
        }
    }
}
