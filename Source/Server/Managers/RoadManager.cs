using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameServer
{
    public static class RoadManager
    {
        public readonly static string fileExtension = ".mproad";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            RoadData data = (RoadData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.RoadStepMode.Add:
                    AddRoad(client, data);
                    break;

                case CommonEnumerators.RoadStepMode.Remove:
                    RemoveRoad(client, data);
                    break;
            }
        }

        private static void AddRoad(ServerClient client, RoadData data)
        {
            if (RoadManagerHelper.CheckIfRoadExists(data.details))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to add a road that already existed");
                return;
            }

            SaveRoad(data.details);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RoadPacket), data);
            foreach(ServerClient cClient in Network.connectedClients.ToArray())
            {
                cClient.listener.EnqueuePacket(packet);
            }
        }

        private static void RemoveRoad(ServerClient client, RoadData data)
        {
            if (!RoadManagerHelper.CheckIfRoadExists(data.details))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to remove a road that didn't exist");
                return;
            }

            foreach (RoadDetails existingRoad in Master.worldValues.Roads)
            {
                if (existingRoad.tileA == data.details.tileA && existingRoad.tileB == data.details.tileB)
                {
                    DeleteRoad(existingRoad);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else if (existingRoad.tileA == data.details.tileB && existingRoad.tileB == data.details.tileA)
                {
                    DeleteRoad(existingRoad);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else continue;
            }

            void BroadcastDeletion(RoadDetails toRemove)
            {
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RoadPacket), data);
                foreach (ServerClient cClient in Network.connectedClients.ToArray())
                {
                    cClient.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SaveRoad(RoadDetails details)
        {
            List<RoadDetails> currentRoads = Master.worldValues.Roads.ToList();
            currentRoads.Add(details);

            Master.worldValues.Roads = currentRoads.ToArray();
            WorldManager.SaveWorldValues(Master.worldValues);
        }

        private static void DeleteRoad(RoadDetails details)
        {
            List<RoadDetails> currentRoads = Master.worldValues.Roads.ToList();
            currentRoads.Remove(details);

            Master.worldValues.Roads = currentRoads.ToArray();
            WorldManager.SaveWorldValues(Master.worldValues);
        }
    }

    public static class RoadManagerHelper
    {
        public static bool CheckIfRoadExists(RoadDetails details)
        {
            foreach (RoadDetails existingRoad in Master.worldValues.Roads)
            {
                if (existingRoad.tileA == details.tileA && existingRoad.tileB == details.tileB) return true;
                else if (existingRoad.tileA == details.tileB && existingRoad.tileB == details.tileA) return true;
            }

            return false;
        }
    }
}
