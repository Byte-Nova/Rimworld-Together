using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class WorldManager
    {
        public static string baseWorldPath = Path.Combine(Master.ConfigsPath, "WorldConfig.json");

        [HandlesPacket(PacketHeader.WorldManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            WorldData data = Serializer.ConvertBytesToObject<WorldData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case WorldStepMode.Sent:
                    WorldManagerReceiver.ReceiveWorld(client, data);
                    break;
            }
        }

        public static bool CheckIfWorldExists() { return File.Exists(baseWorldPath); }

        public static void RequireWorldFile(ServerClient client)
        {
            WorldData worldData = new WorldData();
            worldData._stepMode = WorldStepMode.AskFor;

            client.Listener.EnqueuePacket(PacketHeader.WorldManager, worldData);
        }
    }

    public static class WorldManagerSender
    {
        public static void SendWorld(ServerClient client)
        {
            WorldData data = new WorldData();
            data._fileBytes = GZip.DecompressBytes(File.ReadAllBytes(WorldManager.baseWorldPath));
            data._stepMode = WorldStepMode.Sent;

            client.Listener.EnqueuePacket(PacketHeader.WorldManager, data);
        }
    }

    public static class WorldManagerReceiver
    {
        public static void ReceiveWorld(ServerClient client, WorldData data)
        {
            File.WriteAllBytes(WorldManager.baseWorldPath, GZip.CompressBytes(data._fileBytes));
            Main_.LoadValueFile(ServerFileMode.World);
        }
    }
}
