using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.TCP;
using GameClient.Values;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class RoadManager
    {
        [HandlesPacket(PacketHeader.RoadManager)]
        private static void ParsePacket(byte[] bytes)
        {
            RoadData data = Serializer.ConvertBytesToObject<RoadData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case RoadStepMode.Add:
                    AddRoadSimple(data._details.FromTile, data._details.ToTile, RoadManagerHelper.GetRoadDefFromDefName(data._details.RoadDefName), true);
                    break;

                case RoadStepMode.Remove:
                    RemoveRoadSimple(data._details.FromTile, data._details.ToTile, true);
                    break;
            }
        }

        public static void SendRoadAddRequest(int tileAID, int tileBID, RoadDef roadDef)
        {
            RoadData data = new RoadData();
            data._stepMode = RoadStepMode.Add;

            data._details = new RoadDetails();
            data._details.FromTile = tileAID;
            data._details.ToTile = tileBID;
            data._details.RoadDefName = roadDef.defName;

            Network.Listener.EnqueuePacket(PacketHeader.RoadManager, data);
        }

        public static void SendRoadRemoveRequest(int tileAID, int tileBID)
        {
            RoadData data = new RoadData();
            data._stepMode = RoadStepMode.Remove;

            data._details = new RoadDetails();
            data._details.FromTile = tileAID;
            data._details.ToTile = tileBID;

            Network.Listener.EnqueuePacket(PacketHeader.RoadManager, data);
        }

        public static void AddRoads(RoadDetails[] details, bool forceRefresh)
        {
            if (details == null) return;

            foreach (RoadDetails detail in details)
            {
                AddRoadSimple(detail.FromTile, detail.ToTile, RoadManagerHelper.GetRoadDefFromDefName(detail.RoadDefName), forceRefresh);
            }

            //If we don't want to force refresh we wait for all and then refresh the layer
            if (!forceRefresh) RoadManagerHelper.ForceRoadLayerRefresh();
        }

        public static void AddRoadSimple(int tileAID, int tileBID, RoadDef roadDef, bool forceRefresh)
        {
            if (!RoadManagerHelper.CheckIfCanBuildRoadOnTile(tileBID))
            {
                Printer.Warning($"Tried building a road at '{tileBID}' when it's not possible");
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
            tempRoadDetails = serverGlobalData._roads;

            List<RoadDef> allowedRoads = new List<RoadDef>();
            if (serverGlobalData._roadValues.AllowDirtPath) allowedRoads.Add(DirtPathDef);
            if (serverGlobalData._roadValues.AllowDirtRoad) allowedRoads.Add(DirtRoadDef);
            if (serverGlobalData._roadValues.AllowStoneRoad) allowedRoads.Add(StoneRoadDef);
            if (serverGlobalData._roadValues.AllowAsphaltPath) allowedRoads.Add(AncientAsphaltRoadDef);
            if (serverGlobalData._roadValues.AllowAsphaltHighway) allowedRoads.Add(AncientAsphaltHighwayDef);
            allowedRoadDefs = allowedRoads.ToArray();

            List<int> allowedCosts = new List<int>();
            if (serverGlobalData._roadValues.AllowDirtPath) allowedCosts.Add(serverGlobalData._roadValues.DirtPathCost);
            if (serverGlobalData._roadValues.AllowDirtRoad) allowedCosts.Add(serverGlobalData._roadValues.DirtRoadCost);
            if (serverGlobalData._roadValues.AllowStoneRoad) allowedCosts.Add(serverGlobalData._roadValues.StoneRoadCost);
            if (serverGlobalData._roadValues.AllowAsphaltPath) allowedCosts.Add(serverGlobalData._roadValues.AsphaltPathCost);
            if (serverGlobalData._roadValues.AllowAsphaltHighway) allowedCosts.Add(serverGlobalData._roadValues.AsphaltHighwayCost);
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
            try
            {
                Tile tile = Find.WorldGrid[tileID];

                if (tile.WaterCovered) return false;
                else if (!Find.WorldPathGrid.Passable(tileID)) return false;
                else return true;
            }
            catch { return false; }
        }

        public static string[] GetAvailableRoadLabels(bool includePrices)
        {
            List<string> roadLabels = new List<string>();
            for (int i = 0; i < allowedRoadDefs.Length; i++)
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

        public static void ShowRoadChooseDialog(int[] neighborTiles, bool hasRoadOnTile)
        {
            if (hasRoadOnTile)
            {
                RT_Dialog_Buttons d1 = new RT_Dialog_Buttons("Road manager", "Select the action you want to do",
                    new string[] { "Build", "Destroy" }, 
                    new Action[] { delegate { ShowRoadBuildDialog(neighborTiles); }, delegate { ShowRoadDestroyDialog(neighborTiles); } }, 
                    null);

                RT_Dialog_Base.PushNewDialog(d1);
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
                else if (CheckIfTwoTilesAreConnected(SessionValues.ChosenCaravan.Tile, tileID)) continue;
                else
                {
                    Vector2 vector = Find.WorldGrid.LongLatOf(tileID);
                    string toDisplay = $"Tile at {vector.y.ToStringLatitude()} - {vector.x.ToStringLongitude()}";
                    selectableTileLabels.Add(toDisplay);
                    selectableTiles.Add(tileID);
                }
            }

            Action r1 = delegate
            {
                int selectedTile = selectableTiles[RT_Dialog_ListingWithButton.DialogButtonListingResultInt];

                RT_Dialog_ListingWithButton d1 = new RT_Dialog_ListingWithButton("Road builder", "Select road type to use",
                    GetAvailableRoadLabels(true),
                    delegate
                    {
                        int selectedIndex = RT_Dialog_ListingWithButton.DialogButtonListingResultInt;

                        if (RimworldManager.CheckIfHasEnoughSilverInCaravan(SessionValues.ChosenCaravan, allowedRoadCosts[selectedIndex]))
                        {
                            RimworldManager.RemoveThingFromCaravan(SessionValues.ChosenCaravan, ThingDefOf.Silver, allowedRoadCosts[selectedIndex]);
                            RoadManager.SendRoadAddRequest(SessionValues.ChosenCaravan.Tile, selectedTile, allowedRoadDefs[selectedIndex]);
                            SaveManager.ForceSave();
                        }
                        else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You do not have enough silver for this action!" }));
                    });

                RT_Dialog_Base.PushNewDialog(d1);
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_ListingWithButton("Road builder", "Select a tile to connect with",
                selectableTileLabels.ToArray(), r1));
        }

        public static void ShowRoadDestroyDialog(int[] neighborTiles)
        {
            List<string> selectableTilesLabels = new List<string>();
            List<int> selectableTiles = new List<int>();

            foreach (int tileID in neighborTiles)
            {
                if (CheckIfTwoTilesAreConnected(SessionValues.ChosenCaravan.Tile, tileID))
                {
                    Vector2 vector = Find.WorldGrid.LongLatOf(tileID);
                    string toDisplay = $"Tile at {vector.y.ToStringLatitude()} - {vector.x.ToStringLongitude()}";
                    selectableTilesLabels.Add(toDisplay);
                    selectableTiles.Add(tileID);
                }
            }

            Action r1 = delegate
            {
                int selectedTile = selectableTiles[RT_Dialog_ListingWithButton.DialogButtonListingResultInt];

                RoadManager.SendRoadRemoveRequest(SessionValues.ChosenCaravan.Tile, selectedTile);
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_ListingWithButton("Road destroyer", "Select a tile to disconnect from",
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
                        details.FromTile = Find.WorldGrid.tiles.IndexOf(tile);
                        details.ToTile = link.neighbor;
                        details.RoadDefName = link.road.defName;

                        if (!CheckIfExists(details.FromTile, details.ToTile)) toGet.Add(details);
                    }
                }
            }
            return toGet.ToArray();

            bool CheckIfExists(int tileA, int tileB)
            {
                foreach (RoadDetails details in toGet)
                {
                    if (details.FromTile == tileA && details.ToTile == tileB) return true;
                    else if (details.FromTile == tileB && details.ToTile == tileA) return true;
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
