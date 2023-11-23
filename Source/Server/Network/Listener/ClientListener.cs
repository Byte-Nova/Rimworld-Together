using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Network.Listener
{
    public class ClientListener
    {
        private ServerClient targetClient;

        public ClientListener(ServerClient clientToUse) { targetClient = clientToUse; }

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

        public void ListenToClient()
        {
            try
            {
                while (true)
                {
                    string data = targetClient.streamReader.ReadLine();

                    Packet receivedPacket = Serializer.SerializeStringToPacket(data);
                    PacketHandler.HandlePacket(targetClient, receivedPacket);
                }
            }

            catch (Exception e)
            {
                if (Program.serverConfig.verboseLogs) Logger.WriteToConsole(e.ToString(), Logger.LogMode.Warning);

                targetClient.disconnectFlag = true;
            }
        }

        public void CheckForConnectionHealth()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(100);

                    if (targetClient.disconnectFlag) break;
                }
            }
            catch { }

            Network.KickClient(targetClient);
        }

        public void CheckForKAFlag()
        {
            targetClient.KAFlag = false;

            try
            {
                while (true)
                {
                    Thread.Sleep(30000);

                    if (targetClient.KAFlag) targetClient.KAFlag = false;
                    else targetClient.disconnectFlag = true;
                }
            }
            catch { }
        }
    }
}
