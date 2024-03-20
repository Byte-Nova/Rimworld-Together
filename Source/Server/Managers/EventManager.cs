﻿using Shared;

namespace GameServer
{
    public static class EventManager
    {
        public static void ParseEventPacket(ServerClient client, Packet packet)
        {
            EventDetailsJSON eventDetailsJSON = (EventDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(eventDetailsJSON.eventStepMode))
            {
                case (int)CommonEnumerators.EventStepMode.Send:
                    SendEvent(client, eventDetailsJSON);
                    break;

                case (int)CommonEnumerators.EventStepMode.Receive:
                    //Nothing goes here
                    break;

                case (int)CommonEnumerators.EventStepMode.Recover:
                    //Nothing goes here
                    break;
            }
        }

        public static void SendEvent(ServerClient client, EventDetailsJSON eventDetailsJSON)
        {
            if (!SettlementManager.CheckIfTileIsInUse(eventDetailsJSON.toTile)) ResponseShortcutManager.SendIllegalPacket(client, "Player attmepted to send an event to a tile that has no settlement");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(eventDetailsJSON.toTile);
                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Recover).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventDetailsJSON);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlement.owner);
                    if (target.inSafeZone)
                    {
                        eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Recover).ToString();
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventDetailsJSON);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        target.inSafeZone = true;

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventDetailsJSON);
                        client.listener.EnqueuePacket(packet);

                        eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Receive).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventDetailsJSON);
                        target.listener.EnqueuePacket(rPacket);
                    }
                }
            }
        }
    }
}
