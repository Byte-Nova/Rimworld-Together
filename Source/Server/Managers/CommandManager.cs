using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CommandManager
    {
        public static void ParseCommand(Packet packet)
        {
            CommandData commandData = Serializer.ConvertBytesToObject<CommandData>(packet.contents);

            switch (commandData.commandMode)
            {
                case CommandMode.Op:
                    //Do nothing
                    break;

                case CommandMode.Deop:
                    //Do nothing
                    break;

                case CommandMode.Broadcast:
                    //Do nothing
                    break;
            }
        }

        public static void SendOpCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandMode = CommandMode.Op;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendDeOpCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandMode = CommandMode.Deop;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);

        }

        public static void SendEventCommand(ServerClient client, int eventID)
        {
            EventData eventData = new EventData();
            eventData.eventStepMode = EventStepMode.Receive;
            eventData.eventID = eventID;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.EventPacket), eventData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendBroadcastCommand(string str)
        {
            CommandData commandData = new CommandData();
            commandData.commandMode = CommandMode.Broadcast;
            commandData.commandDetails = str;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                client.listener.EnqueuePacket(packet);
            }
        }

        public static void SendForceSaveCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandMode = CommandMode.ForceSave;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
