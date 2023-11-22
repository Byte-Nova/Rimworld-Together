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

            Threader.GenerateClientThread(this, Threader.ClientMode.Listener, Core.Program.serverCancelationToken);
            Threader.GenerateClientThread(this, Threader.ClientMode.Health, Core.Program.serverCancelationToken);
            Threader.GenerateClientThread(this, Threader.ClientMode.KAFlag, Core.Program.serverCancelationToken);
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

        public void CheckForConnectionHealth()
        {
            while (true)
            {
                Thread.Sleep(100);

                if (targetClient.disconnectFlag) break;
            }

            Network.KickClient(targetClient);
        }

        public void CheckForKAFlag()
        {
            targetClient.KAFlag = false;

            while (!targetClient.disconnectFlag)
            {
                Thread.Sleep(15000);

                if (targetClient.KAFlag) targetClient.KAFlag = false;
                else targetClient.disconnectFlag = true;
            }
        }
    }
}
