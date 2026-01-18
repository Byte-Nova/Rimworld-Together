using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using static TCPNetwork.Packets.TransferData;
using TCPNetwork.Files.Client;
using GameServer.Hooks.TCPNetwork;

namespace GameServer.Managers
{

    public static class TransferManager
    {
        [HandlesPacket(PacketHeader.TransferManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!Master.ActionConfigs.EnableTrading)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            TransferData data = Serializer.ConvertBytesToObject<TransferData>(bytes);

            switch (data._stepMode)
            {
                case TransferStepMode.TradeRequest:
                    TransferThings(client, data);
                    break;

                case TransferStepMode.TradeReject:
                    RejectTransfer(client, bytes);
                    break;

                case TransferStepMode.TradeReRequest:
                    TransferThingsRebound(client, bytes);
                    break;

                case TransferStepMode.TradeReAccept:
                    AcceptReboundTransfer(client, bytes);
                    break;

                case TransferStepMode.TradeReReject:
                    RejectReboundTransfer(client, bytes);
                    break;
            }
        }

        public static void TransferThings(ServerClient client, TransferData transferData)
        {
            if (!SettlementManager.CheckIfTileIsInUse(transferData._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Username} attempted to send items to a settlement at tile {transferData._toTile}, but no settlement could be found");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._toTile);

                if (!UserManagerH.CheckIfUserIsConnected(settlement.Username))
                {
                    if (transferData._transferMode == TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        transferData._stepMode = TransferStepMode.Recover;
                        client.Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
                    }
                }

                else
                {
                    if (transferData._transferMode == TransferMode.Gift)
                    {
                        transferData._stepMode = TransferStepMode.TradeAccept;
                        client.Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
                    }

                    else if (transferData._transferMode == TransferMode.Pod)
                    {
                        transferData._stepMode = TransferStepMode.TradeAccept;
                        client.Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
                    }

                    transferData._stepMode = TransferStepMode.TradeRequest;
                    ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username).Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
                }
            }
        }

        public static void RejectTransfer(ServerClient client, byte[] bytes)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(bytes);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._fromTile);
            if (!UserManagerH.CheckIfUserIsConnected(settlement.Username))
            {
                transferData._stepMode = TransferStepMode.Recover;
                client.Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReject;
                ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username).Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }
        }

        public static void TransferThingsRebound(ServerClient client, byte[] bytes)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(bytes);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._toTile);
            if (!UserManagerH.CheckIfUserIsConnected(settlement.Username))
            {
                transferData._stepMode = TransferStepMode.TradeReReject;
                client.Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReRequest;
                ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username).Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }
        }

        public static void AcceptReboundTransfer(ServerClient client, byte[] bytes)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(bytes);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._fromTile);
            if (!UserManagerH.CheckIfUserIsConnected(settlement.Username))
            {
                transferData._stepMode = TransferStepMode.Recover;
                client.Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReAccept;
                ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username).Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }
        }

        public static void RejectReboundTransfer(ServerClient client, byte[] bytes)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(bytes);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(transferData._fromTile);
            if (!UserManagerH.CheckIfUserIsConnected(settlement.Username))
            {
                transferData._stepMode = TransferStepMode.Recover;
                client.Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }

            else
            {
                transferData._stepMode = TransferStepMode.TradeReReject;
                ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username).Listener.EnqueuePacket(PacketHeader.TransferManager, transferData);
            }
        }
    }
}
