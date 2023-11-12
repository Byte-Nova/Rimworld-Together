using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class EventManager
    {
        public static void ParseEventPacket(ServerClient client, Packet packet)
        {
            EventDetailsJSON eventDetailsJSON = (EventDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

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
            if (!SettlementManager.CheckIfTileIsInUse(eventDetailsJSON.toTile)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(eventDetailsJSON.toTile);
                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Recover).ToString();
                    Packet packet = Packet.CreatePacketFromJSON("EventPacket", eventDetailsJSON);
                    client.clientListener.SendData(packet);
                }

                else
                {
                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlement.owner);
                    if (target.inSafeZone)
                    {
                        eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Recover).ToString();
                        Packet packet = Packet.CreatePacketFromJSON("EventPacket", eventDetailsJSON);
                        client.clientListener.SendData(packet);
                    }

                    else
                    {
                        target.inSafeZone = true;

                        Packet packet = Packet.CreatePacketFromJSON("EventPacket", eventDetailsJSON);
                        client.clientListener.SendData(packet);

                        eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Receive).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON("EventPacket", eventDetailsJSON);
                        target.clientListener.SendData(rPacket);
                    }
                }
            }
        }
    }
}
