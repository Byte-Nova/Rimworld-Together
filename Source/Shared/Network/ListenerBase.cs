using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Concurrent;

namespace Shared
{
    public class ListenerBase
    {
        public TcpClient Connection { get; set; }
        public NetworkStream Stream { get; set; }
        public bool DisconnectFlag { get; set; }

        public ConcurrentQueue<Packet> PacketQueue { get; set; } = new ConcurrentQueue<Packet>();

        // Optional actions for logging
        public Action PrintVerboseAction { get; set; }
        public Action PrintExtremeAction { get; set; }

        public string LatestException { get; private set; }

        // Cancellation token to cancel loops when connection is terminated.
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public void EnqueuePacket(Packet packet)
        {
            PacketQueue.Enqueue(packet);
        }

        public async Task WriteAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(1, _cts.Token);

                    if (DisconnectFlag || Stream == null || !Stream.CanWrite)
                        break;

                    if (PacketQueue.Count > 0)
                    {
                        if (!PacketQueue.TryDequeue(out Packet packet))
                            continue;

                        byte[] packetBuffer = Packet.CompressPacket(packet);
                        byte[] tracerBuffer = BitConverter.GetBytes(packetBuffer.Length);
                        byte[] completeBuffer = tracerBuffer.Concat(packetBuffer).ToArray();

                        if (Stream != null && Stream.CanWrite)
                        {
                            await Stream.WriteAsync(completeBuffer, 0, completeBuffer.Length, _cts.Token);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (IOException e)
            {
                LatestException = e.ToString();
                DisconnectFlag = true;
                PrintExtremeAction?.Invoke();
            }
            catch (Exception e)
            {
                LatestException = e.ToString();
                DisconnectFlag = true;
                PrintVerboseAction?.Invoke();
            }
        }

        public async Task ReadFullPacketAsync(byte[] content)
        {
            int readBytes = 0;
            try
            {
                while (readBytes < content.Length)
                {
                    int read = await Stream.ReadAsync(content, readBytes, content.Length - readBytes, _cts.Token);
                    if (read == 0)
                        throw new EndOfStreamException("Stream returned 0 bytes; connection may have closed.");
                    readBytes += read;
                }
            }
            catch (Exception e)
            {
                LatestException = e.ToString();
                DisconnectFlag = true;
                PrintVerboseAction?.Invoke();
            }
        }

        public async Task SendKAFlagAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(CommonValues.KeepAliveCooldown, _cts.Token);
                    KeepAliveData keepAliveData = new KeepAliveData();
                    Packet packet = Packet.CreateFromObject("KeepAliveManager", keepAliveData);
                    EnqueuePacket(packet);
                }
            }
            catch (Exception e)
            {
                LatestException = e.ToString();
                DisconnectFlag = true;
                PrintVerboseAction?.Invoke();
            }
        }

        public async Task CheckConnectionHealthAsync(Action toDo)
        {
            while (!DisconnectFlag)
            {
                await Task.Delay(1);
            }
            await Task.Delay(1000);
            toDo.Invoke();
        }

        public void DestroyConnection()
        {
            _cts.Cancel();
            Connection.Close();
        }
    }
}