using GameClient.Managers;
using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.AbandonMap))]
    public static class Patch_GravshipUtility_AbandonMap
    {
        [HarmonyPostfix]
        public static void DoPost(Map map)
        {
            SettlementManager.AbandonSettlement(map.Tile);
        }
    }

    [HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.ArriveNewMap))]
    public static class Patch_GravshipUtility_ArriveNewMap
    {
        [HarmonyPostfix]
        public static void DoPost(Gravship gravship)
        {
            Map map = Find.WorldObjects.MapParentAt(gravship.destinationTile)?.Map;
            SettlementManager.SendNewPlayerSettlement(map.Tile);
            SaveManager.ForceSave();
        }
    }

    [HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.ArriveExistingMap))]
    public static class Patch_GravshipUtility_ArriveExistingMap
    {
        [HarmonyPostfix]
        public static void DoPost(Gravship gravship)
        {
            Map map = Find.WorldObjects.MapParentAt(gravship.destinationTile)?.Map;
            SettlementManager.SendNewPlayerSettlement(map.Tile);
            SaveManager.ForceSave();
        }
    }
}
