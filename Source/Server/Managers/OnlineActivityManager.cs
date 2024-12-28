using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OnlineActivityManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableOnlineActivities)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            OnlineActivityData data = Serializer.ConvertBytesToObject<OnlineActivityData>(packet.contents);

            switch (data._stepMode)
            {
                case OnlineActivityStepMode.Request:
                    RequestActivity(client, data);
                    break;

                case OnlineActivityStepMode.Accept:
                    AcceptActivity(client, data);
                    break;

                case OnlineActivityStepMode.Reject:
                    RejectActivity(client, data);
                    break;

                case OnlineActivityStepMode.Ready:
                    ReadyActivity(client, data);
                    break;

                case OnlineActivityStepMode.Stop:
                    StopActivity(client);
                    break;

                case OnlineActivityStepMode.Buffer:
                    SendActions(client, data);
                    break;
            }
        }

        private static void RequestActivity(ServerClient client, OnlineActivityData data)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._toTile);
            if (settlementFile == null) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} tried to engage with settlement at tile {data._toTile}, but no settlement could be found");
            else
            {
                ServerClient toGet = NetworkHelper.GetConnectedClientFromUsername(settlementFile.Owner);
                if (toGet == null)
                {
                    data._stepMode = OnlineActivityStepMode.Unavailable;
                    Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    if (toGet.activityPartner != null)
                    {
                        data._stepMode = OnlineActivityStepMode.Unavailable;
                        Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        data._engagerName = client.userFile.Username;
                        Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                        toGet.listener.EnqueuePacket(packet);
                    }
                }
            }
        }

        private static void AcceptActivity(ServerClient client, OnlineActivityData data)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._fromTile);
            if (settlementFile == null) return;
            else
            {
                ServerClient toGet = NetworkHelper.GetConnectedClientFromUsername(settlementFile.Owner);
                if (toGet == null) return;
                else
                {
                    client.activityPartner = toGet;
                    toGet.activityPartner = client;

                    Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                    toGet.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void RejectActivity(ServerClient client, OnlineActivityData data)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._fromTile);
            if (settlementFile == null) return;
            else
            {
                ServerClient toGet = NetworkHelper.GetConnectedClientFromUsername(settlementFile.Owner);
                if (toGet == null) return;
                else
                {
                    Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                    toGet.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void ReadyActivity(ServerClient client, OnlineActivityData data)
        {
            ServerClient toSendTo = client.activityPartner;
            Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
            toSendTo.listener.EnqueuePacket(packet);
        }

        private static void SendActions(ServerClient client, OnlineActivityData data)
        {
            if (client.activityPartner == null)
            {
                data._stepMode = OnlineActivityStepMode.Stop;
                Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                client.activityPartner.listener.EnqueuePacket(packet);
            }
        }

        public static void StopActivity(ServerClient client)
        {
            OnlineActivityData data = new OnlineActivityData();
            data._stepMode = OnlineActivityStepMode.Stop;

            Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);

            if (client.activityPartner != null)
            {
                client.activityPartner.listener.EnqueuePacket(packet);
                client.activityPartner.activityPartner = null;
            }

            client.listener.EnqueuePacket(packet);
            client.activityPartner = null;
        }
    }
}
