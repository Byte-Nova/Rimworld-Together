using TCPNetwork.Packets;
using Shared;
using Shared.Files;
using TCPNetwork.Files.Client;
using Shared.Files.Maps;

namespace GameServer.Managers
{
    public static class InformationManager
    {
        [HandlesPacket(PacketHeader.InformationManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
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
            ServerClient clientToFind = ServerNetwork.Instance.GetConnectedClientFromUsername(settlementToFind.Username);

            data._isPlayerOnline = clientToFind != null ? true : false;

            client.Listener.EnqueuePacket(PacketHeader.InformationManager, data);
        }

        private static void SendWealth(ServerClient client, InformationData data)
        {
            data._settlementRawData = MapManager.GetMapFromTile(data._settlementTile);

            client.Listener.EnqueuePacket(PacketHeader.InformationManager, data);
        }
    }
}
