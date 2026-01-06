using GameClient.WorldObjects;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient.Misc
{
    public static class Finder
    {
        public static Map GetMapFromID(int id) { return Find.Maps.FirstOrDefault(fetch => fetch.uniqueID == id); }

        public static Pawn GetPawnFromID(Map map, string id) { return GetAllPawnsInMap(map).FirstOrDefault(fetch => fetch.ThingID == id); }

        public static Thing GetThingFromID(Map map, string id) { return GetAllThingsInMap(map).FirstOrDefault(fetch => fetch.ThingID == id); }

        public static Pawn[] GetAllPawnsInMap(Map map) { return map.mapPawns.AllPawns.ToArray(); }

        public static Thing[] GetAllThingsInMap(Map map) { return map.listerThings.AllThings.ToArray(); }

        public static MentalStateDef GetMentalStateDefFromName(string defname)
        {
            return DefDatabase<MentalStateDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == defname);
        }

        public static RTSettlement GetRTSettlementFromTile(int tile) 
        { 
            return (RTSettlement)Find.World.worldObjects.AllWorldObjects.First(fetch => fetch.Tile == tile && fetch is RTSettlement); 
        }

        public static RTSite GetRTSiteFromTile(int tile)
        {
            return (RTSite)Find.World.worldObjects.AllWorldObjects.First(fetch => fetch.Tile == tile && fetch is RTSite);
        }

        public static WorldObject[] GetAllRTSettlements()
        {
            return (WorldObject[])Find.World.worldObjects.AllWorldObjects.FindAll(fetch => fetch is RTSettlement).ToArray();
        }

        public static WorldObject[] GetAllRTSites()
        {
            return (WorldObject[])Find.World.worldObjects.AllWorldObjects.FindAll(fetch => fetch is RTSite).ToArray();
        }
    }
}
