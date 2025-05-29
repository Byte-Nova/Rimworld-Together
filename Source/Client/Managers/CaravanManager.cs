using GameClient.Managers;
using GameClient.Misc;
using GameClient.TCP;
using GameClient.Values;
using GameClient.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class CaravanManager
    {
        //Variables

        public static WorldObjectDef OnlineCaravanDef { get; set; }

        public static List<Caravan> PlayerCaravans { get; private set; } = new List<Caravan>();

        public static List<CaravanFile> GuestCaravans { get; private set; } = new List<CaravanFile>();

        [HandlesPacket(PacketHeader.CaravanManager)]
        private static void ParsePacket(byte[] bytes)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(bytes);

            switch (data._stepMode)
            {
                case CaravanStepMode.Add:
                    AddCaravan(data._caravanFile);
                    break;

                case CaravanStepMode.Remove:
                    RemoveCaravan(data._caravanFile);
                    break;

                case CaravanStepMode.Move:
                    MoveCaravan(data._caravanFile);
                    break;
            }
        }

        public static void AddCaravan(CaravanFile file)
        {
            if (!ClientValues.IsReadyToPlay) return;
            if (file.UID == ClientValues.Uid) return;

            try
            {
                if (CaravanManagerH.GetExistingCaravanFromFile(file) != null)
                {
                    Printer.Warning("Caravan to add already existed", LogImportanceMode.Verbose);
                }

                else
                {
                    GuestCaravans.Add(file);

                    OnlineCaravan onlineCaravan = (OnlineCaravan)WorldObjectMaker.MakeWorldObject(OnlineCaravanDef);
                    onlineCaravan.Tile = file.Tile;
                    onlineCaravan.SetFaction(ClientValues.NeutralPlayer);
                    Find.World.worldObjects.AllWorldObjects.Add(onlineCaravan);
                }
            }
            catch (Exception e) { Printer.Error(e); }
        }

        private static void RemoveCaravan(CaravanFile file)
        {
            if (!ClientValues.IsReadyToPlay) return;
            if (file.UID == ClientValues.Uid) return;

            try
            {
                CaravanFile toFind = CaravanManagerH.GetExistingCaravanFromFile(file);
                if (toFind == null) Printer.Warning("Caravan to remove wasn't found", LogImportanceMode.Verbose);
                else
                {
                    OnlineCaravan toRemove = CaravanManagerH.GetAllExistingOnlineCaravans()
                        .FirstOrDefault(fetch => fetch.Tile == toFind.Tile);

                    if (toRemove != null)
                    {
                        Find.World.worldObjects.AllWorldObjects.Remove(toRemove);
                        GuestCaravans.Remove(toFind);
                    }
                }
            }
            catch (Exception e) { Printer.Error(e); }
        }

        private static void MoveCaravan(CaravanFile file)
        {
            if (!ClientValues.IsReadyToPlay) return;
            if (file.UID == ClientValues.Uid) return;

            try
            {
                CaravanFile toFind = CaravanManagerH.GetExistingCaravanFromFile(file);
                if (toFind == null) AddCaravan(file);
                else
                {
                    OnlineCaravan onlineCaravan = CaravanManagerH.GetAllExistingOnlineCaravans()
                        .FirstOrDefault(fetch => fetch.Tile == toFind.Tile);

                    if (onlineCaravan != null)
                    {
                        onlineCaravan.Tile = file.Tile;
                        toFind.Tile = file.Tile;
                    }
                }
            }
            catch (Exception e) { Printer.Error(e); }
        }

        public static void RequestCaravanAdd(Caravan caravan)
        {
            PlayerCaravans.Add(caravan);

            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Add;
            data._caravanFile = new CaravanFile();
            data._caravanFile.Tile = caravan.Tile;
            data._caravanFile.UID = ClientValues.Uid;
            data._caravanFile.ID = caravan.ID;

            Network.Listener.EnqueuePacket(PacketHeader.CaravanManager, data);
        }

        public static void RequestCaravanRemove(Caravan caravan)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Remove;
            data._caravanFile = new CaravanFile();
            data._caravanFile.Tile = caravan.Tile;
            data._caravanFile.UID = ClientValues.Uid;
            data._caravanFile.ID = caravan.ID;

            PlayerCaravans.Remove(caravan);

            Network.Listener.EnqueuePacket(PacketHeader.CaravanManager, data);
        }

        public static void RequestCaravanUpdate(Caravan caravan)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Move;
            data._caravanFile = new CaravanFile();
            data._caravanFile.Tile = caravan.Tile;
            data._caravanFile.UID = ClientValues.Uid;
            data._caravanFile.ID = caravan.ID;

            Network.Listener.EnqueuePacket(PacketHeader.CaravanManager, data);
        }

        public static void ClearAllCaravans()
        {
            GuestCaravans.Clear();
            PlayerCaravans.Clear();

            foreach (WorldObject worldObject in CaravanManagerH.GetAllExistingOnlineCaravans())
            {
                Find.World.worldObjects.Remove(worldObject);
            }
        }
    }
}

public static class CaravanManagerH
{
    public static OnlineCaravan[] GetAllExistingOnlineCaravans()
    {
        List<OnlineCaravan> onlineCaravans = new List<OnlineCaravan>();
        foreach (WorldObject wo in Find.World.worldObjects.AllWorldObjects)
        {
            if (wo.def == CaravanManager.OnlineCaravanDef) onlineCaravans.Add((OnlineCaravan)wo);
        }

        return onlineCaravans.ToArray();
    }

    public static CaravanFile GetExistingCaravanFromFile(CaravanFile file)
    {
        return CaravanManager.GuestCaravans.FirstOrDefault(fetch => fetch.UID == file.UID 
            && fetch.ID == file.ID);
    }

    public static void SetCaravanDef()
    {
        CaravanManager.OnlineCaravanDef = DefDatabase<WorldObjectDef>.AllDefs.First(fetch => fetch.defName == "RTCaravan");
    }

    public static void SetAllPlayerCaravans()
    {
        Caravan[] playerCaravans = Find.World.worldObjects.Caravans.Where(fetch => fetch.Faction == Faction.OfPlayer).ToArray();
        foreach (Caravan caravan in playerCaravans) CaravanManager.PlayerCaravans.Add(caravan);
    }
}
