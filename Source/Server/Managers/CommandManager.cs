using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Managers
{
    public static class CommandManager
    {
        public enum CommandType { Op, Deop, Ban, Disconnect, Quit, Broadcast }

        public static void ParseCommand(Packet packet)
        {
            CommandDetailsJSON commandDetailsJSON = Serializer.SerializeFromString<CommandDetailsJSON>(packet.contents[0]);

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

        public static void SendOpCommand(Client client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Op).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(commandDetailsJSON) };
            Packet packet = new Packet("CommandPacket", contents);
            Network.Network.SendData(client, packet);
        }

        public static void SendDeOpCommand(Client client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Deop).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(commandDetailsJSON) };
            Packet packet = new Packet("CommandPacket", contents);
            Network.Network.SendData(client, packet);

        }

        public static void SendBanCommand(Client client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Ban).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(commandDetailsJSON) };
            Packet packet = new Packet("CommandPacket", contents);
            Network.Network.SendData(client, packet);

        }

        public static void SendDisconnectCommand(Client client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Disconnect).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(commandDetailsJSON) };
            Packet packet = new Packet("CommandPacket", contents);
            Network.Network.SendData(client, packet);

        }

        public static void SendQuitCommand(Client client)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Quit).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(commandDetailsJSON) };
            Packet packet = new Packet("CommandPacket", contents);
            Network.Network.SendData(client, packet);
        }

        public static void SendEventCommand(Client client, int eventID)
        {
            EventDetailsJSON eventDetailsJSON = new EventDetailsJSON();
            eventDetailsJSON.eventStepMode = ((int)EventManager.EventStepMode.Receive).ToString();
            eventDetailsJSON.eventID = eventID.ToString();

            string[] contents = new string[] { Serializer.SerializeToString(eventDetailsJSON) };
            Packet packet = new Packet("EventPacket", contents);
            Network.Network.SendData(client, packet);
        }

        public static void SendBroadcastCommand(string str)
        {
            CommandDetailsJSON commandDetailsJSON = new CommandDetailsJSON();
            commandDetailsJSON.commandType = ((int)CommandType.Broadcast).ToString();
            commandDetailsJSON.commandDetails = str;

            string[] contents = new string[] { Serializer.SerializeToString(commandDetailsJSON) };
            Packet packet = new Packet("CommandPacket", contents);
            foreach (Client client in Network.Network.connectedClients.ToArray())
            {
                Network.Network.SendData(client, packet);
            }
        }
    }
}
