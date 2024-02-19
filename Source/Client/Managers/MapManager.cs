using Shared;
using Verse;

namespace GameClient
{
    public static class MapManager
    {
        public static void SendMapsToServer()
        {
            foreach (Map map in Find.Maps.ToArray())
            {
                if (map.IsPlayerHome)
                {
                    SendMapToServerSingle(map);
                }
            }
        }

        private static void SendMapToServerSingle(Map map)
        {
            MapDetailsJSON mapDetailsJSON = RimworldManager.GetMap(map, true, true, true, true);

            MapFileJSON mapFileJSON = new MapFileJSON();
            mapFileJSON.mapTile = mapDetailsJSON.mapTile;
            mapFileJSON.mapData = ObjectConverter.ConvertObjectToBytes(mapDetailsJSON);

            Packet packet = Packet.CreatePacketFromJSON("MapPacket", mapFileJSON);
            Network.listener.dataQueue.Enqueue(packet);
        }
    }
}
