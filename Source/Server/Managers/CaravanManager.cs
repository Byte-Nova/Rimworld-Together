using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
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
                    RemoveCaravan(client.userFile.Uid, data._caravanFile);
                    break;

                case CaravanStepMode.Move:
                    MoveCaravan(client, data._caravanFile);
                    break;
            }
        }

        private static void AddCaravan(ServerClient client, CaravanData data)
        {
            data._caravanFile.UID = client.userFile.Uid;
            data._caravanFile.ID = CaravanManagerHelper.GetNewCaravanID();
            RefreshCaravanTimer(data._caravanFile);

            Packet packet = Packet.CreatePacketFromObject(nameof(CaravanManager), data);
            NetworkHelper.SendPacketToAllClients(packet);

            InformationDisplayer.DisplayAddCaravan(data._caravanFile.Tile.ToString());
        }

        public static void RemoveCaravan(string uid, CaravanFile file)
        {
            CaravanFile toRemove = CaravanManagerHelper.GetCaravanFromID(uid, file.ID);
            if (toRemove == null) return;
            else
            {
                DeleteCaravan(file);

                CaravanData data = new CaravanData();
                data._stepMode = CaravanStepMode.Remove;
                data._caravanFile = file;

                Packet packet = Packet.CreatePacketFromObject(nameof(CaravanManager), data);
                NetworkHelper.SendPacketToAllClients(packet);

                InformationDisplayer.DisplayRemoveCaravan(file.Tile.ToString());
            }
        }

        private static void MoveCaravan(ServerClient client, CaravanFile file)
        {
            CaravanFile existingCaravan = CaravanManagerHelper.GetCaravanFromID(client.userFile.Uid, file.ID);
            if (existingCaravan == null) return;
            else
            {
                UpdateCaravan(existingCaravan, file);
                RefreshCaravanTimer(file);

                CaravanData data = new CaravanData();
                data._stepMode = CaravanStepMode.Move;
                data._caravanFile = file;

                Packet packet = Packet.CreatePacketFromObject(nameof(CaravanManager), data);
                NetworkHelper.SendPacketToAllClients(packet, client);

                InformationDisplayer.DisplayMoveCaravan(file.Tile.ToString());
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
            details.TimeSinceRefresh = TimeConverter.GetCurrentTimeToEpoch();

            SaveCaravan(details);
        }

        public static async Task StartCaravanTicker()
        {
            while (true)
            {
                try { IdleCaravanTick(); }
                catch (Exception e) { Printer.Error($"Caravan tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMilliseconds(taskDelayMS));
            }
        }

        private static void IdleCaravanTick()
        {
            foreach (CaravanFile caravans in CaravanManagerHelper.GetActiveCaravans())
            {
                if (TimeConverter.CheckForEpochTimer(caravans.TimeSinceRefresh, baseMaxTimer))
                {
                    DeleteCaravan(caravans);

                    CaravanData data = new CaravanData();
                    data._stepMode = CaravanStepMode.Remove;
                    data._caravanFile = caravans;

                    Packet packet = Packet.CreatePacketFromObject(nameof(CaravanManager), data);
                    NetworkHelper.SendPacketToAllClients(packet);
                }
            }

            InformationDisplayer.DisplayCaravanTick();
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

        public static CaravanFile GetCaravanFromID(string uid, int caravanID)
        {
            return GetActiveCaravans().FirstOrDefault(fetch => fetch.ID == caravanID &&
                fetch.UID == uid);
        }

        public static CaravanFile[] GetCaravansFromUID(string uid)
        {
            CaravanFile[] toGet = GetActiveCaravans().Where(fetch => fetch.UID == uid).ToArray();

            if (toGet == null) return null;
            else return toGet;
        }

        public static int GetNewCaravanID()
        {
            int maxID = 0;
            foreach (CaravanFile caravans in GetActiveCaravans())
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
