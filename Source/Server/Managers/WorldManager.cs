using Shared;

namespace GameServer
{
    public static class WorldManager
    {
        private static string worldFileName = "WorldValues.json";

        private static string worldFilePath = Path.Combine(Master.corePath, worldFileName);

        public static void ParseWorldPacket(ServerClient client, Packet packet)
        {
            WorldDetailsJSON worldDetailsJSON = (WorldDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(worldDetailsJSON.worldStepMode))
            {
                case (int)CommonEnumerators.WorldStepMode.Required:
                    SaveWorldPrefab(client, worldDetailsJSON);
                    break;

                case (int)CommonEnumerators.WorldStepMode.Existing:
                    //Do nothing
                    break;
            }
        }

        public static bool CheckIfWorldExists() { return File.Exists(worldFilePath); }

        public static void SaveWorldPrefab(ServerClient client, WorldDetailsJSON worldDetailsJSON)
        {
            WorldValuesFile worldValues = new WorldValuesFile();
            worldValues.seedString = worldDetailsJSON.seedString;
            worldValues.persistentRandomValue = worldDetailsJSON.persistentRandomValue;
            worldValues.planetCoverage = worldDetailsJSON.planetCoverage;
            worldValues.rainfall = worldDetailsJSON.rainfall;
            worldValues.temperature = worldDetailsJSON.temperature;
            worldValues.population = worldDetailsJSON.population;
            worldValues.pollution = worldDetailsJSON.pollution;
            worldValues.factions = worldDetailsJSON.factions;

            Master.worldValues = worldValues;
            Serializer.SerializeToFile(worldFilePath, worldValues);
            Logger.WriteToConsole($"[Save world] > {client.username}", Logger.LogMode.Title);
        }

        public static void RequireWorldFile(ServerClient client)
        {
            WorldDetailsJSON worldDetailsJSON = new WorldDetailsJSON();
            worldDetailsJSON.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Required).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldDetailsJSON);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendWorldFile(ServerClient client)
        {
            WorldValuesFile worldValues = Master.worldValues;

            WorldDetailsJSON worldDetailsJSON = new WorldDetailsJSON();
            worldDetailsJSON.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Existing).ToString();

            worldDetailsJSON.seedString = worldValues.seedString;
            worldDetailsJSON.persistentRandomValue = worldValues.persistentRandomValue;
            worldDetailsJSON.planetCoverage = worldValues.planetCoverage;
            worldDetailsJSON.rainfall = worldValues.rainfall;
            worldDetailsJSON.temperature = worldValues.temperature;
            worldDetailsJSON.population = worldValues.population;
            worldDetailsJSON.pollution = worldValues.pollution;
            worldDetailsJSON.factions = worldValues.factions;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldDetailsJSON);
            client.listener.EnqueuePacket(packet);
        }

        public static void LoadWorldFile()
        {
            if (File.Exists(worldFilePath))
            {
                Master.worldValues = Serializer.SerializeFromFile<WorldValuesFile>(worldFilePath);

                Logger.WriteToConsole("Loaded world values", Logger.LogMode.Warning);
            }

            else Logger.WriteToConsole("[Warning] > World is missing. Join server to create it", Logger.LogMode.Warning);   
        }
    }
}
