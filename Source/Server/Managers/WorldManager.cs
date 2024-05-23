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

            switch (int.Parse(worldData.worldStepMode))
            {
                case (int)CommonEnumerators.WorldStepMode.Required:
                    SaveWorldPrefab(client, worldData);
                    break;

                case (int)CommonEnumerators.WorldStepMode.Existing:
                    //Do nothing
                    break;
            }
        }

        public static bool CheckIfWorldExists() { return File.Exists(worldFilePath); }

        public static void SaveWorldPrefab(ServerClient client, WorldData worldData)
        {
            WorldValuesFile worldValues = new WorldValuesFile();
            worldValues.seedString = worldData.seedString;
            worldValues.persistentRandomValue = worldData.persistentRandomValue;
            worldValues.planetCoverage = worldData.planetCoverage;
            worldValues.rainfall = worldData.rainfall;
            worldValues.temperature = worldData.temperature;
            worldValues.population = worldData.population;
            worldValues.pollution = worldData.pollution;
            worldValues.factions = worldData.factions;

            Master.worldValues = worldValues;
            Serializer.SerializeToFile(worldFilePath, worldValues);
            ConsoleManager.WriteToConsole($"[Save world] > {client.username}", LogMode.Title);
        }

        public static void RequireWorldFile(ServerClient client)
        {
            WorldData worldData = new WorldData();
            worldData.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Required).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendWorldFile(ServerClient client)
        {
            WorldValuesFile worldValues = Master.worldValues;

            WorldData worldData = new WorldData();
            worldData.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Existing).ToString();

            worldData.seedString = worldValues.seedString;
            worldData.persistentRandomValue = worldValues.persistentRandomValue;
            worldData.planetCoverage = worldValues.planetCoverage;
            worldData.rainfall = worldValues.rainfall;
            worldData.temperature = worldValues.temperature;
            worldData.population = worldValues.population;
            worldData.pollution = worldValues.pollution;
            worldData.factions = worldValues.factions;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            client.listener.EnqueuePacket(packet);
        }

        public static void LoadWorldFile()
        {
            if (File.Exists(worldFilePath))
            {
                Master.worldValues = Serializer.SerializeFromFile<WorldValuesFile>(worldFilePath);

                ConsoleManager.WriteToConsole("Loaded world values", LogMode.Warning);
            }

            else ConsoleManager.WriteToConsole("[Warning] > World is missing. Join server to create it", LogMode.Warning);   
        }
    }
}
