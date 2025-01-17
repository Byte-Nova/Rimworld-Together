using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class RoadManager
    {
        public readonly static string fileExtension = ".mproad";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionConfigs.EnableRoads)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            RoadData data = Serializer.ConvertBytesToObject<RoadData>(packet.contents);

            switch (data._stepMode)
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
            if (RoadManagerHelper.CheckIfRoadExists(data._details))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to add a road that already existed");
                return;
            }

            SaveRoad(data._details, client);

            Packet packet = Packet.CreatePacketFromObject(nameof(RoadManager), data);
            NetworkHelper.SendPacketToAllClients(packet);
        }

        private static void RemoveRoad(ServerClient client, RoadData data)
        {
            if (!RoadManagerHelper.CheckIfRoadExists(data._details))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to remove a road that didn't exist");
                return;
            }

            foreach (RoadDetails existingRoad in Master.worldValues.Roads)
            {
                if (existingRoad.fromTile == data._details.fromTile && existingRoad.toTile == data._details.toTile)
                {
                    DeleteRoad(existingRoad, client);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else if (existingRoad.fromTile == data._details.toTile && existingRoad.toTile == data._details.fromTile)
                {
                    DeleteRoad(existingRoad, client);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else continue;
            }

            void BroadcastDeletion(RoadDetails toRemove)
            {
                Packet packet = Packet.CreatePacketFromObject(nameof(RoadManager), data);
                NetworkHelper.SendPacketToAllClients(packet);
            }
        }

        private static void SaveRoad(RoadDetails details, ServerClient client = null)
        {
            List<RoadDetails> currentRoads = Master.worldValues.Roads.ToList();
            currentRoads.Add(details);

            Master.worldValues.Roads = currentRoads.ToArray();
            Main_.SaveValueFile(ServerFileMode.World, false);

            InformationDisplayer.DisplayAddRoad(details.fromTile.ToString(), details.toTile.ToString());
        }

        private static void DeleteRoad(RoadDetails details, ServerClient client = null)
        {
            List<RoadDetails> currentRoads = Master.worldValues.Roads.ToList();
            currentRoads.Remove(details);

            Master.worldValues.Roads = currentRoads.ToArray();
            Main_.SaveValueFile(ServerFileMode.World, false);

            InformationDisplayer.DisplayRemoveRoad(details.fromTile.ToString(), details.toTile.ToString());
        }
    }

    public static class RoadManagerHelper
    {
        public static bool CheckIfRoadExists(RoadDetails details)
        {
            foreach (RoadDetails existingRoad in Master.worldValues.Roads)
            {
                if (existingRoad.fromTile == details.fromTile && existingRoad.toTile == details.toTile) return true;
                else if (existingRoad.fromTile == details.toTile && existingRoad.toTile == details.fromTile) return true;
            }

            return false;
        }
    }
}
