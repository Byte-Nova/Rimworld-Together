using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OfflineEventManager
    {
        private static readonly double baseEventTimer = 3600000;

        public static void ParseEventPacket(ServerClient client, Packet packet)
        {
            EventData eventData = (EventData)Serializer.ConvertBytesToObject(packet.contents);

            switch (eventData.eventStepMode)
            {
                case EventStepMode.Send:
                    SendEvent(client, eventData);
                    break;

                case EventStepMode.Receive:
                    //Nothing goes here
                    break;

                case EventStepMode.Recover:
                    //Nothing goes here
                    break;
            }
        }

        public static void SendEvent(ServerClient client, EventData eventData)
        {
            if (!SettlementManager.CheckIfTileIsInUse(eventData.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.Username} attempted to send an event to settlement at tile {eventData.toTile}, but it has no settlement");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(eventData.toTile);
                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    eventData.eventStepMode = EventStepMode.Recover;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlement.owner);

                    if (Master.serverConfig.TemporalEventProtection && !TimeConverter.CheckForEpochTimer(target.EventProtectionTime, baseEventTimer))
                    {
                        eventData.eventStepMode = EventStepMode.Recover;
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        //Back to player

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                        client.listener.EnqueuePacket(packet);

                        //To the person that should receive it

                        eventData.eventStepMode = EventStepMode.Receive;

                        target.UpdateEventTime();

                        packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                        target.listener.EnqueuePacket(packet);
                    }
                }
            }
        }
    }
}
