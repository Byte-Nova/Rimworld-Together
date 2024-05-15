using Newtonsoft.Json;
using Shared;

namespace GameServer
{
    public static class CommandManager
    {
        public static void ParseCommand(Packet packet)
        {
            CommandData commandData = (CommandData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(commandData.commandType))
            {
                case (int)CommonEnumerators.CommandType.Op:
                    //Do nothing
                    break;

                case (int)CommonEnumerators.CommandType.Deop:
                    //Do nothing
                    break;

                case (int)CommonEnumerators.CommandType.Broadcast:
                    //Do nothing
                    break;
            }
        }

        public static void SendOpCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.Op).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendDeOpCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.Deop).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);

        }

        public static void SendEventCommand(ServerClient client, int eventID)
        {
            EventData eventData = new EventData();
            eventData.eventStepMode = ((int)CommonEnumerators.EventStepMode.Receive).ToString();
            eventData.eventID = eventID.ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendBroadcastCommand(string str)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.Broadcast).ToString();
            commandData.commandDetails = str;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                client.listener.EnqueuePacket(packet);
            }
        }

        public static void SendForceSaveCommand(ServerClient client, bool isDisconnecting = true)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.ForceSave).ToString();
            commandData.commandDetails = isDisconnecting ? "" : "isNotDisconnecting";

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SyncSettlementsCommand(ServerClient client)
        {
            var settlements = SettlementManager.GetAllSettlementsFromUsername(client.username);

            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.SyncSettlements).ToString();

            List<string> tiles = new();
            foreach(var settlement in settlements)
            {
                tiles.Add(settlement.tile);
            }

            commandData.commandDetails = string.Join(',', tiles);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
