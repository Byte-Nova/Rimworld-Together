using Shared;
using Verse;

namespace GameClient
{
    //Class that handles map functions for the mod to use

    public static class MapManager
    {
        //Sends all the player maps to the server

        public static void SendPlayerMapsToServer()
        {
            foreach (Map map in Find.Maps.ToArray())
            {
                if (map.IsPlayerHome)
                {
                    SendMapToServerSingle(map);
                }
            }
        }

        //Sends a desired map to the server

        private static void SendMapToServerSingle(Map map)
        {
            MapData mapData = new MapData();
            mapData._mapFile = ParseMap(map, true, true, true, true);

            Packet packet = Packet.CreatePacketFromObject(nameof(MapManager), mapData);
            Network.listener.EnqueuePacket(packet);
        }

        //Parses a desired map into an usable mod class

        public static MapFile ParseMap(Map map, bool includeThings, bool includeHumans, bool includeAnimals, bool includeMods)
        {
            MapFile mapFile = MapScribeManager.MapToString(map, includeThings, includeThings, includeHumans, includeHumans, includeAnimals, includeAnimals);

            if (includeMods) mapFile.Mods = ModManagerHelper.GetRunningModList();

            return mapFile;
        }
    }
}
