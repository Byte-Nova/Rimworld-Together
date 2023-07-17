using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Verse;

namespace RimworldTogether
{
    public static class Network
    {
        public static TcpClient connection;
        public static NetworkStream ns;
        public static StreamWriter sw;
        public static StreamReader sr;

        public static bool isConnectedToServer;
        public static bool isTryingToConnect;

        public static string ip = "";
        public static string port = "";

        public static void StartConnection()
        {
            if (TryConnectToServer())
            {
                DialogManager.PopWaitDialog();

                PersistentPatches.ManageDevOptions();

                SiteManager.SetSiteDefs();

                Threader.GenerateThread(Threader.Mode.Heartbeat);

                ListenToServer();
            }

            else
            {
                DialogManager.PopWaitDialog();

                RT_Dialog_Error d1 = new RT_Dialog_Error("The server did not respond in time");
                DialogManager.PushNewDialog(d1);

                DisconnectFromServer();
            }
        }

        private static bool TryConnectToServer()
        {
            if (isTryingToConnect || isConnectedToServer) return false;
            else
            {
                try
                {
                    isTryingToConnect = true;

                    connection = new TcpClient(ip, int.Parse(port));
                    ns = connection.GetStream();
                    sw = new StreamWriter(ns);
                    sr = new StreamReader(ns);
                    isConnectedToServer = true;

                    return true;
                }

                catch { return false; }
            }
        }

        public static void ListenToServer()
        {
            DialogShortcuts.ShowLoginOrRegisterDialogs();

            while (isConnectedToServer)
            {
                try
                {
                    string data = sr.ReadLine();
                    Packet receivedPacket = Serializer.SerializeToPacket(data);
                    PacketHandlers.HandlePacket(receivedPacket);
                }

                catch(Exception e)
                {
                    Log.Warning($"Network exception > {e}");
                    DisconnectFromServer();
                }
            }
        }

        public static void SendData(Packet packet)
        {
            sw.WriteLine(Serializer.SerializeToString(packet));
            sw.Flush();
        }

        public static void HeartbeatServer()
        {
            while (isConnectedToServer)
            {
                Thread.Sleep(100);

                try
                {
                    if (!CheckIfConnected())
                    {
                        DisconnectFromServer();
                    }
                }
                catch { }
            }
        }

        private static bool CheckIfConnected()
        {
            if (!connection.Connected) return false;
            else
            {
                if (connection.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (connection.Client.Receive(buff, SocketFlags.Peek) == 0) return false;
                    else return true;
                }

                else return true;
            }
        }

        public static void DisconnectFromServer()
        {
            if (connection != null) connection.Dispose();
            isConnectedToServer = false;
            isTryingToConnect = false;

            if (Current.ProgramState != ProgramState.Entry)
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error_Loop(new string[]
                {
                    "Connection to the server has been lost!",
                    "Game will now quit to menu"
                },
                delegate { PersistentPatches.DisconnectToMenu(); }));
            }

            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ChatManager.ClearChat();
        }
    }
}
