using RimWorld;
using RimWorld.Planet;
using Shared;
using System.Linq;
using Verse;

namespace GameClient
{
    public static class CaravanManager
    {
        public static void ParsePacket(Packet packet)
        {
            CaravanData data = Serializer.ConvertBytesToObject<CaravanData>(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.CaravanStepMode.Add:
                    AddCaravan(data);
                    break;

                case CommonEnumerators.CaravanStepMode.Remove:
                    RemoveCaravan(data);
                    break;
            }
        }

        //TODO
        //MAKE THIS WORK

        private static void AddCaravan(CaravanData data)
        {
            WorldObject toAdd = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.RoutePlannerWaypoint);
            toAdd.Tile = data.details.tile;
            Find.World.worldObjects.Add(toAdd);
        }

        //TODO
        //MAKE THIS WORK

        private static void RemoveCaravan(CaravanData data)
        {
            WorldObject toRemove = Find.World.worldObjects.AllWorldObjects.First(fetch => fetch.Tile == data.details.tile);
            Find.World.worldObjects.Remove(toRemove);
        }

        public static void RequestCaravanAdd(Caravan caravan)
        {
            CaravanData data = new CaravanData();
            data.stepMode = CommonEnumerators.CaravanStepMode.Add;
            data.details = new CaravanDetails();
            data.details.tile = caravan.Tile;
            data.details.localID = caravan.ID;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void RequestCaravanRemove(Caravan caravan)
        {
            CaravanData data = new CaravanData();
            data.stepMode = CommonEnumerators.CaravanStepMode.Remove;
            data.details = new CaravanDetails();
            data.details.tile = caravan.Tile;
            data.details.localID = caravan.ID;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CaravanPacket), data);
            Network.listener.EnqueuePacket(packet);
        }
    }
}
