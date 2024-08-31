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
            MapData mapData = ParseMap(map, true, true, true, true);
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MapPacket), mapData);
            Network.listener.EnqueuePacket(packet);
        }

        //Parses a desired map into an usable mod class

        public static MapData ParseMap(Map map, bool includeThings, bool includeHumans, bool includeAnimals, bool includeMods)
        {
            MapData mapData = MapScribeManager.MapToString(map, includeThings, includeThings, includeHumans, includeHumans, includeAnimals, includeAnimals);

            if (includeMods) mapData._mapMods = ModManager.GetRunningModList();

            return mapData;
        }
    }
}
