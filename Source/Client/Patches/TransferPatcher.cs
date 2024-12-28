using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(TradeDeal), "AddAllTradeables")]
    public static class AddTradeablePatch
    {
        [HarmonyPrefix]
        public static bool DoPre(ref List<Tradeable> ___tradeables)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            if (!FactionValues.playerFactions.Contains(TradeSession.trader.Faction)) return true;
            
            ___tradeables = new List<Tradeable>();
            ___tradeables.AddRange(SessionValues.listToShowInTradesMenu);
            return false;
        }
    }

    [HarmonyPatch(typeof(Tradeable), "ResolveTrade")]
    public static class GetTradeablePatch
    {
        [HarmonyPrefix]
        public static bool DoPre(List<Thing> ___thingsColony, int ___countToTransfer)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            if (!FactionValues.playerFactions.Contains(TradeSession.trader.Faction)) return true;
            else TransferManagerHelper.AddThingToTransferManifest(___thingsColony[0], ___countToTransfer);

            return true;
        }
    }

    [HarmonyPatch(typeof(Settlement_TraderTracker), nameof(Settlement_TraderTracker.GiveSoldThingToTrader))]
    public static class IgnoreSoldThingCheckPatch
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing toGive, int countToGive)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else if (!FactionValues.playerFactions.Contains(TradeSession.trader.Faction)) return true;
            else
            {
                Thing thing = toGive.SplitOff(countToGive);
                if (toGive is Pawn pawn && !pawn.Destroyed) pawn.Destroy();
                else if (!thing.Destroyed) thing.Destroy();
            }

            return false;
        }
    }
}
