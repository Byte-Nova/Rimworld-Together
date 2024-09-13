using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CaravanManager
    {
        //Variables

        public static readonly string fileExtension = ".mpcaravan";

        private static readonly double baseMaxTimer = 86400000;

        private static readonly double taskDelayMS = 1800000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(packet.contents);

            switch (data._stepMode)
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
            data._caravanFile.ID = CaravanManagerHelper.GetNewCaravanID();
            RefreshCaravanTimer(data._caravanFile);

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
            NetworkHelper.SendPacketToAllClients(packet);

            Logger.Message($"[Add Caravan] > {data._caravanFile.ID} > {client.userFile.Username}");
        }

        private static void RemoveCaravan(ServerClient client, CaravanData data)
        {
            CaravanFile toRemove = CaravanManagerHelper.GetCaravanFromID(client, data._caravanFile.ID);
            if (toRemove == null) return;
            else
            {
                DeleteCaravan(data._caravanFile);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
                NetworkHelper.SendPacketToAllClients(packet);

                Logger.Message($"[Remove Caravan] > {data._caravanFile.ID} > {client.userFile.Username}");
            }
        }

        private static void MoveCaravan(ServerClient client, CaravanData data)
        {
            CaravanFile toMove = CaravanManagerHelper.GetCaravanFromID(client, data._caravanFile.ID);
            if (toMove == null) return;
            else
            {
                UpdateCaravan(toMove, data._caravanFile);
                RefreshCaravanTimer(data._caravanFile);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
                NetworkHelper.SendPacketToAllClients(packet, client);
            }
        }

        private static void SaveCaravan(CaravanFile details)
        {
            Serializer.SerializeToFile(Path.Combine(Master.caravansPath, details.ID + fileExtension), details);
        }

        private static void DeleteCaravan(CaravanFile details)
        {
            File.Delete(Path.Combine(Master.caravansPath, details.ID + fileExtension));
        }

        private static void UpdateCaravan(CaravanFile details, CaravanFile newDetails)
        {
            details.Tile = newDetails.Tile;
        }

        private static void RefreshCaravanTimer(CaravanFile details)
        {
            details.TimeSinceRefresh = TimeConverter.CurrentTimeToEpoch();

            SaveCaravan(details);
        }

        public static async Task StartCaravanTicker()
        {
            while (true)
            {
                try { IdleCaravanTick(); }
                catch (Exception e) { Logger.Error($"Caravan tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMilliseconds(taskDelayMS));
            }
        }

        private static void IdleCaravanTick()
        {
            foreach(CaravanFile caravans in CaravanManagerHelper.GetActiveCaravans())
            {
                if (TimeConverter.CheckForEpochTimer(caravans.TimeSinceRefresh, baseMaxTimer))
                {
                    DeleteCaravan(caravans);

                    CaravanData data = new CaravanData();
                    data._stepMode = CaravanStepMode.Remove;
                    data._caravanFile = caravans;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CaravanPacket), data);
                    NetworkHelper.SendPacketToAllClients(packet);
                }
            }

            Logger.Warning($"[Caravan tick]");
        }
    }

    public static class CaravanManagerHelper
    {
        public static CaravanFile[] GetActiveCaravans()
        {
            List<CaravanFile> activeCaravans = new List<CaravanFile>();
            foreach (string str in Directory.GetFiles(Master.caravansPath))
            {
                activeCaravans.Add(Serializer.SerializeFromFile<CaravanFile>(str));
            }

            return activeCaravans.ToArray();
        }

        public static CaravanFile GetCaravanFromID(ServerClient client, int caravanID)
        {
            CaravanFile toGet = GetActiveCaravans().FirstOrDefault(fetch => fetch.ID == caravanID &&
                fetch.Owner == client.userFile.Username);

            if (toGet == null) return null;
            else return toGet;
        }

        public static CaravanFile[] GetCaravansFromOwner(string userName)
        {
            CaravanFile[] toGet = GetActiveCaravans().Where(fetch => fetch.Owner == userName).ToArray();

            if (toGet == null) return null;
            else return toGet;
        }

        public static CaravanFile GetCaravanFromOwner(string userName)
        {
            CaravanFile toGet = GetActiveCaravans().FirstOrDefault(fetch => fetch.Owner == userName);

            if (toGet == null) return null;
            else return toGet;
        }

        public static int GetNewCaravanID()
        {
            int maxID = 0;
            foreach(CaravanFile caravans in GetActiveCaravans())
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
