using GameClient.Misc;
using Shared;
using Verse;
using TCPNetwork.Packets;
using Shared.Files.Maps;

namespace GameClient.Managers
{
    public static class MapManager
    {
        public static void SendPlayerMapsToServer()
        {
            foreach (Map map in Find.Maps.ToArray())
            {
                if (map.IsPlayerHome)
                {
                    SendMapToServer(map);
                }
            }
        }

        public static void SendMapToServer(Map map)
        {
            MapFile mapFile = MapSaveLoader.MapToString(map, true, true, true, true, true, true);

            MapData mapData = new MapData();
            mapData._mapTile = mapFile.Tile;
            mapData._rawData = Serializer.ConvertObjectToBytes(mapFile);

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.MapManager, mapData);
        }
    }
}
