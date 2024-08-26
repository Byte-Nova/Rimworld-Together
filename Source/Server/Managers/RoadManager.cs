using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class RoadManager
    {
        public readonly static string fileExtension = ".mproad";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            RoadData data = Serializer.ConvertBytesToObject<RoadData>(packet.contents);

            switch (data.stepMode)
            {
                case RoadStepMode.Add:
                    AddRoad(client, data);
                    break;

                case RoadStepMode.Remove:
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

            SaveRoad(data.details, client);

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.RoadPacket), data);
            NetworkHelper.SendPacketToAllClients(packet);
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
                    DeleteRoad(existingRoad, client);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else if (existingRoad.tileA == data.details.tileB && existingRoad.tileB == data.details.tileA)
                {
                    DeleteRoad(existingRoad, client);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else continue;
            }

            void BroadcastDeletion(RoadDetails toRemove)
            {
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.RoadPacket), data);
                NetworkHelper.SendPacketToAllClients(packet);
            }
        }

        private static void SaveRoad(RoadDetails details, ServerClient client = null)
        {
            List<RoadDetails> currentRoads = Master.worldValues.Roads.ToList();
            currentRoads.Add(details);

            Master.worldValues.Roads = currentRoads.ToArray();
            Main_.SaveValueFile(ServerFileMode.World);

            if (client != null) Logger.Warning($"[Added road from tiles '{details.tileA}' to '{details.tileB}'] > {client.userFile.Username}");
            else Logger.Warning($"[Added road from tiles '{details.tileA}' to '{details.tileB}']");
        }

        private static void DeleteRoad(RoadDetails details, ServerClient client = null)
        {
            List<RoadDetails> currentRoads = Master.worldValues.Roads.ToList();
            currentRoads.Remove(details);

            Master.worldValues.Roads = currentRoads.ToArray();
            Main_.SaveValueFile(ServerFileMode.World);

            if (client != null) Logger.Warning($"[Removed road from tiles '{details.tileA}' to '{details.tileB}'] > {client.userFile.Username}");
            else Logger.Warning($"[Removed road from tiles '{details.tileA}' to '{details.tileB}']");
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
