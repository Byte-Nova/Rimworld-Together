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
            WorldValuesFile worldValues = new WorldValuesFile();
            worldValues.seedString              = worldData.seedString;
            worldValues.persistentRandomValue   = worldData.persistentRandomValue;
            worldValues.planetCoverage          = worldData.planetCoverage;
            worldValues.rainfall                = worldData.rainfall;
            worldValues.temperature             = worldData.temperature;
            worldValues.population              = worldData.population;
            worldValues.pollution               = worldData.pollution;
            worldValues.factions                = worldData.factions;
            worldValues.deflateDictionary       = worldData.deflateDictionary;
            worldValues.SettlementDatas         = worldData.SettlementDatas;

            Master.worldValues = worldValues;
            Serializer.SerializeToFile(worldFilePath, worldValues);
            Logger.WriteToConsole($"[Save world] > {client.username}", LogMode.Title);
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
            WorldValuesFile worldValues = Master.worldValues;

            WorldData worldData = new WorldData();
            worldData.worldStepMode = WorldStepMode.Existing;

            worldData.seedString            = worldValues.seedString;
            worldData.persistentRandomValue = worldValues.persistentRandomValue;
            worldData.planetCoverage        = worldValues.planetCoverage;
            worldData.rainfall              = worldValues.rainfall;
            worldData.temperature           = worldValues.temperature;
            worldData.population            = worldValues.population;
            worldData.pollution             = worldValues.pollution;
            worldData.factions              = worldValues.factions;
            worldData.deflateDictionary     = worldValues.deflateDictionary;
            worldData.SettlementDatas       = worldValues.SettlementDatas;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            client.listener.EnqueuePacket(packet);
        }

        public static void LoadWorldFile()
        {
            if (File.Exists(worldFilePath))
            {
                Master.worldValues = Serializer.SerializeFromFile<WorldValuesFile>(worldFilePath);

                Logger.WriteToConsole("Loaded world values", LogMode.Warning);
            }

            else Logger.WriteToConsole("[Warning] > World is missing. Join server to create it", LogMode.Warning);   
        }
    }
}
