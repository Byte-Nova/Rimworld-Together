using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Managers
{
    public static class WorldManager
    {
        public enum WorldStepMode { Required, Existing, Saved }

        private static string worldFileName = "WorldValues.json";

        private static string worldFilePath = Path.Combine(Program.corePath, worldFileName);

        public static void ParseWorldPacket(ServerClient client, Packet packet)
        {
            WorldDetailsJSON worldDetailsJSON = (WorldDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(worldDetailsJSON.worldStepMode))
            {
                case (int)WorldStepMode.Required:
                    SaveWorldPrefab(client, worldDetailsJSON);
                    break;

                case (int)WorldStepMode.Existing:
                    //Do nothing
                    break;

                case (int)WorldStepMode.Saved:
                    //Do nothing
                    break;
            }
        }

        public static bool CheckIfWorldExists() { return File.Exists(worldFilePath); }

        public static void SaveWorldPrefab(ServerClient client, WorldDetailsJSON worldDetailsJSON)
        {
            WorldValuesFile worldValues = new WorldValuesFile();
            worldValues.SeedString = worldDetailsJSON.SeedString;
            worldValues.PlanetCoverage = worldDetailsJSON.PlanetCoverage;
            worldValues.Rainfall = worldDetailsJSON.Rainfall;
            worldValues.Temperature = worldDetailsJSON.Temperature;
            worldValues.Population = worldDetailsJSON.Population;
            worldValues.Pollution = worldDetailsJSON.Pollution;
            worldValues.Factions = worldDetailsJSON.Factions;

            Serializer.SerializeToFile(worldFilePath, worldValues);
            Logger.WriteToConsole($"[Save world] > {client.username}", Logger.LogMode.Title);

            Program.worldValues = worldValues;

            worldDetailsJSON.worldStepMode = ((int)WorldStepMode.Saved).ToString();
            Packet packet = Packet.CreatePacketFromJSON("WorldPacket", worldDetailsJSON);
            client.clientListener.SendData(packet);
        }

        public static void RequireWorldFile(ServerClient client)
        {
            WorldDetailsJSON worldDetailsJSON = new WorldDetailsJSON();
            worldDetailsJSON.worldStepMode = ((int)WorldStepMode.Required).ToString();

            Packet packet = Packet.CreatePacketFromJSON("WorldPacket", worldDetailsJSON);
            client.clientListener.SendData(packet);
        }

        public static void SendWorldFile(ServerClient client)
        {
            WorldValuesFile worldValues = Program.worldValues;

            WorldDetailsJSON worldDetailsJSON = new WorldDetailsJSON();
            worldDetailsJSON.worldStepMode = ((int)WorldStepMode.Existing).ToString();
            worldDetailsJSON.SeedString = worldValues.SeedString;
            worldDetailsJSON.PlanetCoverage = worldValues.PlanetCoverage;
            worldDetailsJSON.Rainfall = worldValues.Rainfall;
            worldDetailsJSON.Temperature = worldValues.Temperature;
            worldDetailsJSON.Population = worldValues.Population;
            worldDetailsJSON.Pollution = worldValues.Pollution;
            worldDetailsJSON.Factions = worldValues.Factions;

            Packet packet = Packet.CreatePacketFromJSON("WorldPacket", worldDetailsJSON);
            client.clientListener.SendData(packet);
        }

        public static void LoadWorldFile()
        {
            if (File.Exists(worldFilePath))
            {
                Program.worldValues = Serializer.SerializeFromFile<WorldValuesFile>(worldFilePath);

                Logger.WriteToConsole("Loaded world values");
            }

            else Logger.WriteToConsole("[Warning] > World is missing. Join server to create it", Logger.LogMode.Warning);   
        }
    }
}
