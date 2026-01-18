using Shared;
using Shared.Files;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TCPNetwork.Files.Client
{
    public class ServerClient
    {
        public string CurrentIP { get; set; } = string.Empty;

        public UserFile UserFile { get; set; } = null;

        public Listener Listener { get; set; } = null;

        public ServerClient SynchronousClient { get; set; } = null;

        public ServerClient(TcpClient tcp)
        {
            if (tcp == null) return;
            else CurrentIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        }

        public void LoadUserFromFile(ServerClient client) 
        { 
            string[] userFiles = Directory.GetFiles(CommonValues.ServerUsersPath);

            foreach (string userFile in userFiles)
            {
                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Username == client.UserFile.Username)
                {
                    UserFile = file;
                    UserFile.UpdateIP(CurrentIP);
                    break;
                }
            }
        }
    }
}