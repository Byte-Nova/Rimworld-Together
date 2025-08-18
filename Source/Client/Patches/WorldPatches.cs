using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Patches.Tabs;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetGizmos))]
    public static class Patch_Settlement_GetGizmos
    {
        [HarmonyPostfix]
        public static void DoPost(ref IEnumerable<Gizmo> __result, Settlement __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return;

            List<Gizmo> gizmoList = __result.ToList();

            Command_Action command_PersonalFactionMenu = new Command_Action
            {
                defaultLabel = "Faction Menu",
                defaultDesc = "Access your faction menu",
                icon = ContentFinder<Texture2D>.Get("Commands/FactionMenu"),
                action = delegate
                {
                    if (SessionValues.ActionValues.EnableFactions)
                    {
                        if (ClientValues.HasFaction) GuildManager.OnFactionOpen();
                        else GuildManager.OnNoFactionOpen();
                    }
                    else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                }
            };

            if (__instance.Faction == Find.FactionManager.OfPlayer) gizmoList.Add(command_PersonalFactionMenu);

            __result = gizmoList;
        }
    }

    [HarmonyPatch(typeof(Caravan), nameof(Caravan.GetGizmos))]
    public static class Patch_Caravan_GetGizmos
    {
        [HarmonyPostfix]
        public static void ModifyPost(ref IEnumerable<Gizmo> __result, Caravan __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Connected && RimworldManager.CheckIfPlayerHasMap())
            {
                Site presentSite = Find.World.worldObjects.Sites.ToList().Find(x => x.Tile == __instance.Tile);
                Settlement presentSettlement = Find.World.worldObjects.Settlements.ToList().Find(x => x.Tile == __instance.Tile);
                List<Gizmo> gizmoList = __result.ToList();

                Command_Action Command_BuildSite = new Command_Action
                {
                    defaultLabel = "Build a Site",
                    defaultDesc = "Build an utility site for your faction",
                    icon = ContentFinder<Texture2D>.Get("Commands/FSite"),
                    action = delegate
                    {
                        SessionValues.ChosenCaravan = __instance;

                        if (SessionValues.ActionValues.EnableSites)
                        {
                            RT_Dialog_Base.PushNewDialog(new RT_Dialog_SiteMenu(false));
                        }
                        else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                    }
                };

                Command_Action Command_BuildRoad = new Command_Action
                {
                    defaultLabel = "Road Builder",
                    defaultDesc = "Build and destroy roads",
                    icon = ContentFinder<Texture2D>.Get("Commands/Road"),
                    action = delegate
                    {
                        SessionValues.ChosenCaravan = __instance;

                        if (SessionValues.ActionValues.EnableRoads)
                        {
                            List<PlanetTile> neighborTiles = new List<PlanetTile>();
                            Find.WorldGrid.GetTileNeighbors(SessionValues.ChosenCaravan.Tile, neighborTiles);

                            SurfaceTile selectedTile = (SurfaceTile)Find.WorldGrid[__instance.Tile];
                            RoadManagerHelper.ShowRoadChooseDialog(neighborTiles.ToArray(), selectedTile.Roads != null);
                        }
                        else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                    }
                };

                if (presentSettlement == null && presentSite == null) gizmoList.Add(Command_BuildSite);

                gizmoList.Add(Command_BuildRoad);

                __result = gizmoList;
            }
        }
    }

    [HarmonyPatch(typeof(TransportersArrivalAction_AttackSettlement), "GetFloatMenuOptions")]
    public static class PatchDropAttack
    {
        [HarmonyPostfix]
        public static void ModifyPost(ref IEnumerable<FloatMenuOption> __result, Settlement settlement)
        {
            if (ClientValues.PlayerFactions.Contains(settlement.Faction))
            {
                List<FloatMenuOption> floatMenuList = __result.ToList();

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
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return;

            List<Gizmo> gizmoList = __result.ToList();
            List<Gizmo> removeList = new List<Gizmo>();
            foreach (Command_Action action in gizmoList.ToList())
            {
                if (action.defaultLabel == "CommandSettle".Translate()) removeList.Add(action);
            }

            foreach (Gizmo gizmo in removeList) gizmoList.Remove(gizmo);

            __result = gizmoList;
        }
    }

    [HarmonyPatch(typeof(WorldInspectPane), "CurTabs", MethodType.Getter)]
    public static class Patch_WorldInspectPane_CurTabs
    {
        [HarmonyPrefix]
        public static bool DoPre(WorldInspectPane __instance, ref IEnumerable<InspectTabBase> __result)
        {
            if (SessionValues.CurrentNetworkState != ClientNetworkState.Connected) return false;
            else
            {
                if (Find.WorldSelector.NumSelectedObjects == 1)
                {
                    __result = Find.WorldSelector.SingleSelectedObject.GetInspectTabs();
                }

                if (Find.WorldSelector.NumSelectedObjects == 0 && Find.WorldSelector.SelectedTile.Valid)
                {
                    __result = PlanetLayer.Selected.Def.Tabs;
                    __result = __result.AddItem(new PlayersUI());
                    __result = __result.AddItem(new BasesUI());
                    __result = __result.AddItem(new SitesUI());
                }

                return false;
            }
        }
    }

    // Makes sure pawns from other players don't get passed into the world

    [HarmonyPatch(typeof(WorldPawns), nameof(WorldPawns.PassToWorld))]
    public static class Patch_WorldPawns_PassToWorld
    {
        [HarmonyPrefix]
        public static bool DoPre(Pawn pawn)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (!ClientValues.PlayerFactions.Contains(pawn.Faction)) return true;
            else return false;
        }
    }
}
