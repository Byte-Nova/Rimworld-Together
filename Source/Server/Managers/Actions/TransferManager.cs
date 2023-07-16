using GameServer.Managers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class TransferManager
    {
        public enum TransferMode { Gift, Trade }

        public enum TransferStepMode { TradeRequest, TradeAccept, TradeReject, TradeReRequest, TradeReAccept, TradeReReject, Recover }

        public static void ParseTransferPacket(Client client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = Serializer.SerializeFromString<TransferManifestJSON>(packet.contents[0]);

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

        public static void TransferThings(Client client, TransferManifestJSON transferManifestJSON)
        {
            if (!SettlementManager.CheckIfTileIsInUse(transferManifestJSON.toTile)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.toTile);
                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                    string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                    Packet rPacket = new Packet("TransferPacket", contents);
                    Network.SendData(client, rPacket);
                }

                else
                {
                    if (int.Parse(transferManifestJSON.transferMode) == (int)TransferMode.Gift)
                    {
                        transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeAccept).ToString();
                        string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                        Packet rPacket = new Packet("TransferPacket", contents);
                        Network.SendData(client, rPacket);
                    }

                    transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeRequest).ToString();
                    string[] contents2 = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                    Packet rPacket2 = new Packet("TransferPacket", contents2);
                    Network.SendData(UserManager.GetConnectedClientFromUsername(settlement.owner), rPacket2);
                }
            }
        }

        public static void RejectTransfer(Client client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = Serializer.SerializeFromString<TransferManifestJSON>(packet.contents[0]);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(client, rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReject).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(UserManager.GetConnectedClientFromUsername(settlement.owner), rPacket);
            }
        }

        public static void TransferThingsRebound(Client client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = Serializer.SerializeFromString<TransferManifestJSON>(packet.contents[0]);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.toTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReReject).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(client, rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReRequest).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(UserManager.GetConnectedClientFromUsername(settlement.owner), rPacket);
            }
        }

        public static void AcceptReboundTransfer(Client client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = Serializer.SerializeFromString<TransferManifestJSON>(packet.contents[0]);
            
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(client, rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReAccept).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(UserManager.GetConnectedClientFromUsername(settlement.owner), rPacket);
            }
        }

        public static void RejectReboundTransfer(Client client, Packet packet)
        {
            TransferManifestJSON transferManifestJSON = Serializer.SerializeFromString<TransferManifestJSON>(packet.contents[0]);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferManifestJSON.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.Recover).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(client, rPacket);
            }

            else
            {
                transferManifestJSON.transferStepMode = ((int)TransferStepMode.TradeReReject).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(transferManifestJSON) };
                Packet rPacket = new Packet("TransferPacket", contents);
                Network.SendData(UserManager.GetConnectedClientFromUsername(settlement.owner), rPacket);
            }
        }
    }
}
