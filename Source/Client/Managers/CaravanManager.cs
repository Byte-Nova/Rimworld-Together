using GameClient;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class CaravanManager
    {
        //Variables

        public static WorldObjectDef onlineCaravanDef;

        public static List<CaravanFile> activeCaravans = new List<CaravanFile>();

        public static Dictionary<Caravan, int> activePlayerCaravans = new Dictionary<Caravan, int>();

        public static void ParsePacket(Packet packet)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(packet.contents);

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

        public static void AddCaravans(CaravanFile[] details)
        {
            if (details == null) return;

            foreach (CaravanFile caravan in details)
            {
                AddCaravan(caravan);
            }
        }

        private static void AddCaravan(CaravanFile details)
        {
            activeCaravans.Add(details);

            if (details.Owner == ClientValues.username)
            {
                Caravan toAdd = Find.WorldObjects.Caravans.FirstOrDefault(fetch => fetch.Faction == Faction.OfPlayer && 
                    !activePlayerCaravans.ContainsKey(fetch));

                if (toAdd == null) return;
                else activePlayerCaravans.Add(toAdd, details.ID);
            }

            else
            {
                OnlineCaravan onlineCaravan = (OnlineCaravan)WorldObjectMaker.MakeWorldObject(onlineCaravanDef);
                onlineCaravan.Tile = details.Tile;
                onlineCaravan.SetFaction(FactionValues.neutralPlayer);
                Find.World.worldObjects.Add(onlineCaravan);
            }
        }

        private static void RemoveCaravan(CaravanFile details)
        {
            CaravanFile toRemove = CaravanManagerHelper.GetCaravanDetailsFromID(details.ID);
            if (toRemove == null) return;
            else
            {
                activeCaravans.Remove(toRemove);

                if (details.Owner == ClientValues.username)
                {
                    foreach (KeyValuePair<Caravan, int> pair in activePlayerCaravans.ToArray())
                    {
                        if (pair.Value == details.ID)
                        {
                            activePlayerCaravans.Remove(pair.Key);
                            break;
                        }
                    }
                }

                else
                {
                    WorldObject worldObject = Find.World.worldObjects.AllWorldObjects.First(fetch => fetch.Tile == details.Tile 
                        && fetch.def == onlineCaravanDef);

                    Find.World.worldObjects.Remove(worldObject);
                }
            }
        }

        private static void MoveCaravan(CaravanFile details)
        {
            CaravanFile toMove = CaravanManagerHelper.GetCaravanDetailsFromID(details.ID);
            if (toMove == null) return;
            else
            {
                if (details.Owner == ClientValues.username) return;
                else
                {
                    RemoveCaravan(toMove);
                    AddCaravan(details);
                }
            }
        }

        public static void RequestCaravanAdd(Caravan caravan)
        {
            CaravanData data = new CaravanData();
            data._stepMode = CaravanStepMode.Add;
            data._caravanFile = new CaravanFile();
            data._caravanFile.Tile = caravan.Tile;
            data._caravanFile.Owner = ClientValues.username;

            Packet packet = Packet.CreatePacketFromObject(nameof(CaravanManager), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void RequestCaravanRemove(Caravan caravan)
        {
            activePlayerCaravans.TryGetValue(caravan, out int caravanID);

            CaravanFile details = CaravanManagerHelper.GetCaravanDetailsFromID(caravanID);
            if (details == null) return;
            else
            {
                CaravanData data = new CaravanData();
                data._stepMode = CaravanStepMode.Remove;
                data._caravanFile = details;

                Packet packet = Packet.CreatePacketFromObject(nameof(CaravanManager), data);
                Network.listener.EnqueuePacket(packet);
            }
        }

        public static void RequestCaravanMove(Caravan caravan)
        {
            activePlayerCaravans.TryGetValue(caravan, out int caravanID);

            CaravanFile details = CaravanManagerHelper.GetCaravanDetailsFromID(caravanID);
            if (details == null) return;
            else
            {
                CaravanData data = new CaravanData();
                data._stepMode = CaravanStepMode.Move;
                data._caravanFile = details;

                Packet packet = Packet.CreatePacketFromObject(nameof(CaravanManager), data);
                Network.listener.EnqueuePacket(packet);
            }
        }

        public static void ClearAllCaravans()
        {            
            activeCaravans.Clear();
            activePlayerCaravans.Clear();

            foreach (WorldObject worldObject in Find.World.worldObjects.AllWorldObjects.ToArray())
            {
                if (worldObject.def == onlineCaravanDef)
                {
                    Find.World.worldObjects.Remove(worldObject);
                }
            }
        }

        public static void ModifyDetailsTile(Caravan caravan, int updatedTile)
        {
            activePlayerCaravans.TryGetValue(caravan, out int caravanID);

            foreach (CaravanFile details in activeCaravans)
            {
                if (details.ID == caravanID)
                {
                    details.Tile = updatedTile;
                    break;
                }
            }
        }
    }
}

public static class CaravanManagerHelper
{
    //Variables

    public static CaravanFile[] tempCaravanDetails;

    public static void SetValues(ServerGlobalData serverGlobalData)
    {
        tempCaravanDetails = serverGlobalData._playerCaravans;
    }

    public static CaravanFile GetCaravanDetailsFromID(int id)
    {
        return CaravanManager.activeCaravans.FirstOrDefault(fetch => fetch.ID == id);
    }

    public static void SetCaravanDefs()
    {
        CaravanManager.onlineCaravanDef = DefDatabase<WorldObjectDef>.AllDefs.First(fetch => fetch.defName == "RTCaravan");
    }
}
