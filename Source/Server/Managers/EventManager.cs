using Shared;

namespace GameServer
{
    public static class EventManager
    {
        public static void ParseEventPacket(ServerClient client, Packet packet)
        {
            EventData eventData = (EventData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(eventData.eventStepMode))
            {
                case (int)CommonEnumerators.EventStepMode.Send:
                    SendEvent(client, eventData);
                    break;

                case (int)CommonEnumerators.EventStepMode.Receive:
                    //Nothing goes here
                    break;

                case (int)CommonEnumerators.EventStepMode.Recover:
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
                    eventData.eventStepMode = ((int)CommonEnumerators.EventStepMode.Recover).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlement.owner);
                    /* @TODO(jrseducate@gmail.com): Re-enable this once [target.inSafeZone] is properly updated based on it's intended purpose */
                    //if (target.inSafeZone)
                    //{
                    //    eventData.eventStepMode = ((int)CommonEnumerators.EventStepMode.Recover).ToString();
                    //    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                    //    client.listener.EnqueuePacket(packet);
                    //}

                    //else
                    //{
                    //    target.inSafeZone = true;

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                    client.listener.EnqueuePacket(packet);

                    eventData.eventStepMode = ((int)CommonEnumerators.EventStepMode.Receive).ToString();
                    Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                    target.listener.EnqueuePacket(rPacket);
                    //}
                }
            }
        }
    }
}
