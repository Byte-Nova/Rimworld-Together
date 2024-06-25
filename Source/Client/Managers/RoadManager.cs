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

        }

        public static void SendRoadRequest()
        {
            int startingTileID = ClientValues.chosenCaravan.Tile;
            int targetTileID = int.Parse(DialogManager.dialogButtonListingResultString);

            Tile startingTile = Find.WorldGrid[startingTileID];
            Tile targetTile = Find.WorldGrid[targetTileID];

            foreach (RoadDef def in DefDatabase<RoadDef>.AllDefs) Logger.Warning(def.defName);

            AddRoadLink(startingTile, targetTileID, baseRoadDef);

            AddRoadLink(targetTile, startingTileID, baseRoadDef);

            RecalculateRoads(startingTileID);

            RecalculateRoads(targetTileID);

            Find.World.renderer.RegenerateAllLayersNow();
        }

        private static void RecalculateRoads(int toCalculateID)
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
                    //TODO
                    //THIS NEEDS TO BE CHANGED SOMEHOW
                    //neighborTile.Roads.Clear();

                    AddRoadLink(toCalculateTile, neighborID, baseRoadDef);
                    AddRoadLink(neighborTile, toCalculateID, baseRoadDef);
                }
            }
        }

        private static void AddRoadLink(Tile toAddTo, int neighborTileID, RoadDef roadDef)
        {
            Tile.RoadLink link = new Tile.RoadLink
            {
                neighbor = neighborTileID,
                road = roadDef
            };

            toAddTo.potentialRoads ??= new List<Tile.RoadLink>();
            toAddTo.potentialRoads.Add(link);
        }
    }
}
