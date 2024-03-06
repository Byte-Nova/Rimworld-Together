using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient
{
    [HarmonyPatch(typeof(CaravanEnterMapUtility), "Enter", new[] { typeof(Caravan), typeof(Map), typeof(Func<Pawn, IntVec3>), typeof(CaravanDropInventoryMode), typeof(bool) })]
    public static class PatchCaravanEnterMapUtility1
    {
        [HarmonyPostfix]
        public static void DoPost(Map map)
        {
            if (FactionValues.playerFactions.Contains(map.Parent.Faction))
            {
                FloodFillerFog.DebugRefogMap(map);
            }
        }
    }

    [HarmonyPatch(typeof(CaravanEnterMapUtility), "Enter", new[] { typeof(Caravan), typeof(Map), typeof(CaravanEnterMode), typeof(CaravanDropInventoryMode), typeof(bool), typeof(Predicate<IntVec3>) })]
    public static class PatchCaravanEnterMapUtility2
    {
        [HarmonyPostfix]
        public static void DoPost(Map map)
        {
            if (FactionValues.playerFactions.Contains(map.Parent.Faction))
            {
                FloodFillerFog.DebugRefogMap(map);
            }
        }
    }
}
