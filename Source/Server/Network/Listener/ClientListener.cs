using RimworldTogether.GameServer.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Network.Listener
{
    public class ClientListener
    {
        private ServerClient targetClient;

        public ClientListener(ServerClient clientToUse)
        {
            targetClient = clientToUse;

            GenerateClientThread();
        }

        public void GenerateClientThread()
        {
            ParameterizedThreadStart toDo = delegate { ListenToClient(); };
            Thread clientThread = new Thread(toDo);
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        public void ListenToClient()
        {
            try
            {
                while (!targetClient.disconnectFlag)
                {
                    string data = targetClient.streamReader.ReadLine();

                    Packet receivedPacket = Serializer.SerializeStringToPacket(data);
                    PacketHandler.HandlePacket(targetClient, receivedPacket);
                }
            }

            catch (Exception e)
            {
                Logger.WriteToConsole(e.ToString(), Logger.LogMode.Warning);
                targetClient.disconnectFlag = true;
            }
        }

        public void SendData(Packet packet)
        {
            while (targetClient.isBusy) Thread.Sleep(100);

            try
            {
                targetClient.isBusy = true;

                targetClient.streamWriter.WriteLine(Serializer.SerializePacketToString(packet));
                targetClient.streamWriter.Flush();

                targetClient.isBusy = false;
            }
            catch { targetClient.disconnectFlag = true; }
        }
    }
}
