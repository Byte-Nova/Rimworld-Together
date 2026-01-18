using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Files.Client;
using GameServer.Hooks.TCPNetwork;

namespace GameServer.Managers
{
    public static class CaravanManager
    {
        [HandlesPacket(PacketHeader.CaravanManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(bytes);

            switch (data._stepMode)
            {
                case CaravanStepMode.Add:
                    AddCaravan(client, data._caravanFile);
                    break;

                case CaravanStepMode.Remove:
                    RemoveCaravan(client, data._caravanFile);
                    break;

                case CaravanStepMode.Move:
                    MoveCaravan(client, data._caravanFile);
                    break;
            }
        }

        private static void AddCaravan(ServerClient client, CaravanFile file)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Add;
            data._caravanFile = file;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.CaravanManager, data, client);

            InformationDisplayer.DisplayAddCaravan(client.UserFile.Username);
        }

        public static void RemoveCaravan(ServerClient client, CaravanFile file)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Remove;
            data._caravanFile = file;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.CaravanManager, data, client);

            InformationDisplayer.DisplayRemoveCaravan(client.UserFile.Username);
        }

        private static void MoveCaravan(ServerClient client, CaravanFile file)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Move;
            data._caravanFile = file;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.CaravanManager, data, client);

            InformationDisplayer.DisplayMoveCaravan(client.UserFile.Username);
        }
    }
}
