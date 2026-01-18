using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Files.Client;
using GameServer.Hooks.TCPNetwork;

namespace GameServer.Managers
{

    public static class EventManager
    {
        [HandlesPacket(PacketHeader.EventManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!Master.ActionConfigs.EventAction.IsEnabled)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            EventData data = Serializer.ConvertBytesToObject<EventData>(bytes);

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
            if (!SettlementManager.CheckIfTileIsInUse(eventData._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Username} attempted to send an event to settlement at tile {eventData._toTile}, but it has no settlement");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(eventData._toTile);
                if (!UserManagerH.CheckIfUserIsConnected(settlement.Username))
                {
                    eventData._stepMode = EventStepMode.Recover;
                    client.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);
                }

                else
                {
                    ServerClient target = ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username);

                    if (!PlayerCooldown.CheckIfCanEvent(target.UserFile, Master.ActionConfigs.EventAction.IsEnabled, Master.ActionConfigs.EventAction.Cooldown))
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

                        target.UserFile.Cooldowns.SetEventTimer(TimeConverter.GetCurrentTimeToEpoch(), target.UserFile);

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
