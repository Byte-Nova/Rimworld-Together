using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class CaravanManager
    {
        //Variables

        public static readonly string fileExtension = ".mpcaravan";

        [HandlesPacket(PacketHeader.CaravanManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

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

            NetworkHelper.SendPacketToAllClients(PacketHeader.CaravanManager, data);

            InformationDisplayer.DisplayAddCaravan(client.UserFile.Uid);
        }

        public static void RemoveCaravan(ServerClient client, CaravanFile file)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Remove;
            data._caravanFile = file;

            NetworkHelper.SendPacketToAllClients(PacketHeader.CaravanManager, data);

            InformationDisplayer.DisplayRemoveCaravan(client.UserFile.Uid);
        }

        private static void MoveCaravan(ServerClient client, CaravanFile file)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Move;
            data._caravanFile = file;

            NetworkHelper.SendPacketToAllClients(PacketHeader.CaravanManager, data, client);

            InformationDisplayer.DisplayMoveCaravan(client.UserFile.Uid);
        }
    }
}
