using Shared;
using Shared.Files;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TCPNetwork.Server
{
    //Class object for the client connecting into the server. Contains all important data about it

    [Serializable]
    public class ServerClient
    {
        //Contains a reference to the user file of the client

        public UserFile UserFile { get; private set; } = null;

        //Variables

        [NonSerialized] public Listener Listener;

        private string UsersPath { get; set; } = string.Empty;

        public ServerClient(TcpClient tcp, string path)
        {
            UsersPath = path;
            UserFile = new UserFile(UsersPath);

            if (tcp == null) return;
            else UserFile.SavedIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        }

        public void LoadUserFromFile() 
        { 
            UserFile = GetUserFile(this);
            UserFile.UsersPath = UsersPath;
        }

        private UserFile GetUserFile(ServerClient client)
        {
            string[] userFiles = Directory.GetFiles(UsersPath);

            foreach (string userFile in userFiles)
            {
                if (!userFile.EndsWith(UserFile.fileExtension)) continue;

                UserFile file = Serializer.SerializeFromFile<UserFile>(userFile);
                if (file.Uid == client.UserFile.Uid) return file;
            }

            return null;
        }
    }
}