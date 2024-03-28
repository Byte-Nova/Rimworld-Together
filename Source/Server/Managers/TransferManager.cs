using Shared;

namespace GameServer
{
    public static class TransferManager
    {
        public static void ParseTransferPacket(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(transferManifestJSON.transferStepMode))
            {
                case (int)CommonEnumerators.TransferStepMode.TradeRequest:
                    TransferThings(client, transferManifestJSON);
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

        public static void TransferThings(ServerClient client, TransferManifestJSON transferManifestJSON)
        {
            if (!SettlementManager.CheckIfTileIsInUse(transferManifestJSON.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to send items to a settlement at tile {transferManifestJSON.toTile}, but no settlement could be found");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.toTile);

                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    if (int.Parse(transferManifestJSON.transferMode) == (int)CommonEnumerators.TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                        client.listener.EnqueuePacket(rPacket);
                    }
                }

                else
                {
                    if (int.Parse(transferManifestJSON.transferMode) == (int)CommonEnumerators.TransferMode.Gift)
                    {
                        transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    else if (int.Parse(transferManifestJSON.transferMode) == (int)CommonEnumerators.TransferMode.Pod)
                    {
                        transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeRequest).ToString();
                    string[] contents2 = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                    Packet rPacket2 = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                    UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket2);
                }
            }
        }

        public static void RejectTransfer(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void TransferThingsRebound(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.toTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReRequest).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void AcceptReboundTransfer(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)Serializer.ConvertBytesToObject(packet.contents);
            
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReAccept).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void RejectReboundTransfer(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }
    }
}
