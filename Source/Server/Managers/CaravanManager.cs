using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CaravanManager
    {
        //Variables

        private static readonly string fileExtension = ".mpcaravan";

        private static readonly double baseMaxTimer = 172800000;

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
            data.details.ID = GetNewCaravanID();
            RefreshCaravanTimer(data.details);

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
            NetworkHelper.SendPacketToAllClients(packet);

            Logger.Message($"[Add Caravan] > {data.details.ID} > {client.userFile.Username}");
        }

        private static void RemoveCaravan(ServerClient client, CaravanData data)
        {
            CaravanDetails toRemove = GetCaravanFromID(client, data.details.ID);
            if (toRemove == null) return;
            else
            {
                DeleteCaravan(data.details);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
                NetworkHelper.SendPacketToAllClients(packet);

                Logger.Message($"[Remove Caravan] > {data.details.ID} > {client.userFile.Username}");
            }
        }

        private static void MoveCaravan(ServerClient client, CaravanData data)
        {
            CaravanDetails toMove = GetCaravanFromID(client, data.details.ID);
            if (toMove == null) return;
            else
            {
                UpdateCaravan(toMove, data.details);
                RefreshCaravanTimer(data.details);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
                NetworkHelper.SendPacketToAllClients(packet, client);
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
        }

        private static void RefreshCaravanTimer(CaravanDetails details)
        {
            details.timeSinceRefresh = TimeConverter.CurrentTimeToEpoch();

            SaveCaravan(details);
        }

        public static void StartCaravanTicker()
        {
            while (true)
            {
                Thread.Sleep(1800000);

                try { IdleCaravanTick(); }
                catch (Exception e) { Logger.Error($"Caravan tick failed, this should never happen. Exception > {e}"); }
            }
        }

        private static void IdleCaravanTick()
        {
            foreach(CaravanDetails caravans in GetActiveCaravans())
            {
                if (TimeConverter.CheckForEpochTimer(caravans.timeSinceRefresh, baseMaxTimer))
                {
                    DeleteCaravan(caravans);

                    CaravanData data = new CaravanData();
                    data.stepMode = CaravanStepMode.Remove;
                    data.details = caravans;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
                    NetworkHelper.SendPacketToAllClients(packet);
                }
            }

            Logger.Message($"[Caravan tick]");
        }

        public static CaravanDetails[] GetActiveCaravans()
        {
            List<CaravanDetails> activeCaravans = new List<CaravanDetails>();
            foreach (string str in Directory.GetFiles(Master.caravansPath))
            {
                activeCaravans.Add(Serializer.SerializeFromFile<CaravanDetails>(str));
            }

            return activeCaravans.ToArray();
        }

        public static CaravanDetails GetCaravanFromID(ServerClient client, int caravanID)
        {
            CaravanDetails toGet = GetActiveCaravans().FirstOrDefault(fetch => fetch.ID == caravanID &&
                fetch.owner == client.userFile.Username);

            if (toGet == null) return null;
            else return toGet;
        }

        private static int GetNewCaravanID()
        {
            int maxID = 0;
            foreach(CaravanDetails caravans in GetActiveCaravans())
            {
                if (caravans.ID >= maxID)
                {
                    maxID = caravans.ID + 1;
                }
            }

            return maxID;
        }
    }
}
