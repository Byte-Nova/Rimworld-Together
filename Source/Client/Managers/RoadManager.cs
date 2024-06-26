using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace GameClient
{
    public static class RoadManager
    {
        public static void ParsePacket(Packet packet)
        {
            RoadData data = (RoadData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.RoadStepMode.Add:
                    AddRoadSimple(data.details.tileA, data.details.tileB, RoadManagerHelper.GetRoadDefFromDefName(data.details.roadDefName), true);
                    break;

                case CommonEnumerators.RoadStepMode.Remove:
                    RemoveRoadSimple(data.details.tileA, data.details.tileB, true);
                    break;
            }
        }

        public static void SendRoadAddRequest(int tileAID, int tileBID, RoadDef roadDef)
        {
            RoadData data = new RoadData();
            data.stepMode = CommonEnumerators.RoadStepMode.Add;

            data.details = new RoadDetails();
            data.details.tileA = tileAID;
            data.details.tileB = tileBID;
            data.details.roadDefName = roadDef.defName;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RoadPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void SendRoadRemoveRequest(int tileAID, int tileBID)
        {
            RoadData data = new RoadData();
            data.stepMode = CommonEnumerators.RoadStepMode.Remove;

            data.details = new RoadDetails();
            data.details.tileA = tileAID;
            data.details.tileB = tileBID;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RoadPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void AddRoads(RoadDetails[] roads, bool forceRefresh)
        {
            foreach (RoadDetails details in roads)
            {
                AddRoadSimple(details.tileA, details.tileB, RoadManagerHelper.GetRoadDefFromDefName(details.roadDefName), forceRefresh);
            }

            //If we don't want to force refresh we wait for all the roads and then refresh the layer

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
                }
            }

            foreach (Tile.RoadLink roadLink in tileB.Roads.ToList())
            {
                if (roadLink.neighbor == tileAID)
                {
                    tileB.Roads.Remove(roadLink);
                    tileB.potentialRoads.Remove(roadLink);
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

        public static void SetRoadValues(ServerGlobalData serverGlobalData) 
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
                RT_Dialog_2Button d1 = new RT_Dialog_2Button("Road manager", "Select the action you want to do",
                    "Build", "Destroy", delegate { ShowRoadBuildDialog(neighborTiles); }, delegate { ShowRoadDestroyDialog(neighborTiles); }, null);

                DialogManager.PushNewDialog(d1);
            }
            else ShowRoadBuildDialog(neighborTiles);
        }

        public static void ShowRoadBuildDialog(int[] neighborTiles)
        {
            List<string> selectableTiles = new List<string>();
            foreach (int tileID in neighborTiles)
            {
                if (!CheckIfCanBuildRoadOnTile(tileID)) continue;
                else if (CheckIfTwoTilesAreConnected(ClientValues.chosenCaravan.Tile, tileID)) continue;
                else selectableTiles.Add(tileID.ToString());
            }

            Action r1 = delegate
            {
                string selectedTile = DialogManager.dialogButtonListingResultString;

                RT_Dialog_ListingWithButton d1 = new RT_Dialog_ListingWithButton("Road builder", "Select road type to use",
                    GetAvailableRoadLabels(true),
                    delegate
                    {
                        int selectedIndex = DialogManager.dialgButtonListingResultInt;

                        if (RimworldManager.CheckIfHasEnoughSilverInCaravan(ClientValues.chosenCaravan, allowedRoadCosts[selectedIndex]))
                        {
                            RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, allowedRoadCosts[selectedIndex], ClientValues.chosenCaravan);
                            RoadManager.SendRoadAddRequest(ClientValues.chosenCaravan.Tile, int.Parse(selectedTile), allowedRoadDefs[selectedIndex]);
                            SaveManager.ForceSave();
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver for this action!"));
                    });

                DialogManager.PushNewDialog(d1);
            };

            DialogManager.PushNewDialog(new RT_Dialog_ListingWithButton("Road builder", "Select a tile to connect with",
                selectableTiles.ToArray(), r1));
        }

        public static void ShowRoadDestroyDialog(int[] neighborTiles)
        {
            List<string> selectableTiles = new List<string>();
            foreach (int tileID in neighborTiles)
            {
                if (CheckIfTwoTilesAreConnected(ClientValues.chosenCaravan.Tile, tileID))
                {
                    selectableTiles.Add(tileID.ToString());
                }
            }

            Action r1 = delegate
            {
                string selectedTile = DialogManager.dialogButtonListingResultString;
                RoadManager.SendRoadRemoveRequest(ClientValues.chosenCaravan.Tile, int.Parse(selectedTile));
            };

            DialogManager.PushNewDialog(new RT_Dialog_ListingWithButton("Road destroyer", "Select a tile to disconnect from",
                selectableTiles.ToArray(), r1));
        }

        public static void ForceRoadLayerRefresh()
        {
            Find.World.renderer.SetDirty<WorldLayer_Roads>();
            Find.World.renderer.RegenerateLayersIfDirtyInLongEvent();
        }
    }
}
