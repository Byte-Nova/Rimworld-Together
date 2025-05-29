using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class RoadManager
    {
        public readonly static string fileExtension = ".mproad";

        [HandlesPacket(PacketHeader.RoadManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            if (!Master.ActionConfigs.EnableRoads)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            RoadData data = Serializer.ConvertBytesToObject<RoadData>(bytes);

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

            NetworkHelper.SendPacketToAllClients(PacketHeader.RoadManager, data);
        }

        private static void RemoveRoad(ServerClient client, RoadData data)
        {
            if (!RoadManagerHelper.CheckIfRoadExists(data._details))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to remove a road that didn't exist");
                return;
            }

            foreach (RoadDetails existingRoad in Master.WorldValues.Roads)
            {
                if (existingRoad.FromTile == data._details.FromTile && existingRoad.ToTile == data._details.ToTile)
                {
                    DeleteRoad(existingRoad, client);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else if (existingRoad.FromTile == data._details.ToTile && existingRoad.ToTile == data._details.FromTile)
                {
                    DeleteRoad(existingRoad, client);
                    BroadcastDeletion(existingRoad);
                    return;
                }

                else continue;
            }

            void BroadcastDeletion(RoadDetails toRemove)
            {
                NetworkHelper.SendPacketToAllClients(PacketHeader.RoadManager, data);
            }
        }

        private static void SaveRoad(RoadDetails details, ServerClient client = null)
        {
            List<RoadDetails> currentRoads = Master.WorldValues.Roads.ToList();
            currentRoads.Add(details);

            Master.WorldValues.Roads = currentRoads.ToArray();
            Main_.SaveValueFile(ServerFileMode.World, false);

            InformationDisplayer.DisplayAddRoad(details.FromTile.ToString(), details.ToTile.ToString());
        }

        private static void DeleteRoad(RoadDetails details, ServerClient client = null)
        {
            List<RoadDetails> currentRoads = Master.WorldValues.Roads.ToList();
            currentRoads.Remove(details);

            Master.WorldValues.Roads = currentRoads.ToArray();
            Main_.SaveValueFile(ServerFileMode.World, false);

            InformationDisplayer.DisplayRemoveRoad(details.FromTile.ToString(), details.ToTile.ToString());
        }
    }

    public static class RoadManagerHelper
    {
        public static bool CheckIfRoadExists(RoadDetails details)
        {
            foreach (RoadDetails existingRoad in Master.WorldValues.Roads)
            {
                if (existingRoad.FromTile == details.FromTile && existingRoad.ToTile == details.ToTile) return true;
                else if (existingRoad.FromTile == details.ToTile && existingRoad.ToTile == details.FromTile) return true;
            }

            return false;
        }
    }
}
