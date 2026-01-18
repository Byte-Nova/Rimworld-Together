using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(CaravanEnterMapUtility), nameof(CaravanEnterMapUtility.Enter), new[] { typeof(Caravan), typeof(Map), typeof(Func<Pawn, IntVec3>), typeof(CaravanDropInventoryMode), typeof(bool) })]
    public static class PatchCaravanEnterMapUtility1
    {
        [HarmonyPostfix]
        public static void DoPost(Map map)
        {
            if (!SessionHandler.PlayerFactions.Contains(map.Parent.Faction)) return;

            FloodFillerFog.DebugRefogMap(map);
        }
    }

    [HarmonyPatch(typeof(CaravanEnterMapUtility), nameof(CaravanEnterMapUtility.Enter), new[] { typeof(Caravan), typeof(Map), typeof(CaravanEnterMode), typeof(CaravanDropInventoryMode), typeof(bool), typeof(Predicate<IntVec3>) })]
    public static class PatchCaravanEnterMapUtility2
    {
        [HarmonyPostfix]
        public static void DoPost(Map map)
        {
            if (!SessionHandler.PlayerFactions.Contains(map.Parent.Faction)) return;

            FloodFillerFog.DebugRefogMap(map);
        }
    }
}
