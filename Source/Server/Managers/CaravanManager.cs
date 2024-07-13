using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CaravanManager
    {
        //Variables

        public readonly static string fileExtension = ".mpcaravan";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(packet.contents);

            switch (data.stepMode)
            {
                case CaravanStepMode.Add:
                    AddCaravan(client, data);
                    break;

                case CaravanStepMode.Remove:
                    RemoveCaravan(client, data);
                    break;

                case CaravanStepMode.Move:
                    MoveCaravan(client, data);
                    break;
            }
        }

        private static void AddCaravan(ServerClient client, CaravanData data)
        {
            data.details.ID = GetActiveCaravanCount() + 1;
            SaveCaravan(data.details);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            NetworkHelper.SendPacketToAllClients(packet);
        }

        private static void RemoveCaravan(ServerClient client, CaravanData data)
        {
            CaravanDetails toRemove = GetActiveCaravans().FirstOrDefault(fetch => fetch.ID == data.details.ID);
            if (toRemove == null) ResponseShortcutManager.SendIllegalPacket(client, "Tried to delete non-existing caravan");
            else
            {
                DeleteCaravan(data.details);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
                NetworkHelper.SendPacketToAllClients(packet);
            }
        }

        private static void MoveCaravan(ServerClient client, CaravanData data)
        {
            CaravanDetails toMove = GetActiveCaravans().FirstOrDefault(fetch => fetch.ID == data.details.ID);
            if (toMove == null) ResponseShortcutManager.SendIllegalPacket(client, "Tried to move non-existing caravan");
            else
            {
                UpdateCaravan(toMove, data.details);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
                NetworkHelper.SendPacketToAllClients(packet);
            }
        }

        private static void SaveCaravan(CaravanDetails details)
        {
            Serializer.SerializeToFile(Path.Combine(Master.caravansPath, details.ID + fileExtension), details);
        }

        private static void DeleteCaravan(CaravanDetails details)
        {
            File.Delete(Path.Combine(Master.caravansPath, details.ID + fileExtension));
        }

        private static void UpdateCaravan(CaravanDetails details, CaravanDetails newDetails)
        {
            details.tile = newDetails.tile;

            SaveCaravan(details);
        }

        public static CaravanDetails[] GetActiveCaravans()
        {
            List<CaravanDetails> activeCaravans = new List<CaravanDetails>();
            foreach(string str in Directory.GetFiles(Master.caravansPath))
            {
                activeCaravans.Add(Serializer.SerializeFromFile<CaravanDetails>(str));
            }

            return activeCaravans.ToArray();
        }

        private static int GetActiveCaravanCount()
        {
            return Directory.GetFiles(Master.caravansPath).Count();
        }
    }
}
