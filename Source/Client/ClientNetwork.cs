using GameClient.Core.Preferences;
using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using TCPNetwork;
using TCPNetwork.Server;
using Shared;
using System;
using System.Net.Sockets;
using static Shared.CommonEnumerators;
using System.Threading.Tasks;

namespace GameClient
{
    public class ClientNetwork : Network
    {
        public static ClientNetwork Instance { get; private set; } = null;

        public override Action<PacketHeader, byte[], ServerClient> OnReadPacket { get; set; } = delegate (PacketHeader header, byte[] buffer, ServerClient client)
        {
            MainThreadHandler.Instance.Enqueue(delegate
            {
                MethodGatherer.ClientMethodDictionary[header].Invoke(null, new object[] { buffer });
            });
        };

        public override Action<ServerClient> OnDisconnect { get; set; } = delegate { Instance.Disconnect(); };

        public override Action<object, LogImportanceMode> OnMessage { get; set; } = delegate (object obj, LogImportanceMode mode)
        {
            Printer.Message(obj, mode);
        };

        public override Action<object, LogImportanceMode> OnWarning { get; set; } = delegate (object obj, LogImportanceMode mode)
        {
            Printer.Warning(obj, mode);
        };

        public override Action<object, LogImportanceMode> OnError { get; set; } = delegate (object obj, LogImportanceMode mode)
        {
            Printer.Error(obj, mode);
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
                ConnectionDataHandler.SaveConnectionData(Ip, Port);

                SessionValues.CurrentNetworkState = ClientNetworkState.Connected;

                Printer.Message($"Connected to server");
            }

            else
            {
                RT_Dialog_Wait.Instance.Close();
                RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "The server did not respond in time" });
                RT_Dialog_Base.PushNewDialog(d1);
                Disconnect();
            }
        }

        public bool TryConnect()
        {
            if (SessionValues.CurrentNetworkState != ClientNetworkState.Disconnected) return false;

            try
            {
                SessionValues.CurrentNetworkState = ClientNetworkState.Connecting;

                TcpClient tcpClient = new TcpClient(Ip, int.Parse(Port));

                ClientListener = new Listener(null, tcpClient, OnReadPacket, OnDisconnect,
                    OnMessage, OnWarning, OnError, Listener.ListenerMode.Client);
            }
            catch { return false; }

            return true;
        }

        public void Disconnect()
        {
            Printer.Warning($"Disconnecting from server...", LogImportanceMode.Verbose);

            SessionValues.CurrentNetworkState = ClientNetworkState.Disconnected;

            if (ClientListener != null)
            {
                ClientListener.DestroyConnection();
                ClientListener = null;
            }

            DisconnectionManager.HandleDisconnect();
        }
    }
}