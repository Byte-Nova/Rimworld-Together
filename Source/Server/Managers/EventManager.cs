using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class EventManager
    {
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
            if (!SettlementManager.CheckIfTileIsInUse(eventData.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to send an event to settlement at tile {eventData.toTile}, but it has no settlement");
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
                    if (target.inSafeZone)
                    {
                        eventData.eventStepMode = EventStepMode.Recover;
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        target.inSafeZone = true;

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                        client.listener.EnqueuePacket(packet);

                        eventData.eventStepMode = EventStepMode.Receive;
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                        target.listener.EnqueuePacket(rPacket);
                    }
                }
            }
        }
    }
}
