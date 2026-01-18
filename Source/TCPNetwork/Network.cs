using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPNetwork.Files.Client;
using static Shared.CommonEnumerators;

namespace TCPNetwork
{
    public class Network
    {
        public const int PacketLengthSizeInBytes = sizeof(int);

        public static string Ip { get; set; } = string.Empty;

        public static string Port { get; set; } = string.Empty;

        public virtual Action<PacketHeader, byte[], ServerClient> OnReadPacket { get; set; }

        public virtual Action<bool> OnWritePacket { get; set; }

        public virtual Action<ServerClient> OnConnect { get; set; }

        public virtual Action<ServerClient> OnDisconnect { get; set; }

        public Listener ClientListener { get; set; } = null;

        public TcpListener ServerListener { get; set; }

        public List<ServerClient> ServerClients { get; private set; } = new List<ServerClient>();
    }
}
