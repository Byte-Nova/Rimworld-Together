using GameClient.Misc;
using Shared;
using Verse;
using TCPNetwork.Packets;

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
            MapData mapData = new MapData();
            mapData._mapFile = MapSaveLoader.MapToString(map, true, true, true, true, true, true);

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.MapManager, mapData);
        }
    }
}
