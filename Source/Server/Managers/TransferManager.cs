using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class TransferManager
    {
        public static void ParseTransferPacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableTrading)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            switch (transferData._stepMode)
            {
                case TransferStepMode.TradeRequest:
                    TransferThings(client, transferData);
                    break;

                case TransferStepMode.TradeAccept:
                    //Nothing goes here
                    break;

                case TransferStepMode.TradeReject:
                    RejectTransfer(client, packet);
                    break;

                case TransferStepMode.TradeReRequest:
                    TransferThingsRebound(client, packet);
                    break;

                case TransferStepMode.TradeReAccept:
                    AcceptReboundTransfer(client, packet);
                    break;

                case TransferStepMode.TradeReReject:
                    RejectReboundTransfer(client, packet);
                    break;
            }
        }

        public static void TransferThings(ServerClient client, TransferData transferData)
        {
            if (!SettlementManager.CheckIfTileIsInUse(transferData._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send items to a settlement at tile {transferData._toTile}, but no settlement could be found");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._toTile);

                if (!UserManagerHelper.CheckIfUserIsConnected(settlement.Owner))
                {
                    if (transferData._transferMode == TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        transferData._stepMode = TransferStepMode.Recover;
                        Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }
                }

                else
                {
                    if (transferData._transferMode == TransferMode.Gift)
                    {
                        transferData._stepMode = TransferStepMode.TradeAccept;
                        Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    else if (transferData._transferMode == TransferMode.Pod)
                    {
                        transferData._stepMode = TransferStepMode.TradeAccept;
                        Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    transferData._stepMode = TransferStepMode.TradeRequest;
                    string[] contents2 = new string[] { Serializer.SerializeToString(transferData) };
                    Packet rPacket2 = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                    UserManagerHelper.GetConnectedClientFromUsername(settlement.Owner).listener.EnqueuePacket(rPacket2);
                }
            }
        }

        public static void RejectTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._fromTile);
            if (!UserManagerHelper.CheckIfUserIsConnected(settlement.Owner))
            {
                transferData._stepMode = TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReject;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                UserManagerHelper.GetConnectedClientFromUsername(settlement.Owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void TransferThingsRebound(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._toTile);
            if (!UserManagerHelper.CheckIfUserIsConnected(settlement.Owner))
            {
                transferData._stepMode = TransferStepMode.TradeReReject;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReRequest;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                UserManagerHelper.GetConnectedClientFromUsername(settlement.Owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void AcceptReboundTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);
            
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._fromTile);
            if (!UserManagerHelper.CheckIfUserIsConnected(settlement.Owner))
            {
                transferData._stepMode = TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReAccept;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                UserManagerHelper.GetConnectedClientFromUsername(settlement.Owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void RejectReboundTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._fromTile);
            if (!UserManagerHelper.CheckIfUserIsConnected(settlement.Owner))
            {
                transferData._stepMode = TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReReject;
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), transferData);
                UserManagerHelper.GetConnectedClientFromUsername(settlement.Owner).listener.EnqueuePacket(rPacket);
            }
        }
    }
}
