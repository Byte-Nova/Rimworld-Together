using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using System.Diagnostics;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class TransferManager
    {
        public enum TransferMode { Gift, Trade, Rebound, Pod }

        public enum TransferStepMode { TradeRequest, TradeAccept, TradeReject, TradeReRequest, TradeReAccept, TradeReReject, Recover, Pod }

        public static void ParseTransferPacket(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(transferManifestJSON.transferStepMode))
            {
                case (int)TransferStepMode.TradeRequest:
                    TransferThings(client, transferManifestJSON);
                    break;

                case (int)TransferStepMode.TradeAccept:
                    //Nothing goes here
                    break;

                case (int)TransferStepMode.TradeReject:
                    RejectTransfer(client, packet);
                    break;

                case (int)TransferStepMode.TradeReRequest:
                    TransferThingsRebound(client, packet);
                    break;

                case (int)TransferStepMode.TradeReAccept:
                    AcceptReboundTransfer(client, packet);
                    break;

                case (int)TransferStepMode.TradeReReject:
                    RejectReboundTransfer(client, packet);
                    break;
            }
        }

        public static void TransferThings(ServerClient client, TransferManifestJSON transferManifestJSON)
        {
            if (!SettlementManager.CheckIfTileIsInUse(transferManifestJSON.toTile)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.toTile);

                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    if (int.Parse(transferManifestJSON.transferMode) == (int)TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                        client.clientListener.SendData(rPacket);
                    }
                }

                else
                {
                    if (int.Parse(transferManifestJSON.transferMode) == (int)TransferMode.Gift)
                    {
                        transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                        client.clientListener.SendData(rPacket);
                    }

                    else if (int.Parse(transferManifestJSON.transferMode) == (int)TransferMode.Pod)
                    {
                        transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                        client.clientListener.SendData(rPacket);
                    }

                    transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeRequest).ToString();
                    string[] contents2 = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                    Packet rPacket2 = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                    UserManager.GetConnectedClientFromUsername(settlement.owner).clientListener.SendData(rPacket2);
                }
            }
        }

        public static void RejectTransfer(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                client.clientListener.SendData(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).clientListener.SendData(rPacket);
            }
        }

        public static void TransferThingsRebound(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.toTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                client.clientListener.SendData(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReRequest).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).clientListener.SendData(rPacket);
            }
        }

        public static void AcceptReboundTransfer(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                client.clientListener.SendData(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReAccept).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).clientListener.SendData(rPacket);
            }
        }

        public static void RejectReboundTransfer(ServerClient client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = (TransferManifestJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                client.clientListener.SendData(rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON("TransferPacket", transferManifestJSON);
                UserManager.GetConnectedClientFromUsername(settlement.owner).clientListener.SendData(rPacket);
            }
        }
    }
}
