using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    //Class object for the client connecting into the server. Contains all important data about it

    [Serializable]
    public class ServerClient
    {
        //Contains a reference to the user file of the client

        public UserFile userFile = new UserFile();

        //Variables

        [NonSerialized] public Listener listener;

        [NonSerialized] public ServerClient activityPartner;

        public ServerClient(TcpClient tcp)
        {
            if (tcp == null) return;
            else userFile.SavedIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        }

        public void LoadUserFromFile() { userFile = UserManagerHelper.GetUserFile(this); }
    }
}
