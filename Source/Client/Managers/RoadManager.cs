using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace GameClient
{
    public static class RoadManager
    {
        private static RoadDef baseRoadDef => DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == "DirtRoad");

        public static void ParsePacket(Packet packet)
        {
            RoadData data = (RoadData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.RoadStepMode.Add:
                    AddRoadSimple(data.details);
                    break;

                case CommonEnumerators.RoadStepMode.Remove:
                    RemoveRoad(data.details);
                    break;
            }
        }

        public static void SendRoadAddRequest()
        {
            RoadData data = new RoadData();
            data.stepMode = CommonEnumerators.RoadStepMode.Add;

            data.details = new RoadDetails();
            data.details.tileA = ClientValues.chosenCaravan.Tile;
            data.details.tileB = int.Parse(DialogManager.dialogButtonListingResultString);
            data.details.roadDefName = baseRoadDef.defName;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RoadPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void AddRoads(RoadDetails[] roads, bool forceRefresh = true)
        {
            foreach (RoadDetails details in roads) AddRoadSimple(details, forceRefresh);

            //If we don't want to force refresh we wait for all the roads and then refresh the layer

            if (!forceRefresh) RoadManagerHelper.ForceRoadLayerRefresh();
        }

        public static void AddRoadSimple(RoadDetails details, bool forceRefresh = true)
        {
            if (!RoadManagerHelper.CheckIfCanBuildRoadOnTile(details.tileB))
            {
                Logger.Warning($"Tried building a road at '{details.tileB}' when it's not possible");
                return;
            }

            Tile sourceTile = Find.WorldGrid[details.tileA];
            Tile targetTile = Find.WorldGrid[details.tileB];

            RoadDef roadDef = DefDatabase<RoadDef>.AllDefs.First(fetch => fetch.defName == details.roadDefName);

            AddRoadLink(sourceTile, details.tileB, roadDef);
            AddRoadLink(targetTile, details.tileA, roadDef);

            RecalculateRoads(details.tileA, roadDef);
            RecalculateRoads(details.tileB, roadDef);

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

        private static void RecalculateRoads(int toCalculateID, RoadDef replacementDef)
        {
            Tile toCalculateTile = Find.WorldGrid[toCalculateID];

            List<int> neighbors = new List<int>();
            Find.WorldGrid.GetTileNeighbors(toCalculateID, neighbors);
            foreach (int neighborID in neighbors)
            {
                Tile neighborTile = Find.WorldGrid[neighborID];

                if (neighborTile.Roads == null) continue;
                else
                {
                    AddRoadLink(toCalculateTile, neighborID, replacementDef);
                    AddRoadLink(neighborTile, toCalculateID, replacementDef);
                }
            }
        }

        private static void RemoveRoad(RoadDetails details, bool forceRefresh = true)
        {
            Tile tileA = Find.WorldGrid[details.tileA];
            Tile tileB = Find.WorldGrid[details.tileB];

            foreach (Tile.RoadLink roadLink in tileA.Roads.ToList())
            {
                if (roadLink.neighbor == details.tileB)
                {
                    tileA.Roads.Remove(roadLink);
                }
            }

            foreach (Tile.RoadLink roadLink in tileB.Roads.ToList())
            {
                if (roadLink.neighbor == details.tileA)
                {
                    tileB.Roads.Remove(roadLink);
                }
            }

            if (forceRefresh) RoadManagerHelper.ForceRoadLayerRefresh();
        }
    }

    public static class RoadManagerHelper
    {
        public static RoadDetails[] tempRoadDetails;

        public static void SetTempRoadValues(ServerGlobalData serverGlobalData) { tempRoadDetails = serverGlobalData.roads; }

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

        public static void ForceRoadLayerRefresh()
        {
            Find.World.renderer.SetDirty<WorldLayer_Roads>();
            Find.World.renderer.RegenerateLayersIfDirtyInLongEvent();
        }
    }
}
