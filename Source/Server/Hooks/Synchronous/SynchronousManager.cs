using GameServer.Core;
using GameServer.Hooks.TCPNetwork;
using GameServer.Managers;
using Shared;
using Shared.Files;
using Shared.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using TCPNetwork.Files.Client;
using TCPNetwork.Packets;
using static Shared.CommonEnumerators;

namespace GameServer.Hooks.Synchronous
{
    public static class SynchronousManager
    {
        [HandlesPacket(PacketHeader.SynchronousManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            SynchronousData data = Serializer.ConvertBytesToObject<SynchronousData>(bytes);

            switch (data._stepMode)
            {
                case SynchronousData.StepMode.Ask:
                    TryStartSynchronousSession(client, data);
                    break;

                case SynchronousData.StepMode.Accept:
                    AcceptSynchronousSession(client, data);
                    break;

                case SynchronousData.StepMode.Reject:
                    RejectSynchronousSession(client, data);
                    break;

                case SynchronousData.StepMode.Start:
                    StartSynchronousSession(client, data);
                    break;
            }
        }

        private static void TryStartSynchronousSession(ServerClient client, SynchronousData data)
        {
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(data._toTile);
            ServerClient toFind = ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username);

            if (toFind == null) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                SynchronousData _ = new SynchronousData();
                _._stepMode = SynchronousData.StepMode.Ask;
                _._fromTile = SettlementManager.GetSettlementFileFromUsername(client.UserFile.Username).Tile;
                _._toTile = data._toTile;
                _._party = data._party;

                toFind.Listener.EnqueuePacket(PacketHeader.SynchronousManager, _);
            }
        }

        private static void AcceptSynchronousSession(ServerClient client, SynchronousData data)
        {
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(data._toTile);
            ServerClient toFind = ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username);

            SynchronousData _ = new SynchronousData();
            _._stepMode = SynchronousData.StepMode.Accept;
            _._fromTile = data._fromTile;
            _._toTile = data._toTile;
            _._contents = MapManager.GetMapFromTile(data._fromTile);
            _._party = data._party;

            client.SynchronousClient = toFind;
            toFind.SynchronousClient = client;

            toFind.Listener.EnqueuePacket(PacketHeader.SynchronousManager, _);
        }

        private static void RejectSynchronousSession(ServerClient client, SynchronousData data)
        {
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(data._toTile);
            ServerClient toFind = ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username);

            SynchronousData _ = new SynchronousData();
            _._stepMode = SynchronousData.StepMode.Reject;
            _._fromTile = data._fromTile;
            _._toTile = data._toTile;

            toFind.Listener.EnqueuePacket(PacketHeader.SynchronousManager, _);
        }

        private static void StartSynchronousSession(ServerClient client, SynchronousData data)
        {
            SynchronousData _ = new SynchronousData();
            _._stepMode = SynchronousData.StepMode.Start;

            client.SynchronousClient.Listener.EnqueuePacket(PacketHeader.SynchronousManager, _);
        }

        [HandlesPacket(PacketHeader.SPlayerDraft)]
        private static void SPlayerDraft(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.Listener.EnqueuePacket(header, bytes);
            client.SynchronousClient.Listener.EnqueuePacket(header, bytes);
        }

        [HandlesPacket(PacketHeader.SPlayerWeather)]
        private static void SPlayerWeather(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.Listener.EnqueuePacket(header, bytes);
            client.SynchronousClient.Listener.EnqueuePacket(header, bytes);
        }

        [HandlesPacket(PacketHeader.SPlayerMentalState)]
        private static void SPlayerMentalState(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.Listener.EnqueuePacket(header, bytes);
            client.SynchronousClient.Listener.EnqueuePacket(header, bytes);
        }

        [HandlesPacket(PacketHeader.SPlayerGameSpeed)]
        private static void SPlayerGameSpeed(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.Listener.EnqueuePacket(header, bytes);
            client.SynchronousClient.Listener.EnqueuePacket(header, bytes);
        }

        [HandlesPacket(PacketHeader.SPlayerJob)]
        private static void SPlayerJob(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.Listener.EnqueuePacket(header, bytes);
            client.SynchronousClient.Listener.EnqueuePacket(header, bytes);
        }

        [HandlesPacket(PacketHeader.SPlayerHediff)]
        private static void SPlayerHediff(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.Listener.EnqueuePacket(header, bytes);
            client.SynchronousClient.Listener.EnqueuePacket(header, bytes);
        }

        [HandlesPacket(PacketHeader.SPlayerPosition)]
        private static void SPlayerPosition(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.SynchronousClient.Listener.EnqueuePacket(header, bytes);
        }
    }
}
