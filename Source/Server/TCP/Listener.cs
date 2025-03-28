using GameServer.Core;
using GameServer.Misc;
using Shared;
using System.Net.Sockets;
using static Shared.CommonEnumerators;
using System.Threading.Tasks;

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

            // Run asynchronous loops on the server side.
            Task.Run(ReadAsync);
            Task.Run(WriteAsync);
            Task.Run(SendKAFlagAsync);
            Task.Run(() => CheckConnectionHealthAsync(() => Network.KickClient(TargetClient)));
        }

        public async Task ReadAsync()
        {
            try
            {
                while (true)
                {
                    await Task.Delay(1);

                    if (DisconnectFlag || Stream == null || !Stream.CanRead)
                        break;

                    if (Stream.DataAvailable)
                    {
                        byte[] buffer = new byte[Packet.DefaultPacketSizeInBytes];
                        int read = await Stream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            DisconnectFlag = true;
                            break;
                        }

                        Packet.SetPacketSize(BitConverter.ToInt32(buffer, 0));
                        buffer = new byte[Packet.CurrentPacketSizeInBytes];
                        await ReadFullPacketAsync(buffer);
                        Packet packet = Packet.DecompressPacket(buffer);

                        Printer.Message($"[Packet] > {packet.Header}", LogImportanceMode.Verbose);

                        try
                        {
                            Master.managerDictionary[packet.Header].Invoke(null, new object[] { TargetClient, packet });
                        }
                        catch (System.Exception ex)
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
            catch (System.Exception e)
            {
                Printer.Warning(e.ToString(), LogImportanceMode.Verbose);
                DisconnectFlag = true;
            }
        }
    }
}