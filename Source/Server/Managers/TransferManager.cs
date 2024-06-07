using Shared;

namespace GameServer
{
    public static class TransferManager
    {
        public static void ParseTransferPacket(ServerClient client, Packet packet)
        {
            TransferData transferData = (TransferData)Serializer.ConvertBytesToObject(packet.contents);

            switch (transferData.transferStepMode)
            {
                case CommonEnumerators.TransferStepMode.TradeRequest:
                    TransferThings(client, transferData);
                    break;

                case CommonEnumerators.TransferStepMode.TradeAccept:
                    //Nothing goes here
                    break;

                case CommonEnumerators.TransferStepMode.TradeReject:
                    RejectTransfer(client, packet);
                    break;

                case CommonEnumerators.TransferStepMode.TradeReRequest:
                    TransferThingsRebound(client, packet);
                    break;

                case CommonEnumerators.TransferStepMode.TradeReAccept:
                    AcceptReboundTransfer(client, packet);
                    break;

                case CommonEnumerators.TransferStepMode.TradeReReject:
                    RejectReboundTransfer(client, packet);
                    break;
            }
        }

        public static void TransferThings(ServerClient client, TransferData transferData)
        {
            if (!SettlementManager.CheckIfTileIsInUse(transferData.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to send items to a settlement at tile {transferData.toTile}, but no settlement could be found");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.toTile);

                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    if (transferData.transferMode == CommonEnumerators.TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        transferData.transferStepMode = CommonEnumerators.TransferStepMode.Recover;
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }
                }

                else
                {
                    if (transferData.transferMode == CommonEnumerators.TransferMode.Gift)
                    {
                        transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeAccept;
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    else if (transferData.transferMode == CommonEnumerators.TransferMode.Pod)
                    {
                        transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeAccept;
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeRequest;
                    string[] contents2 = new string[] { Serializer.SerializeToString(transferData) };
                    Packet rPacket2 = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                    UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket2);
                }
            }
        }

        public static void RejectTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = (TransferData)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeReject;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void TransferThingsRebound(ServerClient client, Packet packet)
        {
            TransferData transferData = (TransferData)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.toTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeReReject;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeReRequest;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void AcceptReboundTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = (TransferData)Serializer.ConvertBytesToObject(packet.contents);
            
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeReAccept;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void RejectReboundTransfer(ServerClient client, Packet packet)
        {
            TransferData transferData = (TransferData)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.Recover;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = CommonEnumerators.TransferStepMode.TradeReReject;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }
    }
}
