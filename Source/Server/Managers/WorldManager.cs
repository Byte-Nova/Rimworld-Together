using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Server;

namespace GameServer.Managers
{

    public static class WorldManager
    {
        [HandlesPacket(PacketHeader.WorldManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            WorldData data = Serializer.ConvertBytesToObject<WorldData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case WorldStepMode.Sent:
                    ReceiveWorld(client, data);
                    break;
            }
        }

        public static bool CheckIfWorldExists() { return File.Exists(WorldValuesFile.Path); }

        public static void RequireWorldFile(ServerClient client)
        {
            WorldData worldData = new WorldData();
            worldData._stepMode = WorldStepMode.AskFor;

            client.Listener.EnqueuePacket(PacketHeader.WorldManager, worldData);
        }

        public static void SendWorld(ServerClient client)
        {
            WorldData data = new WorldData();
            WorldValuesFile file = Serializer.FileBytesToObject<WorldValuesFile>(WorldValuesFile.Path);

            data._fileBytes = Serializer.ConvertObjectToBytes(file);
            data._stepMode = WorldStepMode.Sent;

            client.Listener.EnqueuePacket(PacketHeader.WorldManager, data);
        }

        public static void ReceiveWorld(ServerClient client, WorldData data)
        {
            WorldValuesFile file = Serializer.ConvertBytesToObject<WorldValuesFile>(data._fileBytes);
            Serializer.ObjectBytesToFile(WorldValuesFile.Path, file);
            Master.WorldValues = file;

            InformationDisplayer.DisplaySetWorld(client);
        }
    }
}
