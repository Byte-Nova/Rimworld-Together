using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
                    PacketHandler.HandlePacket(receivedPacket);
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
