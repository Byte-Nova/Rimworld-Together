using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class WorldManager
    {
        private static string worldFileName = "WorldValues.json";

        private static string worldFilePath = Path.Combine(Master.corePath, worldFileName);

        public static void ParseWorldPacket(ServerClient client, Packet packet)
        {
            WorldData worldData = (WorldData)Serializer.ConvertBytesToObject(packet.contents);

            switch (worldData.worldStepMode)
            {
                case WorldStepMode.Required:
                    SaveWorldPrefab(client, worldData);
                    break;

                case WorldStepMode.Existing:
                    //Do nothing
                    break;
            }
        }

        public static bool CheckIfWorldExists() { return File.Exists(worldFilePath); }

        public static void SaveWorldPrefab(ServerClient client, WorldData worldData)
        {
            Master.worldValues = worldData.worldValuesFile;
            Serializer.SerializeToFile(worldFilePath, Master.worldValues);
            Logger.Title($"[Save world] > {client.Username}");
        }

        public static void RequireWorldFile(ServerClient client)
        {
            WorldData worldData = new WorldData();
            worldData.worldStepMode = WorldStepMode.Required;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendWorldFile(ServerClient client)
        {
            WorldData worldData = new WorldData();
            worldData.worldStepMode = WorldStepMode.Existing;
            worldData.worldValuesFile = Master.worldValues;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            client.listener.EnqueuePacket(packet);
        }

        public static void LoadWorldFile()
        {
            if (File.Exists(worldFilePath))
            {
                Master.worldValues = Serializer.SerializeFromFile<WorldValuesFile>(worldFilePath);

                Logger.Warning("Loaded world values");
            }

            else Logger.Warning("World is missing. Join server to create it");   
        }
    }
}
