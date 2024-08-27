using RimWorld;
using RimWorld.Planet;
using Shared;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GameClient
{
    public static class RiverManager
    {
        public static void SetPlanetRivers() 
        {
            if (PlanetGeneratorManager.cachedWorldValues.Rivers == null) return;
            else AddRivers(PlanetGeneratorManager.cachedWorldValues.Rivers, false);
        }

        public static void AddRivers(RiverDetails[] rivers, bool forceRefresh)
        {
            if (rivers == null) return;

            foreach (RiverDetails details in rivers)
            {
                RiverDef riverDef = DefDatabase<RiverDef>.AllDefs.First(fetch => fetch.defName == details.riverDefName);

                AddRiverSimple(details.fromTile, details.toTile, riverDef, forceRefresh);
            }

            //If we don't want to force refresh we wait for all and then refresh the layer

            if (!forceRefresh) RiverManagerHelper.ForceRiverLayerRefresh();
        }

        public static void AddRiverSimple(int tileAID, int tileBID, RiverDef riverDef, bool forceRefresh)
        {
            Tile tileA = Find.WorldGrid[tileAID];
            Tile tileB = Find.WorldGrid[tileBID];

            AddRiverLink(tileA, tileBID, riverDef);
            AddRiverLink(tileB, tileAID, riverDef);

            if (forceRefresh) RiverManagerHelper.ForceRiverLayerRefresh();
        }

        private static void AddRiverLink(Tile toAddTo, int neighborTileID, RiverDef riverDef)
        {
            if (toAddTo.Rivers != null)
            {
                foreach (Tile.RiverLink link in toAddTo.Rivers)
                {
                    if (link.neighbor == neighborTileID) return;
                }
            }

            Tile.RiverLink linkToAdd = new Tile.RiverLink
            {
                neighbor = neighborTileID,
                river = riverDef
            };

            toAddTo.potentialRivers ??= new List<Tile.RiverLink>();
            toAddTo.potentialRivers.Add(linkToAdd);
        }
    }

    public static class RiverManagerHelper
    {
        public static RiverDetails[] GetPlanetRivers()
        {
            List<RiverDetails> toGet = new List<RiverDetails>();

            foreach (Tile tile in Find.WorldGrid.tiles)
            {
                if (tile.Rivers != null)
                {
                    foreach (Tile.RiverLink link in tile.Rivers)
                    {
                        RiverDetails details = new RiverDetails();
                        details.fromTile = Find.WorldGrid.tiles.IndexOf(tile);
                        details.toTile = link.neighbor;
                        details.riverDefName = link.river.defName;

                        if (!CheckIfExists(details.fromTile, details.toTile)) toGet.Add(details);
                    }
                }
            }
            return toGet.ToArray();

            bool CheckIfExists(int tileA, int tileB)
            {
                foreach (RiverDetails details in toGet)
                {
                    if (details.fromTile == tileA && details.toTile == tileB) return true;
                    else if (details.fromTile == tileB && details.toTile == tileA) return true;
                }

                return false;
            }
        }

        public static void ForceRiverLayerRefresh()
        {
            Find.World.renderer.SetDirty<WorldLayer_Rivers>();
            Find.World.renderer.RegenerateLayersIfDirtyInLongEvent();
        }
    }
}
