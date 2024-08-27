using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class RoadManager
    {
        public static void ParsePacket(Packet packet)
        {
            RoadData data = Serializer.ConvertBytesToObject<RoadData>(packet.contents);

            switch (data.stepMode)
            {
                case RoadStepMode.Add:
                    AddRoadSimple(data.details.fromTile, data.details.toTile, RoadManagerHelper.GetRoadDefFromDefName(data.details.roadDefName), true);
                    break;

                case RoadStepMode.Remove:
                    RemoveRoadSimple(data.details.fromTile, data.details.toTile, true);
                    break;
            }
        }

        public static void SendRoadAddRequest(int tileAID, int tileBID, RoadDef roadDef)
        {
            RoadData data = new RoadData();
            data.stepMode = RoadStepMode.Add;

            data.details = new RoadDetails();
            data.details.fromTile = tileAID;
            data.details.toTile = tileBID;
            data.details.roadDefName = roadDef.defName;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.RoadPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void SendRoadRemoveRequest(int tileAID, int tileBID)
        {
            RoadData data = new RoadData();
            data.stepMode = RoadStepMode.Remove;

            data.details = new RoadDetails();
            data.details.fromTile = tileAID;
            data.details.toTile = tileBID;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.RoadPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void AddRoads(RoadDetails[] details, bool forceRefresh)
        {
            if (details == null) return;

            foreach (RoadDetails detail in details)
            {
                AddRoadSimple(detail.fromTile, detail.toTile, RoadManagerHelper.GetRoadDefFromDefName(detail.roadDefName), forceRefresh);
            }

            //If we don't want to force refresh we wait for all and then refresh the layer
            if (!forceRefresh) RoadManagerHelper.ForceRoadLayerRefresh();
        }

        public static void AddRoadSimple(int tileAID, int tileBID, RoadDef roadDef, bool forceRefresh)
        {
            if (!RoadManagerHelper.CheckIfCanBuildRoadOnTile(tileBID))
            {
                Logger.Warning($"Tried building a road at '{tileBID}' when it's not possible");
                return;
            }

            Tile tileA = Find.WorldGrid[tileAID];
            Tile tileB = Find.WorldGrid[tileBID];

            AddRoadLink(tileA, tileBID, roadDef);
            AddRoadLink(tileB, tileAID, roadDef);

            if (forceRefresh) RoadManagerHelper.ForceRoadLayerRefresh();
        }

        private static void AddRoadLink(Tile toAddTo, int neighborTileID, RoadDef roadDef)
        {
            if (toAddTo.Roads != null)
            {
                foreach (Tile.RoadLink roadLink in toAddTo.Roads)
                {
                    if (roadLink.neighbor == neighborTileID) return;
                }
            }

            Tile.RoadLink linkToAdd = new Tile.RoadLink
            {
                neighbor = neighborTileID,
                road = roadDef
            };

            toAddTo.potentialRoads ??= new List<Tile.RoadLink>();
            toAddTo.potentialRoads.Add(linkToAdd);
        }

        public static void ClearAllRoads()
        {
            foreach (Tile tile in Find.WorldGrid.tiles)
            {
                tile.Roads?.Clear();
                tile.potentialRoads = null;
            }

            RoadManagerHelper.ForceRoadLayerRefresh();
        }

        private static void RemoveRoadSimple(int tileAID, int tileBID, bool forceRefresh)
        {
            Tile tileA = Find.WorldGrid[tileAID];
            Tile tileB = Find.WorldGrid[tileBID];

            foreach (Tile.RoadLink roadLink in tileA.Roads.ToList())
            {
                if (roadLink.neighbor == tileBID)
                {
                    tileA.Roads.Remove(roadLink);
                    tileA.potentialRoads.Remove(roadLink);

                    //We need this to let the game know it shouldn't try to draw anything in here if there's no roads
                    if (tileA.potentialRoads.Count() == 0) tileA.potentialRoads = null;
                }
            }

            foreach (Tile.RoadLink roadLink in tileB.Roads.ToList())
            {
                if (roadLink.neighbor == tileAID)
                {
                    tileB.Roads.Remove(roadLink);
                    tileB.potentialRoads.Remove(roadLink);

                    //We need this to let the game know it shouldn't try to draw anything in here if there's no roads
                    if (tileB.potentialRoads.Count() == 0) tileB.potentialRoads = null;
                }
            }

            if (forceRefresh) RoadManagerHelper.ForceRoadLayerRefresh();
        }
    }

    public static class RoadManagerHelper
    {
        public static RoadDetails[] tempRoadDetails;
        public static RoadDef[] allowedRoadDefs;
        public static int[] allowedRoadCosts;

        public static RoadDef DirtPathDef => DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == "DirtPath");
        public static RoadDef DirtRoadDef => DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == "DirtRoad");
        public static RoadDef StoneRoadDef => DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == "StoneRoad");
        public static RoadDef AncientAsphaltRoadDef => DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == "AncientAsphaltRoad");
        public static RoadDef AncientAsphaltHighwayDef => DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == "AncientAsphaltHighway");

        public static void SetValues(ServerGlobalData serverGlobalData) 
        {
            tempRoadDetails = serverGlobalData.roads;

            List<RoadDef> allowedRoads = new List<RoadDef>();
            if (serverGlobalData.roadValues.AllowDirtPath) allowedRoads.Add(DirtPathDef);
            if (serverGlobalData.roadValues.AllowDirtRoad) allowedRoads.Add(DirtRoadDef);
            if (serverGlobalData.roadValues.AllowStoneRoad) allowedRoads.Add(StoneRoadDef);
            if (serverGlobalData.roadValues.AllowAsphaltPath) allowedRoads.Add(AncientAsphaltRoadDef);
            if (serverGlobalData.roadValues.AllowAsphaltHighway) allowedRoads.Add(AncientAsphaltHighwayDef);
            allowedRoadDefs = allowedRoads.ToArray();

            List<int> allowedCosts = new List<int>();
            if (serverGlobalData.roadValues.AllowDirtPath) allowedCosts.Add(serverGlobalData.roadValues.DirtPathCost);
            if (serverGlobalData.roadValues.AllowDirtRoad) allowedCosts.Add(serverGlobalData.roadValues.DirtRoadCost);
            if (serverGlobalData.roadValues.AllowStoneRoad) allowedCosts.Add(serverGlobalData.roadValues.StoneRoadCost);
            if (serverGlobalData.roadValues.AllowAsphaltPath) allowedCosts.Add(serverGlobalData.roadValues.AsphaltPathCost);
            if (serverGlobalData.roadValues.AllowAsphaltHighway) allowedCosts.Add(serverGlobalData.roadValues.AsphaltHighwayCost);
            allowedRoadCosts = allowedCosts.ToArray();
        }

        public static bool CheckIfTwoTilesAreConnected(int tileAID, int tileBID)
        {
            Tile tileA = Find.WorldGrid[tileAID];

            if (tileA.Roads != null)
            {
                foreach (Tile.RoadLink roadLink in tileA.Roads)
                {
                    if (roadLink.neighbor == tileBID) return true;
                }
            }

            return false;
        }

        public static bool CheckIfCanBuildRoadOnTile(int tileID)
        {
            Tile tile = Find.WorldGrid[tileID];

            if (tile.WaterCovered) return false;
            else if (!Find.WorldPathGrid.Passable(tileID)) return false;
            else return true;
        }

        public static string[] GetAvailableRoadLabels(bool includePrices)
        {
            List<string> roadLabels = new List<string>();
            for(int i = 0; i < allowedRoadDefs.Length; i++)
            {
                RoadDef def = allowedRoadDefs[i];

                if (includePrices) roadLabels.Add($"{def.LabelCap} > {allowedRoadCosts[i]}$/u");
                else roadLabels.Add(def.LabelCap);
            }

            return roadLabels.ToArray();
        }

        public static RoadDef GetRoadDefFromDefName(string defName)
        {
            return DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == defName);
        }

        public static void ChooseRoadDialogs(int[] neighborTiles, bool hasRoadOnTile)
        {
            if (hasRoadOnTile)
            {
                RT_Dialog_2Button d1 = new RT_Dialog_2Button("RTRoadManager".Translate(), "RTRoadManagerDesc".Translate(),
                    "RTRoadBuild".Translate(), "RTRoadDestroy".Translate(), delegate { ShowRoadBuildDialog(neighborTiles); }, delegate { ShowRoadDestroyDialog(neighborTiles); }, null);

                DialogManager.PushNewDialog(d1);
            }
            else ShowRoadBuildDialog(neighborTiles);
        }

        public static void ShowRoadBuildDialog(int[] neighborTiles)
        {
            List<string> selectableTileLabels = new List<string>();
            List<int> selectableTiles = new List<int>();

            foreach (int tileID in neighborTiles)
            {
                if (!CheckIfCanBuildRoadOnTile(tileID)) continue;
                else if (CheckIfTwoTilesAreConnected(SessionValues.chosenCaravan.Tile, tileID)) continue;
                else
                {
                    Vector2 vector = Find.WorldGrid.LongLatOf(tileID);
                    string toDisplay = $"";
                    selectableTileLabels.Add(toDisplay);
                    selectableTiles.Add(tileID);
                }
            }

            Action r1 = delegate
            {
                int selectedTile = selectableTiles[DialogManager.dialogButtonListingResultInt];

                RT_Dialog_ListingWithButton d1 = new RT_Dialog_ListingWithButton("RTRoadManager".Translate(), "RTRoadBuilding".Translate(),
                    GetAvailableRoadLabels(true),
                    delegate
                    {
                        int selectedIndex = DialogManager.dialogButtonListingResultInt;

                        if (RimworldManager.CheckIfHasEnoughSilverInCaravan(SessionValues.chosenCaravan, allowedRoadCosts[selectedIndex]))
                        {
                            RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, allowedRoadCosts[selectedIndex], SessionValues.chosenCaravan);
                            RoadManager.SendRoadAddRequest(SessionValues.chosenCaravan.Tile, selectedTile, allowedRoadDefs[selectedIndex]);
                            SaveManager.ForceSave();
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("RTNotEnoughSilver".Translate()));
                    });

                DialogManager.PushNewDialog(d1);
            };

            DialogManager.PushNewDialog(new RT_Dialog_ListingWithButton("RTRoadManager".Translate(), "RTRoadTileToConnect".Translate(),
                selectableTileLabels.ToArray(), r1));
        }

        public static void ShowRoadDestroyDialog(int[] neighborTiles)
        {
            List<string> selectableTilesLabels = new List<string>();
            List<int> selectableTiles = new List<int>();

            foreach (int tileID in neighborTiles)
            {
                if (CheckIfTwoTilesAreConnected(SessionValues.chosenCaravan.Tile, tileID))
                {
                    Vector2 vector = Find.WorldGrid.LongLatOf(tileID);
                    string toDisplay = "RTRoadTile".Translate(vector.y.ToStringLatitude(), vector.x.ToStringLongitude());
                    selectableTilesLabels.Add(toDisplay);
                    selectableTiles.Add(tileID);
                }
            }

            Action r1 = delegate
            {
                int selectedTile = selectableTiles[DialogManager.dialogButtonListingResultInt];

                RoadManager.SendRoadRemoveRequest(SessionValues.chosenCaravan.Tile, selectedTile);
            };

            DialogManager.PushNewDialog(new RT_Dialog_ListingWithButton("RTRoadDestroyer".Translate(), "RTRoadDestroyerDesc".Translate(),
                selectableTilesLabels.ToArray(), r1));
        }

        public static RoadDetails[] GetPlanetRoads()
        {
            List<RoadDetails> toGet = new List<RoadDetails>();
            foreach (Tile tile in Find.WorldGrid.tiles)
            {
                if (tile.Roads != null)
                {
                    foreach (Tile.RoadLink link in tile.Roads)
                    {
                        RoadDetails details = new RoadDetails();
                        details.fromTile = Find.WorldGrid.tiles.IndexOf(tile);
                        details.toTile = link.neighbor;
                        details.roadDefName = link.road.defName;

                        if (!CheckIfExists(details.fromTile, details.toTile)) toGet.Add(details);
                    }
                }
            }
            return toGet.ToArray();

            bool CheckIfExists(int tileA, int tileB)
            {
                foreach (RoadDetails details in toGet)
                {
                    if (details.fromTile == tileA && details.toTile == tileB) return true;
                    else if (details.fromTile == tileB && details.toTile == tileA) return true;
                }

                return false;
            }
        }

        public static void ForceRoadLayerRefresh()
        {
            Find.World.renderer.SetDirty<WorldLayer_Roads>();
            Find.World.renderer.RegenerateLayersIfDirtyInLongEvent();
        }
    }
}
