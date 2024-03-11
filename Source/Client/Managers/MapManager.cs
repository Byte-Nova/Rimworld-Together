using Shared;
using System.Linq;
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
            MapDetailsJSON mapDetailsJSON = ParseMap(map, true, true, true, true);

            MapFileJSON mapFileJSON = new MapFileJSON();
            mapFileJSON.mapTile = mapDetailsJSON.mapTile;
            mapFileJSON.mapData = Serializer.ConvertObjectToBytes(mapDetailsJSON);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MapPacket), mapFileJSON);
            Network.listener.EnqueuePacket(packet);
        }

        //Parses a desired map into an usable mod class

        public static MapDetailsJSON ParseMap(Map map, bool includeItems, bool includeHumans, bool includeAnimals, bool includeMods)
        {
            MapDetailsJSON mapDetailsJSON = MapScribeManager.MapToString(map, includeItems, includeHumans, includeAnimals);

            if (includeMods) mapDetailsJSON.mapMods = ModManager.GetRunningModList().ToList();

            return mapDetailsJSON;
        }
    }
}
