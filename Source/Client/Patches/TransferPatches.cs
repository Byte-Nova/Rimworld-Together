using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
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
using Shared;
using Shared.Misc;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
    public static class Patch_TradeDeal_TryExecute
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return;
            else
            {
                if (TradeSession.giftMode) SessionHandler.OutgoingManifest._transferMode = TransferMode.Gift;
                else SessionHandler.OutgoingManifest._transferMode = TransferMode.Trade;

                if (SessionHandler.LastTradeStep != CommonEnumerators.TradeMode.Receiving) TransferManager.SendTransferRequestToServer(TransferLocation.Caravan);
                else TransferManager.SendTransferRequestToServer(TransferLocation.Settlement);
            }
        }
    }

    [HarmonyPatch(typeof(TraderKindDef), nameof(TraderKindDef.WillTrade))]
    public static class Patch_TraderKindDef_WillTrade
    {
        [HarmonyPrefix]
        public static bool DoPre(ref bool __result, TraderKindDef __instance)
        {
            if (SessionHandler.PlayerFactionDefs.Contains(__instance.faction)) return true;
            else
            {
                __result = true;
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(TradeDeal), "AddAllTradeables")]
    public static class Patch_TradeDeal_AddAllTradeables
    {
        [HarmonyPrefix]
        public static bool DoPre(TradeDeal __instance)
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
            else
            {
                if (SessionHandler.LastTradeStep != CommonEnumerators.TradeMode.Receiving)
                {
                    if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(SessionHandler.ChosenCaravan, 1))
                    {
                        Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                        RimworldManager.PlaceThingIntoCaravan(silver, SessionHandler.ChosenCaravan);
                    }
                }

                else
                {
                    if (!RimworldManager.CheckIfHasEnoughSilverInMap(Find.AnyPlayerHomeMap, 1))
                    {
                        Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                        RimworldManager.PlaceThingIntoMap(silver, Find.AnyPlayerHomeMap);
                    }
                }

                MethodInfo toInvoke = typeof(TradeDeal).GetMethod("AddToTradeables", BindingFlags.NonPublic | BindingFlags.Instance);

                // This means we are adding items from the CARAVAN

                if (SessionHandler.LastTradeStep != CommonEnumerators.TradeMode.Receiving)
                {
                    foreach (Thing thing in SessionHandler.ChosenCaravan.AllThings)
                    {
                        if (TradeSession.playerNegotiator == thing) continue;
                        else toInvoke.Invoke(__instance, new object[] { thing, Transactor.Colony });
                    }

                    return false;
                }

                // This means we are adding items from the SETTLEMENT

                else
                {
                    foreach (Thing thing in Finder.GetAllThingsInMap(TradeSession.playerNegotiator.Map))
                    {
                        if (thing is Pawn && thing.Faction == Faction.OfPlayer && TradeSession.playerNegotiator != thing) toInvoke.Invoke(__instance, new object[] { thing, Transactor.Colony });
                        else if (thing is not Pawn && thing.def.alwaysHaulable) toInvoke.Invoke(__instance, new object[] { thing, Transactor.Colony });
                    }

                    return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TradeDeal), "AddToTradeables")]
    public static class Patch_TradeDeal_AddToTradeables
    {
        [HarmonyPrefix]
        public static bool DoPre(Transactor trans)
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
            else
            {
                if (trans == Transactor.Trader) return false;
                else return true;
            }
        }
    }

    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.DoesTraderHaveEnoughSilver))]
    public static class Patch_TradeDeal_DoesTraderHaveEnoughSilver
    {
        [HarmonyPrefix]
        public static bool DoPre(ref bool __result)
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
            else
            {
                __result = true;
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.CountPostDealFor))]
    public static class Patch_Tradeable_CountPostDealFor
    {
        [HarmonyPrefix]
        public static bool DoPre(ref int __result)
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
            else
            {
                __result = int.MaxValue;
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Tradeable), nameof(Tradeable.ResolveTrade))]
    public static class Patch_Tradeable_ResolveTrade
    {
        [HarmonyPrefix]
        public static bool DoPre(List<Thing> ___thingsColony, int ___countToTransfer)
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
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

    [HarmonyPatch(typeof(Tradeable_Pawn), nameof(Tradeable_Pawn.ResolveTrade))]
    public static class Patch_Tradeable_Pawn_ResolveTrade
    {
        [HarmonyPrefix]
        public static bool DoPre(Tradeable_Pawn __instance)
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
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
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
            else
            {
                // This means we are calculating from the CARAVAN

                if (SessionHandler.LastTradeStep != CommonEnumerators.TradeMode.Receiving) return true;

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
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionHandler.LastTradeStep == CommonEnumerators.TradeMode.None) return true;
            else return false;
        }
    }

    [HarmonyPatch(typeof(Dialog_Trade), nameof(Dialog_Trade.Close))]
    public static class Patch_Dialog_Trade_Close
    {
        [HarmonyPostfix]
        public static void DoPre()
        {
            if (SessionHandler.CurrentNetworkState == ClientNetworkState.Disconnected) return;
            else SessionHandler.LastTradeStep = CommonEnumerators.TradeMode.None;
        }
    }
}
