using TCPNetwork.Packets;
using TCPNetwork.Server;
using Shared;
using Shared.Files;

namespace GameServer.Managers
{
    public static class InformationManager
    {
        [HandlesPacket(PacketHeader.InformationManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            InformationData data = Serializer.ConvertBytesToObject<InformationData>(bytes);

            switch (data._stepMode)
            {
                case InformationData.InfoStepMode.Connection:
                    SendInformation(client, data);
                    break;

                case InformationData.InfoStepMode.Wealth:
                    SendWealth(client, data);
                    break;
            }
        }

        private static void SendInformation(ServerClient client, InformationData data)
        {
            SettlementFile settlementToFind = SettlementManager.GetSettlementFileFromTile(data._settlementTile);
            ServerClient clientToFind = ServerNetwork.Instance.GetConnectedClientFromUid(settlementToFind.UID);

            data._isPlayerOnline = clientToFind != null ? true : false;

            client.Listener.EnqueuePacket(PacketHeader.InformationManager, data);
        }

        private static void SendWealth(ServerClient client, InformationData data)
        {
            MapFile mapToFind = MapManager.GetMapFromTile(data._settlementTile);

            data._settlementWealth = mapToFind != null ? mapToFind.Wealth : -1;

            client.Listener.EnqueuePacket(PacketHeader.InformationManager, data);
        }
    }
}
