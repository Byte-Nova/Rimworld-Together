using RimworldTogether.GameServer.Network.Listener;
using Shared.Network;
using System.Net;
using System.Net.Sockets;

namespace RimworldTogether.GameServer.Network
{
    public class ServerClient
    {
        [NonSerialized] public TcpClient tcp;
        [NonSerialized] public NetworkStream networkStream;
        [NonSerialized] public StreamWriter streamWriter;
        [NonSerialized] public StreamReader streamReader;

        [NonSerialized] public ClientListener clientListener;
        [NonSerialized] public UploadManager uploadManager;
        [NonSerialized] public DownloadManager downloadManager;

        [NonSerialized] public bool disconnectFlag;
        [NonSerialized] public bool KAFlag;

        public string uid;

        public string username = "Unknown";

        public string password;

        public string factionName;

        public bool hasFaction;

        public bool isAdmin;

        public bool isBanned;

        [NonSerialized] public ServerClient inVisitWith;

        [NonSerialized] public bool isBusy;

        [NonSerialized] public bool inSafeZone;

        [NonSerialized] public List<string> runningMods = new List<string>();

        [NonSerialized] public List<string> allyPlayers = new List<string>();

        [NonSerialized] public List<string> enemyPlayers = new List<string>();

        public string SavedIP { get; set; }

        public ServerClient(TcpClient tcp)
        {
            if (tcp == null) return;
            else
            {
                this.tcp = tcp;
                networkStream = tcp.GetStream();
                streamWriter = new StreamWriter(networkStream);
                streamReader = new StreamReader(networkStream);

                SavedIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
            }
        }
    }
}
