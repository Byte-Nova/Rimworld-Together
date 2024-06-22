using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameServer
{
    public static class AidManager
    {
        private static readonly double baseAidTimer = 3600000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            AidData data = (AidData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.AidStepMode.Send:
                    SendAidRequest(client, data);
                    break;

                case CommonEnumerators.AidStepMode.Receive:
                    //Empty
                    break;

                case CommonEnumerators.AidStepMode.Accept:
                    SendAidAccept(client, data);
                    break;

                case CommonEnumerators.AidStepMode.Reject:
                    SendAidReject(client, data);
                    break;
            }
        }

        private static void SendAidRequest(ServerClient client, AidData data) 
        {
            if (!SettlementManager.CheckIfTileIsInUse(data.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.Username} attempted to send an aid packet to settlement at tile {data.toTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.toTile);
                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

                    if (Master.serverConfig.TemporalAidProtection && !TimeConverter.CheckForEpochTimer(target.AidProtectionTime, baseAidTimer))
                    {
                        data.stepMode = CommonEnumerators.AidStepMode.Reject;
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);

                        data.stepMode = CommonEnumerators.AidStepMode.Receive;
                        packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                        target.listener.EnqueuePacket(packet);
                    }
                }

                else
                {
                    data.stepMode = CommonEnumerators.AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidAccept(ServerClient client, AidData data)
        {
            if (!SettlementManager.CheckIfTileIsInUse(data.fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.Username} attempted to send an aid packet to settlement at tile {data.fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.fromTile);
                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    client.UpdateAidTime();

                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                    target.listener.EnqueuePacket(packet);
                }

                //Back to client sending the request

                else
                {
                    data.stepMode = CommonEnumerators.AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidReject(ServerClient client, AidData data) 
        {
            if (!SettlementManager.CheckIfTileIsInUse(data.fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.Username} attempted to send an aid packet to settlement at tile {data.fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.fromTile);
                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                    target.listener.EnqueuePacket(packet);
                }

                //Back to client sending the request

                else
                {
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
