using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Managers
{
    public static class CommandManager
    {
        public enum CommandType { Op, Deop, Ban, Disconnect, Quit, Broadcast, ForceSave }

        public static void ParseCommand(Packet packet)
        {
            CommandDetailsJSON commandDetailsJSON = (CommandDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(commandDetailsJSON.commandType))
            {
                case (int)CommandType.Op:
                    //Do nothing
                    break;

                case (int)CommandType.Deop:
                    //Do nothing
                    break;

                case (int)CommandType.Ban:
                    //Do nothing
                    break;

                case (int)CommandType.Disconnect:
                    //Do nothing
                    break;

                case (int)CommandType.Quit:
                    //Do nothing
                    break;

                case (int)CommandType.Broadcast:
                    //Do nothing
                    break;
            }
        }

        public static void SendOpCommand(ServerClient client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Op).ToString();

            Packet packet = Packet.CreatePacketFromJSON("CommandPacket", commandDetailsJSON);
            client.clientListener.SendData(packet);
        }

        public static void SendDeOpCommand(ServerClient client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Deop).ToString();

            Packet packet = Packet.CreatePacketFromJSON("CommandPacket", commandDetailsJSON);
            client.clientListener.SendData(packet);

        }

        public static void SendBanCommand(ServerClient client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Ban).ToString();

            Packet packet = Packet.CreatePacketFromJSON("CommandPacket", commandDetailsJSON);
            client.clientListener.SendData(packet);

        }

        public static void SendDisconnectCommand(ServerClient client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Disconnect).ToString();

            Packet packet = Packet.CreatePacketFromJSON("CommandPacket", commandDetailsJSON);
            client.clientListener.SendData(packet);

        }

        public static void SendQuitCommand(ServerClient client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Quit).ToString();

            Packet packet = Packet.CreatePacketFromJSON("CommandPacket", commandDetailsJSON);
            client.clientListener.SendData(packet);
        }

        public static void SendEventCommand(ServerClient client, int eventID)
        {
            EventDetailsJSON eventDetailsJSON = new EventDetailsJSON();
            eventDetailsJSON.eventStepMode = ((int)EventManager.EventStepMode.Receive).ToString();
            eventDetailsJSON.eventID = eventID.ToString();

            Packet packet = Packet.CreatePacketFromJSON("EventPacket", eventDetailsJSON);
            client.clientListener.SendData(packet);
        }

        public static void SendBroadcastCommand(string str)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Broadcast).ToString();
            commandDetailsJSON.commandDetails = str;

            Packet packet = Packet.CreatePacketFromJSON("CommandPacket", commandDetailsJSON);
            foreach (ServerClient client in Network.Network.connectedClients.ToArray())
            {
                client.clientListener.SendData(packet);
            }
        }

        public static void SendForceSaveCommand(ServerClient client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.ForceSave).ToString();

            Packet packet = Packet.CreatePacketFromJSON("CommandPacket", commandDetailsJSON);
            client.clientListener.SendData(packet);
        }
    }
}
