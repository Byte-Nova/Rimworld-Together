using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CommandManager
    {
        public static void ParseCommand(Packet packet)
        {
            CommandData commandData = (CommandData)Serializer.ConvertBytesToObject(packet.contents);

            switch (commandData.commandType)
            {
                case CommandName.Op:
                    //Do nothing
                    break;

                case CommandName.Deop:
                    //Do nothing
                    break;

                case CommandName.Broadcast:
                    //Do nothing
                    break;
            }
        }

        public static void SendOpCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = CommandName.Op;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendDeOpCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = CommandName.Deop;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);

        }

        public static void SendEventCommand(ServerClient client, int eventID)
        {
            EventData eventData = new EventData();
            eventData.eventStepMode = EventStepMode.Receive;
            eventData.eventID = eventID.ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendBroadcastCommand(string str)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = CommandName.Broadcast;
            commandData.commandDetails = str;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                client.listener.EnqueuePacket(packet);
            }
        }

        public static void SendForceSaveCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = CommandName.ForceSave;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
