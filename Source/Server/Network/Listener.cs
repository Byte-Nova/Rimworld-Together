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

        //Enqueues a new packet into the data queue if needed

        public void EnqueuePacket(Packet packet)
        {
            if (disconnectFlag) return;
            else dataQueue.Enqueue(packet);
        }

        //Runs in a separate thread and sends all queued packets through the connection

        public void SendData()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    if (dataQueue.Count() > 0)
                    {
                        Packet packet = dataQueue.Dequeue();
                        if (packet == null) continue;

                        streamWriter.WriteLine(Serializer.SerializePacketToString(packet));
                        streamWriter.Flush();
                    }
                }
            }
            catch { disconnectFlag = true; }
        }

        //Runs in a separate thread and listens for any kind of information being sent through the connection

        public void Listen()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    string data = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(data)) continue;

                    Packet receivedPacket = Serializer.SerializeStringToPacket(data);
                    PacketHandler.HandlePacket(targetClient, receivedPacket);
                }
            }

            catch (Exception e)
            {
                if (Master.serverConfig.verboseLogs) Logger.WriteToConsole(e.ToString(), Logger.LogMode.Warning);

                disconnectFlag = true;
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

                    if (disconnectFlag) break;
                }
            }
            catch { }

            Thread.Sleep(1000);

            Network.KickClient(targetClient);
        }

        //Runs in a separate thread and checks if the connection is still alive

        public void CheckKAFlag()
        {
            KAFlag = false;

            try
            {
                while (true)
                {
                    Thread.Sleep(5000);

                    if (KAFlag) KAFlag = false;
                    else break;
                }
            }
            catch { }

            disconnectFlag = true;
        }

        //Forcefully ends the connection with the client and any important process associated with it

        public void DestroyConnection()
        {
            connection.Close();
            uploadManager?.fileStream.Close();
            downloadManager?.fileStream.Close();
        }
    }
}
