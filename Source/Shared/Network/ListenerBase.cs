using System;
using System.IO;
using System.Threading;
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

        public void EnqueuePacket(Packet packet)
        {
            PacketQueue.Enqueue(packet);
        }

        public void Write()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    if (PacketQueue.Count > 0)
                    {
                        // If TryDequeue fails, we just continue the loop rather than return
                        if (!PacketQueue.TryDequeue(out Packet packet))
                            continue;

                        byte[] packetBuffer = Packet.CompressPacket(packet);
                        byte[] tracerBuffer = BitConverter.GetBytes(packetBuffer.Length);
                        byte[] completeBuffer = tracerBuffer.Concat(packetBuffer).ToArray();
                        Stream.Write(completeBuffer, 0, completeBuffer.Length);
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

        public void ReadFullPacket(byte[] content)
        {
            int readBytes = 0;
            try
            {
                while (readBytes < content.Length)
                {
                    int read = Stream.Read(content, readBytes, content.Length - readBytes);
                    // If read == 0, the connection is closed unexpectedly
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

        public void SendKAFlag()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(CommonValues.KeepAliveCooldown);

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

        public void CheckConnectionHealth(Action toDo)
        {
            while (!DisconnectFlag)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(1000);
            toDo.Invoke();
        }

        public void DestroyConnection()
        {
            Connection.Close();
        }
    }
}