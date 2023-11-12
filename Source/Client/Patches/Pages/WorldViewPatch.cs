using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Patches.Tabs;
using RimworldTogether.GameClient.Values;
using Shared.Misc;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(WorldInspectPane), "SetInitialSizeAndPosition")]
    public static class AddSideTabs
    {
        [HarmonyPrefix]
        public static bool DoPre(ref WITab[] ___TileTabs)
        {
            if (___TileTabs.Count() != 5 && Network.Network.isConnectedToServer)
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
            if (!Network.Network.isConnectedToServer) return true;
            else
            {
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
    }

    [HarmonyPatch(typeof(Settlement), "GetGizmos")]
    public static class SettlementGizmoPatch
    {
        [HarmonyPostfix]
        public static void DoPost(ref IEnumerable<Gizmo> __result, Settlement __instance)
        {
            if (!Network.Network.isConnectedToServer) return;

            if (FactionValues.playerFactions.Contains(__instance.Faction))
            {
                var gizmoList = __result.ToList();
                gizmoList.Clear();

                Command_Action command_Likelihood = new Command_Action
                {
                    defaultLabel = "Change Likelihood",
                    defaultDesc = "Change the likelihood of this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Likelihood"),
                    action = delegate
                    {
                        ClientValues.chosenSettlement = __instance;

                        Action r1 = delegate { LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Enemy, 
                            CommonEnumerators.LikelihoodTarget.Settlement); };

                        Action r2 = delegate { LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Neutral,
                            CommonEnumerators.LikelihoodTarget.Settlement); };

                        Action r3 = delegate { LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Ally,
                            CommonEnumerators.LikelihoodTarget.Settlement); };

                        RT_Dialog_3Button d1 = new RT_Dialog_3Button("Change Likelihood", "Set settlement's likelihood to",
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
                        ClientValues.chosenSettlement = __instance;

                        if (ClientValues.chosenSettlement.Faction == FactionValues.yourOnlineFaction) OnlineFactionManager.OnFactionOpenOnMember();
                        else OnlineFactionManager.OnFactionOpenOnNonMember();
                    }
                };

                Command_Action command_Caravan = new Command_Action
                {
                    defaultLabel = "Form Caravan",
                    defaultDesc = "Form a new caravan",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan"),
                    action = delegate
                    {
                        ClientValues.chosenSettlement = __instance;

                        Dialog_FormCaravan d1 = new Dialog_FormCaravan(__instance.Map, mapAboutToBeRemoved:true);
                        DialogManager.PushNewDialog(d1);
                    }
                };

                if (ServerValues.hasFaction) gizmoList.Add(command_FactionMenu);
                if (__instance.Faction != FactionValues.yourOnlineFaction) gizmoList.Add(command_Likelihood);
                if (__instance.Map != null && __instance.Map.mapPawns.AllPawns.ToList().Find(fetch => fetch.Faction == Faction.OfPlayer) != null)
                {
                    gizmoList.Add(command_Caravan);
                }
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
                        ClientValues.chosenSettlement = __instance;

                        if (ServerValues.hasFaction) OnlineFactionManager.OnFactionOpen();
                        else OnlineFactionManager.OnNoFactionOpen();
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
            if (!Network.Network.isConnectedToServer) return;

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
                        ClientValues.chosenSettlement = __instance;
                        ClientValues.chosenCaravan = caravan;

                        SpyManager.RequestSpy();
                    }
                };

                Command_Action command_Raid = new Command_Action
                {
                    defaultLabel = "Raid Settlement",
                    defaultDesc = "Raid this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Raid"),
                    action = delegate
                    {
                        ClientValues.chosenSettlement = __instance;
                        ClientValues.chosenCaravan = caravan;

                        RaidManager.RequestRaid();
                    }
                };

                Command_Action command_Visit = new Command_Action
                {
                    defaultLabel = "Visit Settlement",
                    defaultDesc = "Visit this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Visit"),
                    action = delegate
                    {
                        ClientValues.chosenSettlement = __instance;
                        ClientValues.chosenCaravan = caravan;

                        RT_Dialog_2Button d1 = new RT_Dialog_2Button("Visit Mode", "Please choose your visit mode",
                            "Online", "Offline",
                            delegate { VisitManager.RequestVisit(); },
                            delegate { OfflineVisitManager.OnOfflineVisitAccept(); },
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
                        ClientValues.chosenSettlement = __instance;
                        ClientValues.chosenCaravan = caravan;

                        if (RimworldManager.CheckForAnySocialPawn(RimworldManager.SearchLocation.Caravan))
                        {
                            DialogManager.PushNewDialog(new RT_Dialog_TransferMenu(CommonEnumerators.TransferLocation.Caravan, true, true, true));
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have any pawn capable of trading!"));
                    }
                };

                Command_Action command_Event = new Command_Action
                {
                    defaultLabel = "Send Event",
                    defaultDesc = "Send an event to this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Event"),
                    action = delegate
                    {
                        ClientValues.chosenSettlement = __instance;
                        ClientValues.chosenCaravan = caravan;

                        RT_Dialog_ScrollButtons d1 = new RT_Dialog_ScrollButtons("Event Selector", "Choose the even you want to send",
                            EventManager.eventNames, EventManager.ShowSendEventDialog, null);

                        DialogManager.PushNewDialog(d1);
                    }
                };

                Command_Action command_Likelihood = new Command_Action
                {
                    defaultLabel = "Change Likelihood",
                    defaultDesc = "Change the likelihood of this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Likelihood"),
                    action = delegate
                    {
                        ClientValues.chosenSettlement = __instance;

                        Action r1 = delegate {
                            LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Enemy,
                            CommonEnumerators.LikelihoodTarget.Settlement);
                        };

                        Action r2 = delegate {
                            LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Neutral,
                            CommonEnumerators.LikelihoodTarget.Settlement);
                        };

                        Action r3 = delegate {
                            LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Ally,
                            CommonEnumerators.LikelihoodTarget.Settlement);
                        };

                        RT_Dialog_3Button d1 = new RT_Dialog_3Button("Change Likelihood", "Set settlement's likelihood to",
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
                        ClientValues.chosenSettlement = __instance;

                        if (ClientValues.chosenSettlement.Faction == FactionValues.yourOnlineFaction) OnlineFactionManager.OnFactionOpenOnMember();
                        else OnlineFactionManager.OnFactionOpenOnNonMember();
                    }
                };

                if (RimworldManager.CheckIfPlayerHasMap())
                {
                    gizmoList.Add(command_Transfer);
                    gizmoList.Add(command_Visit);
                }

                if (ServerValues.hasFaction) gizmoList.Add(command_FactionMenu);
                if (__instance.Faction != FactionValues.yourOnlineFaction)
                {
                    gizmoList.Add(command_Likelihood);
                    gizmoList.Add(command_Spy);
                    gizmoList.Add(command_Raid);
                }
                gizmoList.Add(command_Event);
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
            if (!Network.Network.isConnectedToServer) return;

            if (FactionValues.playerFactions.Contains(__instance.Faction))
            {
                var gizmoList = __result.ToList();
                gizmoList.Clear();

                Command_Action command_Likelihood = new Command_Action
                {
                    defaultLabel = "Change Likelihood",
                    defaultDesc = "Change the likelihood of this site",
                    icon = ContentFinder<Texture2D>.Get("Commands/Likelihood"),
                    action = delegate
                    {
                        ClientValues.chosenSite = __instance;

                        Action r1 = delegate { LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Enemy,
                            CommonEnumerators.LikelihoodTarget.Site); };

                        Action r2 = delegate { LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Neutral,
                            CommonEnumerators.LikelihoodTarget.Site); };

                        Action r3 = delegate { LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Ally,
                            CommonEnumerators.LikelihoodTarget.Site); };

                        RT_Dialog_3Button d1 = new RT_Dialog_3Button("Change Likelihood", "Set site's likelihood to",
                            "Enemy", "Neutral", "Ally", r1, r2, r3, null);

                        DialogManager.PushNewDialog(d1);
                    }
                };

                if (__instance.Faction != FactionValues.yourOnlineFaction) gizmoList.Add(command_Likelihood);

                __result = gizmoList;
            }

            else if (__instance.Faction == Find.FactionManager.OfPlayer)
            {
                var gizmoList = __result.ToList();
                gizmoList.Clear();

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
            if (Network.Network.isConnectedToServer && RimworldManager.CheckIfPlayerHasMap())
            {
                Site presentSite = Find.World.worldObjects.Sites.ToList().Find(x => x.Tile == __instance.Tile);
                Settlement presentSettlement = Find.World.worldObjects.Settlements.ToList().Find(x => x.Tile == __instance.Tile);
                List<Gizmo> gizmoList = __result.ToList();

                if (presentSettlement == null && presentSite == null)
                {
                    Command_Action Command_BuildSite = new Command_Action
                    {
                        defaultLabel = "Build Personal Site",
                        defaultDesc = "Build an utility site for your convenience",
                        icon = ContentFinder<Texture2D>.Get("Commands/PSite"),
                        action = delegate
                        {
                            ClientValues.chosenCaravan = __instance;

                            RT_Dialog_ScrollButtons d1 = new RT_Dialog_ScrollButtons("Buildable Personal Sites", "Available sites to build",
                                SiteManager.siteDefLabels, PersonalSiteManager.PushConfirmSiteDialog, null);

                            DialogManager.PushNewDialog(d1);
                        }
                    };

                    Command_Action Command_BuildFactionSite = new Command_Action
                    {
                        defaultLabel = "Build Faction Site",
                        defaultDesc = "Build an utility site for your faction",
                        icon = ContentFinder<Texture2D>.Get("Commands/FSite"),
                        action = delegate
                        {
                            ClientValues.chosenCaravan = __instance;

                            RT_Dialog_ScrollButtons d1 = new RT_Dialog_ScrollButtons("Buildable Faction Sites", "Available sites to build",
                                SiteManager.siteDefLabels, FactionSiteManager.PushConfirmSiteDialog, null);

                            DialogManager.PushNewDialog(d1);
                        }
                    };

                    gizmoList.Add(Command_BuildSite);
                    if (ServerValues.hasFaction) gizmoList.Add(Command_BuildFactionSite);
                }

                else if (presentSite != null)
                {
                    Command_Action command_Likelihood = new Command_Action
                    {
                        defaultLabel = "Change Likelihood",
                        defaultDesc = "Change the likelihood of this site",
                        icon = ContentFinder<Texture2D>.Get("Commands/Likelihood"),
                        action = delegate
                        {
                            ClientValues.chosenSite = Find.WorldObjects.Sites.Find(x => x.Tile == __instance.Tile);

                            Action r1 = delegate {
                                LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Enemy,
                                    CommonEnumerators.LikelihoodTarget.Site);
                            };

                            Action r2 = delegate {
                                LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Neutral,
                                    CommonEnumerators.LikelihoodTarget.Site);
                            };

                            Action r3 = delegate {
                                LikelihoodManager.TryRequestLikelihood(CommonEnumerators.Likelihoods.Ally,
                                    CommonEnumerators.LikelihoodTarget.Site);
                            };

                            RT_Dialog_3Button d1 = new RT_Dialog_3Button("Change Likelihood", "Set site's likelihood to",
                                "Enemy", "Neutral", "Ally", r1, r2, r3, null);

                            DialogManager.PushNewDialog(d1);
                        }
                    };

                    Command_Action command_AccessPersonalSite = new Command_Action
                    {
                        defaultLabel = "Access Personal Site",
                        defaultDesc = "Access your personal site",
                        icon = ContentFinder<Texture2D>.Get("Commands/PSite"),
                        action = delegate
                        {
                            ClientValues.chosenCaravan = __instance;
                            ClientValues.chosenSite = Find.WorldObjects.Sites.Find(x => x.Tile == __instance.Tile);

                            SiteManager.OnSimpleSiteRequest();
                        }
                    };

                    Command_Action command_DestroySite = new Command_Action
                    {
                        defaultLabel = "Destroy Site",
                        defaultDesc = "Destroy this site",
                        icon = ContentFinder<Texture2D>.Get("Commands/DestroySite"),
                        action = delegate
                        {
                            ClientValues.chosenCaravan = __instance;
                            ClientValues.chosenSite = Find.WorldObjects.Sites.Find(x => x.Tile == __instance.Tile);

                            SiteManager.RequestDestroySite();
                        }
                    };

                    if (presentSite.Faction == Faction.OfPlayer)
                    {
                        gizmoList.Add(command_AccessPersonalSite);
                        gizmoList.Add(command_DestroySite);
                    }

                    else if (presentSite.Faction == FactionValues.yourOnlineFaction)
                    {
                        gizmoList.Add(command_DestroySite);
                    }

                    else gizmoList.Add(command_Likelihood);
                }

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

                if (Network.Network.isConnectedToServer)
                {
                    ClientValues.chosenSettlement = settlement;
                    ClientValues.chosendPods = representative;

                    string optionLabel = $"Transfer things to {settlement.Name}";
                    Action toDo = delegate
                    {
                        TransferManager.TakeTransferItemsFromPods(ClientValues.chosendPods);
                        TransferManager.SendTransferRequestToServer(CommonEnumerators.TransferLocation.Pod);
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
            if (!Network.Network.isConnectedToServer) return;

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
