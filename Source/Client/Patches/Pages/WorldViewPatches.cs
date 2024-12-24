using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(WorldInspectPane), "SetInitialSizeAndPosition")]
    public static class AddSideTabs
    {
        [HarmonyPrefix]
        public static bool DoPre(ref WITab[] ___TileTabs)
        {
            if (___TileTabs.Count() != 5 && Network.state == ClientNetworkState.Connected)
            {
                ___TileTabs = new WITab[5]
                {
                    new PlayersUI(),
                    new BasesUI(),
                    new SitesUI(),
                    new WITab_Terrain(),
                    new WITab_Planet()
                };
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SettlementProximityGoodwillUtility), "AppendProximityGoodwillOffsets")]
    public static class PrevenGoodwillChangePatch
    {
        [HarmonyPrefix]
        public static bool DoPre(ref int tile, ref List<Pair<Settlement, int>> outOffsets)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;

            int maxDist = SettlementProximityGoodwillUtility.MaxDist;
            List<Settlement> settlements = Find.WorldObjects.Settlements;
            for (int i = 0; i < settlements.Count; i++)
            {
                Settlement settlement = settlements[i];

                if (FactionValues.playerFactions.Contains(settlement.Faction) || settlement.Faction == Faction.OfPlayer) continue;
                else
                {
                    int num = Find.WorldGrid.TraversalDistanceBetween(tile, settlement.Tile, passImpassable: false, maxDist);
                    if (num != int.MaxValue)
                    {
                        int num2 = Mathf.RoundToInt(DiplomacyTuning.Goodwill_PerQuadrumFromSettlementProximity.Evaluate(num));
                        if (num2 != 0) outOffsets.Add(new Pair<Settlement, int>(settlement, num2));
                    }
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Settlement), "GetGizmos")]
    public static class SettlementGizmoPatch
    {
        [HarmonyPostfix]
        public static void DoPost(ref IEnumerable<Gizmo> __result, Settlement __instance)
        {
            if (Network.state == ClientNetworkState.Disconnected) return;

            if (FactionValues.playerFactions.Contains(__instance.Faction))
            {
                var gizmoList = __result.ToList();
                gizmoList.Clear();

                Command_Action command_Goodwill = new Command_Action
                {
                    defaultLabel = "Change Goodwill",
                    defaultDesc = "Change the goodwill of this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Goodwill"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;

                        Action r1 = delegate { GoodwillManager.TryRequestGoodwill(Goodwill.Enemy,
                            GoodwillTarget.Settlement); };

                        Action r2 = delegate { GoodwillManager.TryRequestGoodwill(Goodwill.Neutral,
                            GoodwillTarget.Settlement); };

                        Action r3 = delegate { GoodwillManager.TryRequestGoodwill(Goodwill.Ally,
                            GoodwillTarget.Settlement); };

                        RT_Dialog_3Button d1 = new RT_Dialog_3Button("Change Goodwill", "Set settlement's goodwill to",
                            "Enemy", "Neutral", "Ally", r1, r2, r3, null);

                        DialogManager.PushNewDialog(d1);
                    }
                };

                Command_Action command_FactionMenu = new Command_Action
                {
                    defaultLabel = "Faction Menu",
                    defaultDesc = "Access your faction menu",
                    icon = ContentFinder<Texture2D>.Get("Commands/FactionMenu"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;

                        if (SessionValues.actionValues.EnableFactions)
                        {
                            if (SessionValues.chosenSettlement.Faction == FactionValues.yourOnlineFaction) FactionManager.OnFactionOpenOnMember();
                            else FactionManager.OnFactionOpenOnNonMember();
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                    }
                };

                Command_Action command_Caravan = new Command_Action
                {
                    defaultLabel = "Form Caravan",
                    defaultDesc = "Form a new caravan",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;

                        Dialog_FormCaravan d1 = new Dialog_FormCaravan(__instance.Map, mapAboutToBeRemoved:true);
                        DialogManager.PushNewDialog(d1);
                    }
                };

                Command_Action command_Aid = new Command_Action
                {
                    defaultLabel = "Aid",
                    defaultDesc = "Send aid to this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Aid"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;

                        if (SessionValues.actionValues.EnableAids)
                        {
                            List<string> pawnNames = new List<string>();
                            foreach (Pawn pawn in RimworldManager.GetAllSettlementsPawns(Faction.OfPlayer, false)) pawnNames.Add(pawn.LabelCapNoCount);
                            DialogManager.PushNewDialog(new RT_Dialog_ListingWithButton("Aid menu", "Select the pawn you want to send for aid", 
                                pawnNames.ToArray(), AidManager.SendAidRequest));
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                    }
                };

                Command_Action command_Event = new Command_Action
                {
                    defaultLabel = "Send Event",
                    defaultDesc = "Send an event to this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Event"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;

                        if (SessionValues.actionValues.EnableEvents) EventManager.ShowEventMenu();
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                    }
                };

                if (__instance.Map == null && __instance.Faction != FactionValues.yourOnlineFaction) gizmoList.Add(command_Goodwill);
                if (ServerValues.hasFaction) gizmoList.Add(command_FactionMenu);
                if (__instance.Map != null) gizmoList.Add(command_Caravan);
                gizmoList.Add(command_Event);
                gizmoList.Add(command_Aid);
                __result = gizmoList;
            }

            else if (__instance.Faction == Find.FactionManager.OfPlayer)
            {
                var gizmoList = __result.ToList();

                Command_Action command_FactionMenu = new Command_Action
                {
                    defaultLabel = "Faction Menu",
                    defaultDesc = "Access your faction menu",
                    icon = ContentFinder<Texture2D>.Get("Commands/FactionMenu"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;

                        if (SessionValues.actionValues.EnableFactions)
                        {
                            if (ServerValues.hasFaction) FactionManager.OnFactionOpen();
                            else FactionManager.OnNoFactionOpen();
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                    }
                };

                gizmoList.Add(command_FactionMenu);
                __result = gizmoList;
            }
        }
    }

    [HarmonyPatch(typeof(Settlement), "GetCaravanGizmos")]
    public static class CaravanSettlementGizmoPatch
    {
        [HarmonyPostfix]
        public static void DoPost(ref IEnumerable<Gizmo> __result, Settlement __instance, Caravan caravan)
        {
            if (Network.state == ClientNetworkState.Disconnected) return;

            if (FactionValues.playerFactions.Contains(__instance.Faction))
            {
                var gizmoList = __result.ToList();

                List<Gizmo> removeList = new List<Gizmo>();
                foreach (Command_Action action in gizmoList.ToList())
                {
                    if (action.defaultLabel == "CommandAttackSettlement".Translate()) removeList.Add(action);
                    else if (action.defaultLabel == "CommandOfferGifts".Translate()) removeList.Add(action);
                    else if (action.defaultLabel == "CommandTrade".Translate()) removeList.Add(action);
                }
                foreach (Gizmo g in removeList) gizmoList.Remove(g);

                Command_Action command_Spy = new Command_Action
                {
                    defaultLabel = "Spy Settlement",
                    defaultDesc = "Spy this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Spy"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;
                        SessionValues.chosenCaravan = caravan;

                        OfflineActivityManager.RequestOfflineActivity(OfflineActivityType.Spy);
                    }
                };

                Command_Action command_Raid = new Command_Action
                {
                    defaultLabel = "Raid Settlement",
                    defaultDesc = "Raid this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Raid"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;
                        SessionValues.chosenCaravan = caravan;

                        RT_Dialog_2Button d1 = new RT_Dialog_2Button("Raid Mode", "Please choose your raid mode",
                            "[BETA] Online", "Offline",
                            delegate { OnlineActivityManager.RequestOnlineActivity(OnlineActivityType.Raid); },
                            delegate { OfflineActivityManager.RequestOfflineActivity(OfflineActivityType.Raid); },
                            null);

                        DialogManager.PushNewDialog(d1);
                    }
                };

                Command_Action command_Visit = new Command_Action
                {
                    defaultLabel = "Visit Settlement",
                    defaultDesc = "Visit this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Visit"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;
                        SessionValues.chosenCaravan = caravan;

                        RT_Dialog_2Button d1 = new RT_Dialog_2Button("Visit Mode", "Please choose your visit mode",
                            "[BETA] Online", "Offline",
                            delegate { OnlineActivityManager.RequestOnlineActivity(OnlineActivityType.Visit); },
                            delegate { OfflineActivityManager.RequestOfflineActivity(OfflineActivityType.Visit); },
                            null);

                        DialogManager.PushNewDialog(d1);
                    }
                };

                Command_Action command_Transfer = new Command_Action
                {
                    defaultLabel = "Transfer Items",
                    defaultDesc = "Transfer items between settlements",
                    icon = ContentFinder<Texture2D>.Get("Commands/Transfer"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = __instance;
                        SessionValues.chosenCaravan = caravan;

                        if (!SessionValues.actionValues.EnableTrading)
                        {
                            DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                            return;
                        }

                        else
                        {
                            if (RimworldManager.CheckIfSocialPawnInCaravan(SessionValues.chosenCaravan))
                            {
                                DialogManager.PushNewDialog(new RT_Dialog_TransferMenu(TransferLocation.Caravan, true, true, true));
                            }
                            else DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have any pawn capable of trading!"));
                        }
                    }
                };

                if (RimworldManager.CheckIfPlayerHasMap())
                {
                    gizmoList.Add(command_Transfer);
                    gizmoList.Add(command_Visit);
                }

                if (__instance.Faction != FactionValues.yourOnlineFaction)
                {
                    gizmoList.Add(command_Spy);
                    gizmoList.Add(command_Raid);
                }

                __result = gizmoList;
            }
        }
    }

    [HarmonyPatch(typeof(Settlement), "GetFloatMenuOptions")]
    public static class PatchPlayerSettlements
    {
        [HarmonyPostfix]
        public static void DoPost(ref IEnumerable<FloatMenuOption> __result, Caravan caravan, Settlement __instance)
        {
            if (FactionValues.playerFactions.Contains(__instance.Faction))
            {
                var gizmoList = __result.ToList();
                gizmoList.Clear();

                if (CaravanVisitUtility.SettlementVisitedNow(caravan) != __instance)
                {
                    foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitSettlement.GetFloatMenuOptions(caravan, __instance))
                    {
                        gizmoList.Add(floatMenuOption2);
                    }
                }

                __result = gizmoList;
            }
        }
    }

    [HarmonyPatch(typeof(Site), "GetGizmos")]
    public static class SiteGizmoPatch
    {
        [HarmonyPostfix]
        public static void DoPost(ref IEnumerable<Gizmo> __result, Site __instance)
        {
            if (Network.state == ClientNetworkState.Disconnected) return;

            List<Gizmo> gizmoList = __result.ToList();
            gizmoList.Clear();

            if (FactionValues.playerFactions.Contains(__instance.Faction))
            {
                Command_Action command_Goodwill = new Command_Action
                {
                    defaultLabel = "Change Goodwill",
                    defaultDesc = "Change the goodwill of this site",
                    icon = ContentFinder<Texture2D>.Get("Commands/Goodwill"),
                    action = delegate
                    {
                        SessionValues.chosenSite = __instance;

                        Action r1 = delegate { GoodwillManager.TryRequestGoodwill(Goodwill.Enemy,
                            GoodwillTarget.Site); };

                        Action r2 = delegate { GoodwillManager.TryRequestGoodwill(Goodwill.Neutral,
                            GoodwillTarget.Site); };

                        Action r3 = delegate { GoodwillManager.TryRequestGoodwill(Goodwill.Ally,
                            GoodwillTarget.Site); };

                        RT_Dialog_3Button d1 = new RT_Dialog_3Button("Change Goodwill", "Set site's goodwill to",
                            "Enemy", "Neutral", "Ally", r1, r2, r3, null);

                        DialogManager.PushNewDialog(d1);
                    }
                };

                if (__instance.Faction != FactionValues.yourOnlineFaction || __instance.Faction != Faction.OfPlayer) gizmoList.Add(command_Goodwill);

                __result = gizmoList;
            }

            else if (__instance.Faction == Find.FactionManager.OfPlayer)
            {
                Command_Action command_Config = new Command_Action
                {
                    defaultLabel = "Change site configs",
                    defaultDesc = "Change the configuration of your sites. These settings affect all sites currently under your control.",
                    icon = ContentFinder<Texture2D>.Get("Commands/SiteConfig"),
                    action = delegate
                    {
                        if (SessionValues.actionValues.EnableSites) DialogManager.PushNewDialog(new RT_Dialog_SiteMenu(true));
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                    }
                };
                
                gizmoList.Add(command_Config);

                __result = gizmoList;
            }
        }
    }

    [HarmonyPatch(typeof(Site), "GetFloatMenuOptions")]
    public static class PatchPlayerSites
    {
        [HarmonyPostfix]
        public static void DoPost(Site __instance, ref IEnumerable<FloatMenuOption> __result)
        {
            if (FactionValues.playerFactions.Contains(__instance.Faction) || __instance.Faction == Faction.OfPlayer)
            {
                var gizmoList = __result.ToList();
                gizmoList.Clear();

                __result = gizmoList;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(Caravan), "GetGizmos")]
    public static class PatchCaravanGizmos
    {
        [HarmonyPostfix]
        public static void ModifyPost(ref IEnumerable<Gizmo> __result, Caravan __instance)
        {
            if (Network.state == ClientNetworkState.Connected && RimworldManager.CheckIfPlayerHasMap())
            {
                Site presentSite = Find.World.worldObjects.Sites.ToList().Find(x => x.Tile == __instance.Tile);
                Settlement presentSettlement = Find.World.worldObjects.Settlements.ToList().Find(x => x.Tile == __instance.Tile);
                List<Gizmo> gizmoList = __result.ToList();

                if (presentSettlement == null && presentSite == null)
                {

                    Command_Action Command_BuildSite = new Command_Action
                    {
                        defaultLabel = "Build a Site",
                        defaultDesc = "Build an utility site for your faction",
                        icon = ContentFinder<Texture2D>.Get("Commands/FSite"),
                        action = delegate
                        {
                            SessionValues.chosenCaravan = __instance;

                            if (SessionValues.actionValues.EnableSites)
                            {
                                DialogManager.PushNewDialog(new RT_Dialog_SiteMenu(false));
                            }
                            else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                        }
                    };
                    gizmoList.Add(Command_BuildSite);
                }

                else if (presentSite != null)
                {
                    Command_Action command_DestroySite = new Command_Action
                    {
                        defaultLabel = "Destroy Site",
                        defaultDesc = "Destroy this site",
                        icon = ContentFinder<Texture2D>.Get("Commands/DestroySite"),
                        action = delegate
                        {
                            SessionValues.chosenCaravan = __instance;
                            SessionValues.chosenSite = Find.WorldObjects.Sites.Find(x => x.Tile == __instance.Tile);

                            if (SessionValues.actionValues.EnableSites) SiteManager.RequestDestroySite();
                            else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                        }
                    };

                    if (presentSite.Faction == Faction.OfPlayer)
                    {
                        gizmoList.Add(command_DestroySite);
                    }
                    else if (presentSite.Faction == FactionValues.yourOnlineFaction)
                    {
                        gizmoList.Add(command_DestroySite);
                    }
                }

                Command_Action Command_BuildRoad = new Command_Action
                {
                    defaultLabel = "Road Builder",
                    defaultDesc = "Build and destroy roads",
                    icon = ContentFinder<Texture2D>.Get("Commands/Road"),
                    action = delegate
                    {
                        SessionValues.chosenCaravan = __instance;

                        if (SessionValues.actionValues.EnableRoads)
                        {
                            List<int> neighborTiles = new List<int>();
                            Find.WorldGrid.GetTileNeighbors(SessionValues.chosenCaravan.Tile, neighborTiles);
                            RoadManagerHelper.ShowRoadChooseDialog(neighborTiles.ToArray(), Find.WorldGrid[__instance.Tile].Roads != null);
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                    }
                };

                gizmoList.Add(Command_BuildRoad);

                __result = gizmoList;
            }
        }
    }

    [HarmonyPatch(typeof(TransportPodsArrivalAction_GiveGift), "GetFloatMenuOptions")]
    public static class PatchDropGift
    {
        [HarmonyPostfix]
        public static void ModifyPost(ref IEnumerable<FloatMenuOption> __result, Settlement settlement, CompLaunchable representative)
        {
            if (FactionValues.playerFactions.Contains(settlement.Faction))
            {
                var floatMenuList = __result.ToList();
                floatMenuList.Clear();

                if (Network.state == ClientNetworkState.Connected)
                {
                    SessionValues.chosenSettlement = settlement;
                    SessionValues.chosendPods = representative;

                    string optionLabel = $"Transfer things to {settlement.Name}";
                    Action toDo = delegate
                    {
                        TransferManager.TakeTransferItemsFromPods(SessionValues.chosendPods);
                        TransferManager.SendTransferRequestToServer(TransferLocation.Pod);
                    };

                    FloatMenuOption floatMenuOption = new FloatMenuOption(optionLabel, toDo);
                    floatMenuList.Add(floatMenuOption);
                }

                __result = floatMenuList;
            }
        }
    }

    [HarmonyPatch(typeof(TransportPodsArrivalAction_AttackSettlement), "GetFloatMenuOptions")]
    public static class PatchDropAttack
    {
        [HarmonyPostfix]
        public static void ModifyPost(ref IEnumerable<FloatMenuOption> __result, Settlement settlement)
        {
            if (FactionValues.playerFactions.Contains(settlement.Faction))
            {
                var floatMenuList = __result.ToList();
                floatMenuList.Clear();

                __result = floatMenuList;
            }
        }
    }

    [HarmonyPatch(typeof(DestroyedSettlement), "GetGizmos")]
    public static class DestroyedSettlementPatch
    {
        [HarmonyPostfix]
        public static void DoPost(ref IEnumerable<Gizmo> __result)
        {
            if (Network.state == ClientNetworkState.Disconnected) return;

            var gizmoList = __result.ToList();
            List<Gizmo> removeList = new List<Gizmo>();

            foreach (Command_Action action in gizmoList.ToList())
            {
                if (action.defaultLabel == "CommandSettle".Translate()) removeList.Add(action);
            }
            foreach (Gizmo g in removeList) gizmoList.Remove(g);

            __result = gizmoList;
        }
    }
}
