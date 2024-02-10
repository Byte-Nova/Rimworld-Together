using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using Verse;

namespace RimworldTogether.GameClient.Patches
{
    [HarmonyPatch(typeof(TradeDeal), "AddAllTradeables")]
    public static class AddTradeablePatch
    {
        [HarmonyPrefix]
        public static bool DoPre(ref List<Tradeable> ___tradeables)
        {
            if (!Network.Network.isConnectedToServer) return true;
            else if (!FactionValues.playerFactions.Contains(TradeSession.trader.Faction)) return true;
            else
            {
                ___tradeables = new List<Tradeable>();
                ___tradeables.AddRange(ClientValues.listToShowInTradesMenu);
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Tradeable), "ResolveTrade")]
    public static class GetTradeablePatch
    {
        [HarmonyPrefix]
        public static bool DoPre(List<Thing> ___thingsColony, int ___countToTransfer)
        {
            if (Network.Network.isConnectedToServer)
            {
                if (FactionValues.playerFactions.Contains(TradeSession.trader.Faction))
                {
                    TransferManagerHelper.AddThingToTransferManifest(___thingsColony[0], ___countToTransfer);
                }
            }

            return true;
        }
    }
}
