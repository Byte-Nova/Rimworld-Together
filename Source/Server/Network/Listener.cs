using Shared;
using System.Net.Sockets;

namespace GameServer
{
    //Class that handles all incoming and outgoing packet instructions

    public class Listener
    {
        //TCP variables needed for the listener to comunicate
        public TcpClient connection;
        public NetworkStream networkStream;

        //Stream tools used to read and write the connection stream
        public StreamWriter streamWriter;
        public StreamReader streamReader;

        //Upload and download classes to send/receive files
        public UploadManager uploadManager;
        public DownloadManager downloadManager;

        //Data queue used to hold packets that are to be sent through the connection
        public Queue<Packet> dataQueue = new Queue<Packet>();

        //Useful variables to handle connection status
        public bool disconnectFlag;
        public bool KAFlag;

        //Reference to the ServerClient instance of this listener
        private ServerClient targetClient;

        public Listener(ServerClient clientToUse, TcpClient connection) 
        { 
            targetClient = clientToUse;

            this.connection = connection;
            networkStream = connection.GetStream();
            streamWriter = new StreamWriter(networkStream);
            streamReader = new StreamReader(networkStream);
        }

        //Runs in a separate thread and sends all queued packets through the connection

        public void SendData()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    if (targetClient.listener.dataQueue.Count() > 0)
                    {
                        Packet packet = targetClient.listener.dataQueue.Dequeue();
                        if (packet == null) continue;

                        targetClient.listener.streamWriter.WriteLine(Serializer.SerializePacketToString(packet));
                        targetClient.listener.streamWriter.Flush();
                    }
                }
            }
            catch { targetClient.listener.disconnectFlag = true; }
        }

        //Runs in a separate thread and listens for any kind of information being sent through the connection

        public void Listen()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    string data = targetClient.listener.streamReader.ReadLine();
                    if (string.IsNullOrEmpty(data)) continue;

                    Packet receivedPacket = Serializer.SerializeStringToPacket(data);
                    PacketHandler.HandlePacket(targetClient, receivedPacket);
                }
            }

            catch (Exception e)
            {
                if (Master.serverConfig.verboseLogs) Logger.WriteToConsole(e.ToString(), Logger.LogMode.Warning);

                targetClient.listener.disconnectFlag = true;
            }
        }

        //Runs in a separate thread and checks if the connection should still be up

        public void CheckConnectionHealth()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    if (targetClient.listener.disconnectFlag) break;
                }
            }
            catch { }

            Network.KickClient(targetClient);
        }

        //Runs in a separate thread and checks if the connection is still alive

        public void CheckKAFlag()
        {
            targetClient.listener.KAFlag = false;

            try
            {
                while (true)
                {
                    Thread.Sleep(5000);

                    if (targetClient.listener.KAFlag) targetClient.listener.KAFlag = false;
                    else targetClient.listener.disconnectFlag = true;
                }
            }
            catch { }
        }
    }
}
