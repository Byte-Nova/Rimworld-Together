using Shared;

namespace GameServer
{
    public static class CaravanManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.CaravanStepMode.Add:
                    AddCaravan(client, data);
                    break;

                case CommonEnumerators.CaravanStepMode.Remove:
                    RemoveCaravan(client, data);
                    break;
            }
        }

        //TODO
        //MAKE THIS WORK

        private static void AddCaravan(ServerClient client, CaravanData data)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            NetworkHelper.SendPacketToAllClients(packet, client);
        }

        //TODO
        //MAKE THIS WORK

        private static void RemoveCaravan(ServerClient client, CaravanData data)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            NetworkHelper.SendPacketToAllClients(packet, client);
        }
    }
}
