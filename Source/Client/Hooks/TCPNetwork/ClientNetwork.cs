using GameClient.Core.Configs;
using GameClient.Dialogs;
using GameClient.Files;
using GameClient.Managers;
using GameClient.Misc;
using Shared;
using Shared.Misc;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using TCPNetwork;
using TCPNetwork.Files.Client;
using TCPNetwork.Misc;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Hooks.TCPNetwork
{
    public class ClientNetwork : Network
    {
        public static ClientNetwork Instance { get; private set; } = null;

        public override Action<PacketHeader, byte[], ServerClient> OnReadPacket { get; set; } = delegate (PacketHeader header, byte[] buffer, ServerClient client)
        {
            Thread.Sleep(250 * (int)ModConfigGetter.CurrentSimulatedLag);

            if (!SessionHandler.IsReadyToPlay && !Listener.BypassReadyPackets.Contains(header)) return;
            else
            {
                MainThreadHandler.Instance.Enqueue(delegate
                {
                    PacketCache.ClientMethodDictionary[header](buffer);
                });
            }
        };

        public override Action<bool> OnWritePacket { get; set; } = delegate (bool mode) { };

        public override Action<ServerClient> OnConnect { get; set; } = delegate
        {
            MainThreadHandler.Instance.Enqueue(delegate { HarmonyHandler.EnableMainPatches(); });
        };

        public override Action<ServerClient> OnDisconnect { get; set; } = delegate 
        {
            MainThreadHandler.Instance.Enqueue(delegate
            {
                DisconnectionManager.HandleDisconnect();
                MainThreadHandler.Instance.DoOnEndMethods();
                Printer.Warning($"Disconnecting from server", LogImportanceMode.Verbose);
            });
        };

        public ClientNetwork()
        {
            Instance = this;

            StartConnection();
        }

        public void StartConnection()
        {
            if (TryConnect())
            {
                SessionHandler.CurrentNetworkState = ClientNetworkState.Connected;

                PersistentSettings settings = PersistentSettings.Load();
                settings.ServerSettings.Set(Ip, Port);
                settings.Save();

                Printer.Message($"Connected to server");
            }

            else
            {
                RT_Dialog_Wait.Instance.Close();
                RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "The server did not respond in time" });
                RT_Dialog_Base.PushNewDialog(d1);
                OnDisconnect.Invoke(null);
            }
        }

        public bool TryConnect()
        {
            if (SessionHandler.CurrentNetworkState != ClientNetworkState.Disconnected) return false;

            try
            {
                TcpClient tcpClient = new TcpClient(Ip, int.Parse(Port));

                ClientListener = new Listener(null, tcpClient, OnReadPacket, OnWritePacket, OnConnect, OnDisconnect, Listener.ListenerMode.Client);
            }
            catch { return false; }

            return true;
        }
    }
}