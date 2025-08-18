using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using static TCPNetwork.Packets.TransferData;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    // Sends the request to the server after the vanilla trading has gone through

    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
    public static class Patch_TradeDeal_TryExecute
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return;
            else
            {
                if (TradeSession.giftMode) SessionValues.OutgoingManifest._transferMode = TransferMode.Gift;
                else SessionValues.OutgoingManifest._transferMode = TransferMode.Trade;

                if (ClientValues.LastTradeStep != ClientValues.TradeMode.Receiving) TransferManager.SendTransferRequestToServer(TransferLocation.Caravan);
                else TransferManager.SendTransferRequestToServer(TransferLocation.Settlement);
            }
        }
    }

    // Forces the trader to want every item the player wants to give

    [HarmonyPatch(typeof(TraderKindDef), nameof(TraderKindDef.WillTrade))]
    public static class Patch_TraderKindDef_WillTrade
    {
        [HarmonyPrefix]
        public static bool DoPre(ref bool __result)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                __result = true;
                return false;
            }
        }
    }

    // Adds all available items to the tradeable list

    [HarmonyPatch(typeof(TradeDeal), "AddAllTradeables")]
    public static class Patch_TradeDeal_AddAllTradeables
    {
        [HarmonyPrefix]
        public static bool DoPre(TradeDeal __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                // This means we are adding items from the CARAVAN

                if (ClientValues.LastTradeStep != ClientValues.TradeMode.Receiving)
                {
                    SessionValues.ChosenCaravan = TradeSession.playerNegotiator.GetCaravan();

                    MethodInfo toInvoke = typeof(TradeDeal).GetMethod("AddToTradeables", BindingFlags.NonPublic | BindingFlags.Instance);

                    //Need to check if they are slaves or prisoners because the game already adds them by default

                    foreach (Pawn pawn in SessionValues.ChosenCaravan.PawnsListForReading)
                    {
                        if (TradeSession.playerNegotiator == pawn) continue;
                        else if (!pawn.IsFreeColonist || !pawn.IsFreeNonSlaveColonist) continue;
                        else toInvoke.Invoke(__instance, new object[] { pawn, Transactor.Colony });
                    }

                    return true;
                }

                // This means we are adding items from the SETTLEMENT

                else
                {
                    MethodInfo toInvoke = typeof(TradeDeal).GetMethod("AddToTradeables", BindingFlags.NonPublic | BindingFlags.Instance);

                    foreach (Pawn pawn in RimworldManager.GetPawnsFromMap(TradeSession.playerNegotiator.Map, Faction.OfPlayer, true))
                    {
                        if (TradeSession.playerNegotiator == pawn) continue;
                        else toInvoke.Invoke(__instance, new object[] { pawn, Transactor.Colony });
                    }

                    foreach (Thing thing in RimworldManager.GetAllThingsInMap(TradeSession.playerNegotiator.Map))
                    {
                        toInvoke.Invoke(__instance, new object[] { thing, Transactor.Colony });
                    }

                    return false;
                }
            }
        }
    }

    // Adds the selected item to the tradeable list while preventing AI faction from adding it

    [HarmonyPatch(typeof(TradeDeal), "AddToTradeables")]
    public static class Patch_TradeDeal_AddToTradeables
    {
        [HarmonyPrefix]
        public static bool DoPre(Transactor trans)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                if (trans == Transactor.Trader) return false;
                else return true;
            }
        }
    }

    // Prevents the warning of trader not having enough silver

    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.DoesTraderHaveEnoughSilver))]
    public static class Patch_TradeDeal_DoesTraderHaveEnoughSilver
    {
        [HarmonyPrefix]
        public static bool DoPre(ref bool __result)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                __result = true;
                return false;
            }
        }
    }

    // Prevents the trade from failing if the AI faction has no silver

    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.CountPostDealFor))]
    public static class Patch_Tradeable_CountPostDealFor
    {
        [HarmonyPrefix]
        public static bool DoPre(ref int __result)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                __result = int.MaxValue;
                return false;
            }
        }
    }

    // Trades every item that has been selected

    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.ResolveTrade))]
    public static class Patch_Tradeable_ResolveTrade
    {
        [HarmonyPrefix]
        public static bool DoPre(List<Thing> ___thingsColony, int ___countToTransfer)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                // We need to set it back to positive because the way RimWorld treats traded items
                int toTransfer = Math.Abs(___countToTransfer);

                if (toTransfer > 0)
                {
                    TransferManagerHelper.AddThingToTransferManifest(___thingsColony[0], toTransfer);
                    Printer.Warning($"Transfered {Math.Abs(___countToTransfer)} of thing {___thingsColony[0]}", LogImportanceMode.Verbose);
                }

                return true;
            }
        }
    }

    // Trades every pawn that has been selected

    [HarmonyPatch(typeof(Tradeable_Pawn), nameof(Tradeable_Pawn.ResolveTrade))]
    public static class Patch_Tradeable_Pawn_ResolveTrade
    {
        [HarmonyPrefix]
        public static bool DoPre(Tradeable_Pawn __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                // We need to set it back to positive because the way RimWorld treats traded items
                int toTransfer = Math.Abs(__instance.CountToTransfer);

                if (toTransfer > 0)
                {
                    TransferManagerHelper.AddThingToTransferManifest(__instance.thingsColony[0], toTransfer);
                    Printer.Warning($"Transfered {Math.Abs(__instance.CountToTransfer)} of thing {__instance.thingsColony[0]}", LogImportanceMode.Verbose);
                }

                return true;
            }
        }
    }

    // Patches settlement trading so items and pawns can be sent over

    [HarmonyPatch(typeof(Settlement_TraderTracker), nameof(Settlement_TraderTracker.GiveSoldThingToTrader))]
    public static class Patch_Settlement_TraderTracker_GiveSoldThingToTrader
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing toGive, int countToGive)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else
            {
                // This means we are calculating from the CARAVAN

                if (ClientValues.LastTradeStep != ClientValues.TradeMode.Receiving) return true;

                // This means we are adding items from the SETTLEMENT

                else
                {
                    Thing thing = toGive.SplitOff(countToGive);
                    if (toGive is Pawn pawn && !pawn.Destroyed) pawn.Destroy();
                    else if (!thing.Destroyed) thing.Destroy();

                    return false;
                }
            }
        }
    }

    // Patches captive UI so it doesn't show on online trades

    [HarmonyPatch(typeof(TransferableUIUtility), nameof(TransferableUIUtility.DrawCaptiveTradeInfo))]
    public static class Patch_TransferableUIUtility_DrawCaptiveTradeInfo
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (ClientValues.LastTradeStep == ClientValues.TradeMode.None) return true;
            else return false;
        }
    }

    // Resets the trade variables to make sure it doesn't conflict with AI trades

    [HarmonyPatch(typeof(Dialog_Trade), nameof(Dialog_Trade.Close))]
    public static class Patch_Dialog_Trade_Close
    {
        [HarmonyPostfix]
        public static void DoPre()
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return;
            else ClientValues.ToggleTradeStep(ClientValues.TradeMode.None);
        }
    }
}
