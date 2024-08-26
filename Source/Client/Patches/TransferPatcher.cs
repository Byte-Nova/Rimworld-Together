using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
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
            if (Network.state == ClientNetworkState.Connected && FactionValues.playerFactions.Contains(TradeSession.trader.Faction)) 
            {
                TransferManagerHelper.AddThingToTransferManifest(___thingsColony[0], ___countToTransfer);                
            }

            return true;
        }
    }
}
