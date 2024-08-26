using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    //Class object for the client connecting into the server. Contains all important data about it

    [Serializable]
    public class ServerClient
    {
        public UserFile userFile = new UserFile();

        [NonSerialized] public Listener listener;

        [NonSerialized] public ServerClient InVisitWith;

        public ServerClient(TcpClient tcp)
        {
            if (tcp == null) return;
            else userFile.SavedIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        }

        public void LoadFromUserFile() { userFile = UserManagerHelper.GetUserFile(this); }
    }
}
