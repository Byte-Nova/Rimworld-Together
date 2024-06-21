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
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            AidData data = (AidData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.AidStepMode.Send:
                    SendAidRequest(client, data);
                    break;

                case CommonEnumerators.AidStepMode.Receive:
                    //Do nothing
                    break;

                case CommonEnumerators.AidStepMode.Recover:
                    RecoverAidRequest(client, data);
                    break;
            }
        }

        private static void SendAidRequest(ServerClient client, AidData data) { CheckIfPossible(client, data, data.toTile); }

        private static void RecoverAidRequest(ServerClient client, AidData data) { CheckIfPossible(client, data, data.fromTile); }

        private static void CheckIfPossible(ServerClient client, AidData data, int targetTile)
        {
            if (!SettlementManager.CheckIfTileIsInUse(targetTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to send an aid to settlement at tile {targetTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(targetTile);
                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    ServerClient target = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

                    if (Master.serverConfig.TemporalAidProtection && !TimeConverter.CheckForEpochTimer(target.aidProtectionTime, 3600000))
                    {
                        data.stepMode = CommonEnumerators.AidStepMode.Recover;
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        target.UpdateAidTime();

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                        target.listener.EnqueuePacket(packet);
                    }
                }

                else
                {
                    data.stepMode = CommonEnumerators.AidStepMode.Recover;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
