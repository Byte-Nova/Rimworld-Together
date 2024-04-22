using Shared;

namespace GameServer
{
    public static class TransferManager
    {
        public static void ParseTransferPacket(ServerClient client, Packet packet)
        {
            TransferData transferData = (TransferData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(transferData.transferStepMode))
            {
                case (int)CommonEnumerators.TransferStepMode.TradeRequest:
                    TransferThings(client, transferData);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeAccept:
                    //Nothing goes here
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReject:
                    RejectTransfer(client, packet);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReRequest:
                    TransferThingsRebound(client, packet);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReAccept:
                    AcceptReboundTransfer(client, packet);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReReject:
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
                    if (int.Parse(transferData.transferMode) == (int)CommonEnumerators.TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }
                }

                else
                {
                    if (int.Parse(transferData.transferMode) == (int)CommonEnumerators.TransferMode.Gift)
                    {
                        transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    else if (int.Parse(transferData.transferMode) == (int)CommonEnumerators.TransferMode.Pod)
                    {
                        transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeRequest).ToString();
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
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReject).ToString();
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
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReRequest).ToString();
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
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReAccept).ToString();
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
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferData.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }
    }
}
