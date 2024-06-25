using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            SaveRoad(data.details.tileA, data.details.tileB, data.details);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RoadPacket), data);
            foreach(ServerClient cClient in Network.connectedClients.ToArray())
            {
                cClient.listener.EnqueuePacket(packet);
            }
        }

        private static void RemoveRoad(ServerClient client, RoadData data)
        {

        }

        private static void SaveRoad(int startingTile, int targetTile, RoadDetails details)
        {
            Serializer.SerializeToFile(Path.Combine(Master.roadsPath, $"{startingTile}-{targetTile}{fileExtension}"), details);
        }

        private static void DeleteRoad(RoadData data)
        {

        }

        public static RoadDetails[] GetAllRoads()
        {
            List<RoadDetails> roadDetails = new List<RoadDetails>();

            string[] roadPaths = Directory.GetFiles(Master.roadsPath);
            foreach(string roadPath in roadPaths)
            {
                RoadDetails details = Serializer.SerializeFromFile<RoadDetails>(roadPath);
                roadDetails.Add(details);
            }

            return roadDetails.ToArray();
        }
    }
}
