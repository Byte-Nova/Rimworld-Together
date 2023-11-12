using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using System;
using System.IO;
using System.Net.Sockets;
using Verse;

namespace RimworldTogether.GameClient.Network.Listener
{
    public class ServerListener
    {
        public TcpClient connection;
        public NetworkStream ns;
        public StreamWriter sw;
        public StreamReader sr;

        public ServerListener(TcpClient connection)
        {
            this.connection = connection;
            ns = connection.GetStream();
            sw = new StreamWriter(ns);
            sr = new StreamReader(ns);
        }

        public void ListenToServer()
        {
            try
            {
                DialogShortcuts.ShowLoginOrRegisterDialogs();

                while (true)
                {
                    string data = sr.ReadLine();
                    Packet receivedPacket = Serializer.SerializeStringToPacket(data);

                    Action toDo = delegate { PacketHandler.HandlePacket(receivedPacket); };
                    Main.threadDispatcher.Enqueue(toDo);
                }
            }

            catch (Exception e)
            {
                Log.Message(e.ToString());
                Network.DisconnectFromServer();
            }
        }

        public void SendData(Packet packet)
        {
            sw.WriteLine(Serializer.SerializePacketToString(packet));
            sw.Flush();
        }
    }
}
