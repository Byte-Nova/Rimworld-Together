using GameClient;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using Verse;

namespace GameClient
{
    public static class CaravanManager
    {
        //Variables

        public static WorldObjectDef onlineCaravanDef;
        public static List<CaravanDetails> activeCaravans = new List<CaravanDetails>();

        public static void ParsePacket(Packet packet)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.CaravanStepMode.Add:
                    AddCaravan(data.details);
                    break;

                case CommonEnumerators.CaravanStepMode.Remove:
                    RemoveCaravan(data.details);
                    break;

                case CommonEnumerators.CaravanStepMode.Move:
                    MoveCaravan(data.details);
                    break;
            }
        }

        public static void AddCaravans(CaravanDetails[] details)
        {
            foreach(CaravanDetails caravan in details)
            {
                AddCaravan(caravan);
            }
        }

        private static void AddCaravan(CaravanDetails details)
        {
            activeCaravans.Add(details);

            //Make sure we don't accept server orders from our caravans
            if (details.owner == ClientValues.username) return;
            else
            {
                OnlineCaravan onlineCaravan = (OnlineCaravan)WorldObjectMaker.MakeWorldObject(onlineCaravanDef);
                onlineCaravan.Tile = details.tile;
                onlineCaravan.SetFaction(FactionValues.neutralPlayer);
                Find.World.worldObjects.Add(onlineCaravan);
            }
        }

        private static void RemoveCaravan(CaravanDetails details)
        {
            CaravanDetails toRemove = CaravanManagerHelper.GetCaravanDetailsFromID(details.ID);
            if (toRemove == null) return;
            else
            {
                activeCaravans.Remove(toRemove);

                //Make sure we don't accept server orders from our caravans
                if (details.owner == ClientValues.username) return;
                else
                {
                    WorldObject worldObject = Find.World.worldObjects.AllWorldObjects.First(fetch => fetch.Tile == details.tile 
                        && fetch.def == onlineCaravanDef);

                    Find.World.worldObjects.Remove(worldObject);
                }
            }
        }

        private static void MoveCaravan(CaravanDetails details)
        {
            CaravanDetails toMove = CaravanManagerHelper.GetCaravanDetailsFromID(details.ID);
            if (toMove == null) return;
            else
            {
                if (details.owner == ClientValues.username) return;
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
            data.stepMode = CommonEnumerators.CaravanStepMode.Add;
            data.details = new CaravanDetails();
            data.details.tile = caravan.Tile;
            data.details.owner = ClientValues.username;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void RequestCaravanRemove(Caravan caravan)
        {
            CaravanData data = new CaravanData();
            data.stepMode = CommonEnumerators.CaravanStepMode.Remove;
            data.details = CaravanManagerHelper.GetCaravanDetailsFromTile(caravan.Tile);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void RequestCaravanMove(Caravan caravan)
        {
            CaravanData data = new CaravanData();
            data.stepMode = CommonEnumerators.CaravanStepMode.Move;
            data.details = CaravanManagerHelper.GetCaravanDetailsFromTile(caravan.pather.nextTile);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void ClearAllCaravans()
        {
            activeCaravans.Clear();

            foreach(WorldObject worldObject in Find.World.worldObjects.AllWorldObjects.ToArray())
            {
                if (worldObject.def == onlineCaravanDef)
                {
                    Find.World.worldObjects.Remove(worldObject);
                }
            }
        }

        public static void ModifyDetailsTile(Caravan caravan)
        {
            foreach (CaravanDetails details in activeCaravans)
            {
                if (details.owner == ClientValues.username && details.tile == caravan.Tile)
                {
                    details.tile = caravan.pather.nextTile;
                    Logger.Warning($"Caravan moved to {details.tile}");
                    break;
                }
            }
        }
    }
}

public static class CaravanManagerHelper
{
    //Variables

    public static CaravanDetails[] tempCaravanDetails;

    public static void SetCaravanValues(ServerGlobalData serverGlobalData)
    {
        tempCaravanDetails = serverGlobalData.playerCaravans;
    }

    public static CaravanDetails GetCaravanDetailsFromTile(int tile)
    {
        return CaravanManager.activeCaravans.First(fetch => fetch.tile == tile);
    }

    public static CaravanDetails GetCaravanDetailsFromID(int id)
    {
        return CaravanManager.activeCaravans.FirstOrDefault(fetch => fetch.ID == id);
    }

    public static void SetCaravanDefs()
    {
        CaravanManager.onlineCaravanDef = DefDatabase<WorldObjectDef>.AllDefs.First(fetch => fetch.defName == "RTCaravan");
    }
}
