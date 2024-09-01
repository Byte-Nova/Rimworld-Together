using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CommandManager
    {
        public static void ParseCommand(Packet packet)
        {
            CommandData commandData = Serializer.ConvertBytesToObject<CommandData>(packet.contents);

            switch (commandData._commandMode)
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
            commandData._commandMode = CommandMode.Op;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendDeOpCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData._commandMode = CommandMode.Deop;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendEventCommand(ServerClient client, EventFile eventFile)
        {
            EventData eventData = new EventData();
            eventData._stepMode = EventStepMode.Receive;
            eventData._eventFile = eventFile;

            //We set it to -1 to let the client know it will fall at any settlement
            eventData._toTile = -1;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.EventPacket), eventData);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendBroadcastCommand(string str)
        {
            CommandData commandData = new CommandData();
            commandData._commandMode = CommandMode.Broadcast;
            commandData._details = str;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            NetworkHelper.SendPacketToAllClients(packet);
        }

        public static void SendForceSaveCommand(ServerClient client)
        {
            CommandData commandData = new CommandData();
            commandData._commandMode = CommandMode.ForceSave;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CommandPacket), commandData);
            client.listener.EnqueuePacket(packet);
        }
    }

    public static class ConsoleCommandManager
    {
        //Variable that stores the command parameters while they execute

        public static string[] commandParameters;

        public static void ParseServerCommands(string parsedString)
        {
            string parsedPrefix = parsedString.Split(' ')[0].ToLower();
            int parsedParameters = parsedString.Split(' ').Count() - 1;
            commandParameters = parsedString.Replace(parsedPrefix + " ", "").Split(" ");

            try
            {
                ServerCommand commandToFetch = CommandStorage.serverCommands.ToList().Find(x => x.prefix == parsedPrefix);
                if (commandToFetch == null) Logger.Warning($"Command '{parsedPrefix}' was not found");
                else
                {
                    if (commandToFetch.parameters != parsedParameters && commandToFetch.parameters != -1)
                    {
                        Logger.Warning($"Command '{commandToFetch.prefix}' wanted [{commandToFetch.parameters}] parameters "
                            + $"but was passed [{parsedParameters}]");
                    }

                    else
                    {
                        if (commandToFetch.commandAction != null) commandToFetch.commandAction.Invoke();

                        else Logger.Warning($"Command '{commandToFetch.prefix}' didn't have any action built in");
                    }
                }
            }
            catch (Exception e) { Logger.Error($"Couldn't parse command '{parsedPrefix}'. Reason: {e}"); }
        }

        public static void ListenForServerCommands()
        {
            bool interactiveConsole = false;

            try { interactiveConsole = Console.In.Peek() != -1 ? true : false; }
            catch { Logger.Warning($"Couldn't find interactive console, disabling commands"); }

            if (interactiveConsole)
            {
                while (true)
                {
                    ParseServerCommands(Console.ReadLine());
                }
            }
        }
    }
}