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
                case (int)CommonEnumerators.CommandType.Grant:
                    //Do nothing
                    break;

                case (int)CommonEnumerators.CommandType.Revoke:
                    //Do nothing
                    break;

                case (int)CommonEnumerators.CommandType.Broadcast:
                    //Do nothing
                    break;
            }
        }

        public static void SendGrantCommand(ServerClient client, string flag)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.Grant).ToString();
            commandData.commandDetails = flag;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendRevokeCommand(ServerClient client, string flag)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.Revoke).ToString();
            commandData.commandDetails = flag;

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

        public static void SendForceSaveCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData.commandType = ((int)CommonEnumerators.CommandType.ForceSave).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
