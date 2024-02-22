using Shared;

namespace GameServer
{
    public class ClientListener
    {
        private ServerClient targetClient;

        public ClientListener(ServerClient clientToUse) { targetClient = clientToUse; }

        public void SendData(Packet packet)
        {
            try
            {
                targetClient.listener.streamWriter.WriteLine(Serializer.SerializePacketToString(packet));
                targetClient.listener.streamWriter.Flush();
            }
            catch { targetClient.listener.disconnectFlag = true; }
        }

        public void ListenToClient()
        {
            try
            {
                while (true)
                {
                    string data = targetClient.listener.streamReader.ReadLine();

                    Packet receivedPacket = Serializer.SerializeStringToPacket(data);
                    PacketHandler.HandlePacket(targetClient, receivedPacket);
                }
            }

            catch (Exception e)
            {
                if (Program.serverConfig.verboseLogs) Logger.WriteToConsole(e.ToString(), Logger.LogMode.Warning);

                targetClient.listener.disconnectFlag = true;
            }
        }

        public void CheckForConnectionHealth()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(100);

                    if (targetClient.listener.disconnectFlag) break;
                }
            }
            catch { }

            Network.KickClient(targetClient);
        }

        public void CheckForKAFlag()
        {
            targetClient.listener.KAFlag = false;

            try
            {
                while (true)
                {
                    Thread.Sleep(30000);

                    if (targetClient.listener.KAFlag) targetClient.listener.KAFlag = false;
                    else targetClient.listener.disconnectFlag = true;
                }
            }
            catch { }
        }
    }
}
