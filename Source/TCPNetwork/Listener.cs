using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using TCPNetwork.Files.Client;
using TCPNetwork.Packets;
using static Shared.CommonEnumerators;
using static Shared.CommonValues;

namespace TCPNetwork
{
    public class Listener
    {
        public enum ListenerMode { Client, Server }

        private ServerClient TargetClient { get; set; } = null;

        public TcpClient Connection { get; set; } = null;

        public NetworkStream Stream { get; set; } = null;

        private Action<PacketHeader, byte[], ServerClient> OnReadPacket { get; set; } = null;

        private Action<bool> OnWritePacket { get; set; } = null;

        public Action<ServerClient> OnDisconnect { get; set; } = null;

        private Action<object, LogImportanceMode> OnMessage { get; set; } = null;

        private Action<object, LogImportanceMode> OnWarning { get; set; } = null;

        private Action<object, LogImportanceMode> OnError { get; set; } = null;

        private ConcurrentQueue<KeyValuePair<byte, byte[]>> PacketQueue { get; set; } = new ConcurrentQueue<KeyValuePair<byte, byte[]>>();

        private bool DisconnectFlag { get; set; } = false;

        private bool IsDisconnecting { get; set; } = false;
        
        public int CurrentKeepAliveTime { get; set; } = 0;

        public static readonly int KeepAliveMaxTime = 30000;

        public static readonly string DefaultParserMethodName = "ParsePacket";

        public static readonly PacketHeader[] IgnoreLogPackets = { PacketHeader.KeepAliveManager };

        public static readonly PacketHeader[] BypassReadyPackets =
        {
            PacketHeader.LoginManager,
            PacketHeader.KeepAliveManager,
            PacketHeader.VersionManager,
            PacketHeader.SaveManager,
            PacketHeader.WorldManager,
            PacketHeader.GlobalDataManager,
            PacketHeader.RecountManager,
            PacketHeader.ChatManager,
            PacketHeader.ConsoleManager,
            PacketHeader.ServerBrowserReachability
        };

        public Listener(ServerClient clientToUse, TcpClient connection, Action<PacketHeader, byte[], ServerClient> onReadPacket, Action<bool> onWritePacket, 
            Action<ServerClient> onConnect, Action<ServerClient> onDisconnect, Action<object, LogImportanceMode> onMessage, 
            Action<object, LogImportanceMode> onWarning, Action<object, LogImportanceMode> onError, ListenerMode mode)
        {
            this.Connection = connection;
            this.TargetClient = clientToUse;
            this.Stream = connection.GetStream();

            this.OnMessage = onMessage;
            this.OnWarning = onWarning;
            this.OnError = onError;

            this.OnReadPacket = onReadPacket;
            this.OnWritePacket = onWritePacket;
            this.OnDisconnect = onDisconnect;

            onConnect.Invoke(clientToUse);

            Task.Run(() => Read());
            Task.Run(() => Write());
            Task.Run(() => SendKAFlag());
            Task.Run(() => CheckKAFlag());
        }

        public void EnqueuePacket(PacketHeader header, object obj)
        {
            if(IsDisconnecting)
                return;
            PacketQueue.Enqueue(new KeyValuePair<byte, byte[]>((byte)header, Serializer.ConvertObjectToBytes(obj)));
        }
        public void EnqueueBytes(PacketHeader header, byte[] bytes)
        {
            if(IsDisconnecting)
                return;
            PacketQueue.Enqueue(new KeyValuePair<byte, byte[]>((byte)header, bytes));
        }

        private void Read()
        {
            try
            {
                byte[] headerBuffer = new byte[sizeof(PacketHeader)];
                byte[] lengthBuffer = new byte[Network.PacketLengthSizeInBytes];
                while (!DisconnectFlag)
                {
                    Thread.Sleep(1);

                    if (Stream.DataAvailable)
                    {
                        // Read packet header
                        
                        Stream.Read(headerBuffer, 0, sizeof(PacketHeader));
                        PacketHeader header = (PacketHeader)headerBuffer[0];

                        // Read packet size
                        Stream.Read(lengthBuffer, 0, Network.PacketLengthSizeInBytes);

                        // Read packet contents
                        var packetBuffer = new byte[BitConverter.ToInt32(lengthBuffer, 0)];
                        ReadFullPacket(packetBuffer);

                        if (!IgnoreLogPackets.Contains(header)) OnMessage($"[Packet] > Received packet {header}", LogImportanceMode.Verbose);
                        else OnMessage($"[Packet] > Received packet {header}", LogImportanceMode.Extreme);

                        try { OnReadPacket(header, packetBuffer, TargetClient); }
                        catch (Exception e) { OnWarning(e, LogImportanceMode.Extreme); }
                    }
                }
            }
            catch (Exception e) { OnWarning(e, LogImportanceMode.Extreme); }

            DisconnectNow();
        }

        private void Write()
        {
            try
            {
                byte[] headerBuffer = new byte[sizeof(PacketHeader)];
                while (!DisconnectFlag)
                {
                    Thread.Sleep(1);

                    OnWritePacket(true);

                    if (PacketQueue.Count > 0)
                    {
                        if (!PacketQueue.TryDequeue(out KeyValuePair<byte, byte[]> packetData)) return;
                        byte[] packetSize = BitConverter.GetBytes(packetData.Value.Length);
                        // Write packet header
                        headerBuffer[0] = packetData.Key;
                        Stream.Write(headerBuffer, 0, sizeof(PacketHeader));

                        // Write packet size
                        Stream.Write(packetSize, 0, packetSize.Length);

                        // Write packet data
                        Stream.Write(packetData.Value, 0, packetData.Value.Length);

                        //Log the packet data
                        if (!IgnoreLogPackets.Contains((PacketHeader)(packetData.Key))) OnMessage($"[Packet] Sent packet > {(PacketHeader)(packetData.Key)}", LogImportanceMode.Verbose);
                        else OnMessage($"[Packet] > Sent packet {(PacketHeader)(packetData.Key)}", LogImportanceMode.Extreme);
                    }

                    if (IsDisconnecting)
                    {
                        DisconnectNow();
                    }
                    
                    OnWritePacket(false);
                }
            }
            catch (Exception e) { OnWarning(e, LogImportanceMode.Extreme); }

            DisconnectNow();
        }

        private void SendKAFlag()
        {
            try
            {
                while (!DisconnectFlag)
                {
                    Thread.Sleep(1000);
                    KeepAliveData keepAliveData = new KeepAliveData();
                    EnqueuePacket(PacketHeader.KeepAliveManager, keepAliveData);
                }
            }
            catch (Exception e) { OnWarning(e, LogImportanceMode.Verbose); }
        }

        private void CheckKAFlag()
        {
            try
            {
                while (!DisconnectFlag)
                {
                    Thread.Sleep(1);

                    if (CurrentKeepAliveTime < KeepAliveMaxTime) CurrentKeepAliveTime++;
                    else break;
                }
            }
            catch (Exception e) { OnWarning(e, LogImportanceMode.Verbose); }

            DisconnectNow();
        }

        private void ReadFullPacket(byte[] content)
        {
            int readBytes = 0;

            try
            {
                while (readBytes < content.Length)
                {
                    int read = Stream.Read(content, readBytes, content.Length - readBytes);
                    if (read == 0) throw new ArgumentOutOfRangeException();
                    readBytes += read;
                }
            }
            catch (Exception e) { OnWarning(e, LogImportanceMode.Verbose); }
        }

        /// <summary>
        /// Empties the packet buffer first
        /// </summary>
        public void DisconnectSmooth()
        {
            IsDisconnecting = true;
        }
        
        /// <summary>
        /// Disconnects instantly, all packets not sent yet are lost
        /// </summary>
        public void DisconnectNow()
        {
            if (DisconnectFlag) return;
            else
            {
                DisconnectFlag = true;
                Connection.Dispose();
                Stream.Dispose();

                this.OnDisconnect(TargetClient);
            }
        }
    }
}