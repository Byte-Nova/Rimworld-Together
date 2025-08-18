using GameClient.Managers;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(SettleInEmptyTileUtility), nameof(SettleInEmptyTileUtility.Settle))]
    public static class Patch_SettleInEmptyTileUtility_Settle
    {
        [HarmonyPostfix]
        public static void ModifyPost(Caravan caravan)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Connected)
            {
                SettlementManager.SendNewPlayerSettlement(caravan.Tile);

                SaveManager.ForceSave();
            }
        }
    }

    [HarmonyPatch(typeof(SettleInExistingMapUtility), nameof(SettleInExistingMapUtility.Settle))]
    public static class Patch_SettleInExistingMapUtility_Settle
    {
        [HarmonyPostfix]
        public static void ModifyPost(Map map)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Connected)
            {
                SettlementManager.SendNewPlayerSettlement(map.Tile);

                SaveManager.ForceSave();
            }
        }
    }

    [HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
    public static class Patch_SettlementAbandonUtility_Abandon
    {
        [HarmonyPostfix]
        public static void ModifyPost(Settlement settlement)
        {
            if (SessionValues.CurrentNetworkState != ClientNetworkState.Connected) return;
            else SettlementManager.AbandonSettlement(settlement.Tile);
        }
    }

    [HarmonyPatch(typeof(Settlement), nameof(Settlement.PostRemove))]
    public static class Patch_Settlement_PostRemove
    {
        [HarmonyPostfix]
        public static void ModifyPost(Settlement __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Connected)
            {
                if (!ClientValues.IsReadyToPlay) return;
                if (!SessionValues.ActionValues.EnableNPCDestruction) return;

                if (__instance.Faction == Faction.OfPlayer) return;
                else if (NPCManagerH.lastRemovedSettlement != __instance) NPCManager.RequestSettlementRemoval(__instance);
            }
        }
    }
}
