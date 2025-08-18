using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Server;

namespace GameServer.Managers
{

    public static class EventManager
    {
        [HandlesPacket(PacketHeader.EventManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            if (!Master.ActionConfigs.EnableEvents)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            EventData data = Serializer.ConvertBytesToObject<EventData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case EventStepMode.Send:
                    SendEvent(client, data);
                    break;

                case EventStepMode.Set:
                    SetEvents(client, data);
                    break;

                case EventStepMode.Customize:
                    ModifyEvents(client, data);
                    break;
            }
        }

        public static void SendEvent(ServerClient client, EventData eventData)
        {
            if (!SettlementManager.CheckIfTileIsInUse(eventData._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Uid} attempted to send an event to settlement at tile {eventData._toTile}, but it has no settlement");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(eventData._toTile);
                if (!UserManagerH.CheckIfUserIsConnected(settlement.UID))
                {
                    eventData._stepMode = EventStepMode.Recover;
                    client.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);
                }

                else
                {
                    ServerClient target = ServerNetwork.Instance.GetConnectedClientFromUid(settlement.UID);

                    if (!ValueChecker.CheckIfCanEvent(target.UserFile))
                    {
                        eventData._stepMode = EventStepMode.Recover;
                        client.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);
                    }

                    else
                    {
                        //Back to player

                        client.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);

                        //To the person that should receive it

                        eventData._stepMode = EventStepMode.Receive;

                        target.UserFile.UpdateEventTime();

                        target.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);
                    }
                }
            }
        }

        public static void SetEvents(ServerClient client, EventData eventData)
        {
            if (EventManagerH.LoadedEvents.Count() > 0) ResponseShortcutManager.SendIllegalPacket(client, "Illegal setting of events!");
            else
            {
                foreach (EventFile file in eventData._eventFiles)
                {
                    Serializer.SerializeToFile(Path.Combine(Master.EventsPath, file.DefName + EventManagerH.FileExtension), file);
                }

                EventManagerH.LoadAllEvents();
                InformationDisplayer.DisplaySetEvents(client);
            }
        }

        private static void ModifyEvents(ServerClient client, EventData data)
        {
            if (!client.UserFile.IsAdmin) ResponseShortcutManager.SendIllegalPacket(client, "Tried to modify events without being admin!");
            else
            {
                foreach (EventFile file in data._eventFiles)
                {
                    Serializer.SerializeToFile(Path.Combine(Master.EventsPath, file.DefName + EventManagerH.FileExtension), file);
                }

                EventManagerH.LoadAllEvents();
                InformationDisplayer.DisplaySetEvents(client);
            }
        }
    }

    public static class EventManagerH
    {
        public static string FileExtension { get; private set; } = ".mpevent";

        public static EventFile[] LoadedEvents { get; private set; } = null;

        public static void LoadAllEvents()
        {
            List<EventFile> toLoad = new List<EventFile>();
            foreach (string str in Directory.GetFiles(Master.EventsPath))
            {
                toLoad.Add(Serializer.SerializeFromFile<EventFile>(str));
            }

            LoadedEvents = toLoad.OrderBy(fetch => fetch.Name).ToArray();
        }
    }
}
