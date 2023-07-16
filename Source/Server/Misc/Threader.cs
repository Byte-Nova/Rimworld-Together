using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class Threader
    {
        public enum ServerMode { Start, Heartbeat, Sites }

        public enum ClientMode { Start }

        public static void GenerateServerThread(ServerMode mode)
        {
            if (mode == ServerMode.Start)
            {
                Thread thread = new Thread(new ThreadStart(Network.ReadyServer));
                thread.IsBackground = true;
                thread.Name = "Networking";
                thread.Start();
            }

            else if (mode == ServerMode.Heartbeat)
            {
                Thread thread = new Thread(Network.HearbeatClients);
                thread.IsBackground = true;
                thread.Name = "Heartbeat";
                thread.Start();
            }

            else if (mode == ServerMode.Sites)
            {
                Thread thread = new Thread(SiteManager.StartSiteTicker);
                thread.IsBackground = true;
                thread.Name = "Sites";
                thread.Start();
            }
        }

        public static void GenerateClientThread(ClientMode mode, Client client)
        {
            if (mode == ClientMode.Start)
            {
                Thread thread = new Thread(() => Network.ListenToClient(client));
                thread.IsBackground = true;
                thread.Name = $"Client {client.SavedIP}";
                thread.Start();
            }
        }
    }
}
