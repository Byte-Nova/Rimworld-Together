using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Network;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Verse;

namespace RimworldTogether.GameClient.Network.Listener
{
    public class ServerListener
    {
        public TcpClient connection;
        public NetworkStream ns;
        public StreamWriter sw;
        public StreamReader sr;

        public DownloadManager downloadManager;
        public UploadManager uploadManager;

        private bool isBusy;
        public bool disconnectFlag;

        public ServerListener(TcpClient connection)
        {
            Main.threadDispatcher.Enqueue(DialogShortcuts.ShowLoginOrRegisterDialogs);

            this.connection = connection;
            ns = connection.GetStream();
            sw = new StreamWriter(ns);
            sr = new StreamReader(ns);
        }

        public void SendData(Packet packet)
        {
            while (isBusy) Thread.Sleep(100);

            try
            {
                isBusy = true;

                sw.WriteLine(Serializer.SerializePacketToString(packet));
                sw.Flush();

                isBusy = false;
            }
            catch { disconnectFlag = true; }
        }

        public void ListenToServer()
        {
            while (Network.isConnectedToServer)
            {
                try
                {
                    string data = sr.ReadLine();
                    Packet receivedPacket = Serializer.SerializeStringToPacket(data);

                    Action toDo = delegate { PacketHandler.HandlePacket(receivedPacket); };
                    Main.threadDispatcher.Enqueue(toDo);
                }

                catch (Exception e)
                {
                    Log.Error($"[Rimworld Together] > {e}");

                    disconnectFlag = true;
                }
            }
        }

        public void CheckForConnectionHealth()
        {
            while (Network.isConnectedToServer)
            {
                Thread.Sleep(100);

                if (disconnectFlag) Network.DisconnectFromServer();
            }
        }

        public void SendKAFlag()
        {
            while (Network.isConnectedToServer)
            {
                Thread.Sleep(1000);

                KeepAliveJSON keepAliveJSON = new KeepAliveJSON();
                Packet packet = Packet.CreatePacketFromJSON("KeepAlivePacket", keepAliveJSON);
                SendData(packet);
            }
        }
    }
}
