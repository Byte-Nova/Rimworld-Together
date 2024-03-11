using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    //Class object for the client connecting into the server. Contains all important data about it

    public class ServerClient
    {
        //Reference to the listener instance of this client
        [NonSerialized] public Listener listener;

        public string username = "Unknown";

        public string uid;

        public string password;

        public string factionName;

        public bool hasFaction;

        public bool isAdmin;

        public bool isBanned;

        [NonSerialized] public ServerClient inVisitWith;

        [NonSerialized] public bool inSafeZone;

        [NonSerialized] public List<string> runningMods = new List<string>();

        [NonSerialized] public List<string> allyPlayers = new List<string>();

        [NonSerialized] public List<string> enemyPlayers = new List<string>();

        public string SavedIP { get; set; }

        public ServerClient(TcpClient tcp)
        {
            if (tcp == null) return;
            else SavedIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        }
    }
}
