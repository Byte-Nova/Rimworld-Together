using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class TransferManager
    {
        public static void ParseTransferPacket(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            switch (transferData.transferStepMode)
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
            if (!SettlementManager.CheckIfTileIsInUse(transferData.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send items to a settlement at tile {transferData.toTile}, but no settlement could be found");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.toTile);

                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    if (transferData.transferMode == TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        transferData.transferStepMode = TransferStepMode.Recover;
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }
                }

                else
                {
                    if (transferData.transferMode == TransferMode.Gift)
                    {
                        transferData.transferStepMode = TransferStepMode.TradeAccept;
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    else if (transferData.transferMode == TransferMode.Pod)
                    {
                        transferData.transferStepMode = TransferStepMode.TradeAccept;
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    transferData.transferStepMode = TransferStepMode.TradeRequest;
                    string[] contents2 = new string[] { Serializer.SerializeToString(transferData) };
                    Packet rPacket2 = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                    UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket2);
                }
            }
        }

        public static void RejectTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = TransferStepMode.TradeReject;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void TransferThingsRebound(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.toTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = TransferStepMode.TradeReReject;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = TransferStepMode.TradeReRequest;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void AcceptReboundTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);
            
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = TransferStepMode.TradeReAccept;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void RejectReboundTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = TransferStepMode.TradeReReject;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }
    }
}
