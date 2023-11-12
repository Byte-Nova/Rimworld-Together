using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using Verse;
using Verse.Noise;

namespace RimworldTogether.GameClient.Managers
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
            Packet packet = Packet.CreatePacketFromJSON("MapPacket", mapDetailsJSON);
            Network.Network.serverListener.SendData(packet);
        }
    }
}
