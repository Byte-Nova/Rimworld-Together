using GameServer.Core;
using GameServer.Misc;
using Shared;
using System.Net.Sockets;
using static Shared.CommonEnumerators;

namespace GameServer.TCP
{
    public class Listener : ListenerBase
    {
        private ServerClient TargetClient { get; set; }

        public Listener(ServerClient clientToUse, TcpClient connection)
        {
            TargetClient = clientToUse;
            Connection = connection;
            Stream = connection.GetStream();

            PrintVerboseAction = () => Printer.Warning(LatestException, LogImportanceMode.Verbose);
            PrintExtremeAction = () => Printer.Warning(LatestException, LogImportanceMode.Extreme);

            Task.Run(() => Read());
            Task.Run(() => Write());
            Task.Run(() => SendKAFlag());
            Task.Run(() => CheckConnectionHealth(() => Network.KickClient(TargetClient)));
        }

        public void Read()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);
                    if (Stream.DataAvailable)
                    {
                        byte[] buffer = new byte[Packet.DefaultPacketSizeInBytes];
                        Stream.Read(buffer, 0, buffer.Length);
                        Packet.SetPacketSize(BitConverter.ToInt32(buffer, 0));

                        buffer = new byte[Packet.CurrentPacketSizeInBytes];
                        ReadFullPacket(buffer);
                        Packet packet = Packet.DecompressPacket(buffer);

                        Printer.Message($"[Packet] > {packet.Header}", LogImportanceMode.Verbose);

                        try
                        {
                            Master.managerDictionary[packet.Header].Invoke(null, new object[] { TargetClient, packet });
                        }
                        catch (Exception ex)
                        {
                            Printer.Error($"Error executing method '{packet.Header}'");
                            Printer.Error("Force-disconnecting due to method manager exception");
                            Printer.Error(ex.ToString());
                            DisconnectFlag = true;
                        }
                    }
                }
            }
            catch (ObjectDisposedException e)
            {
                Printer.Warning(e, LogImportanceMode.Extreme);
                DisconnectFlag = true;
            }
            catch (Exception e)
            {
                Printer.Warning(e.ToString(), LogImportanceMode.Verbose);
                DisconnectFlag = true;
            }
        }
    }
}