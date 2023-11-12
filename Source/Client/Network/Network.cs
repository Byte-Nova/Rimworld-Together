using System;
using System.Net.Sockets;
using System.Threading;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Network.Listener;
using RimworldTogether.GameClient.Patches;
using RimworldTogether.GameClient.Values;
using Verse;

namespace RimworldTogether.GameClient.Network
{
    public static class Network
    {
        public static ServerListener serverListener;
        public static string ip = "";
        public static string port = "";

        public static bool isConnectedToServer;
        public static bool isTryingToConnect;
        public static bool usingNewNetworking;

        public static void StartConnection()
        {
            if (TryConnectToServer())
            {
                Threader.GenerateThread(Threader.Mode.Heartbeat);

                DialogManager.PopWaitDialog();

                //TODO
                //PersistentPatches.ManageDevOptions();

                SiteManager.SetSiteDefs();

                serverListener.ListenToServer();
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

                    serverListener = new ServerListener(new(ip, int.Parse(port)));

                    isConnectedToServer = true;

                    return true;
                }

                catch (Exception e)
                {
                    Log.Warning($"[Rimworld Together] (DEBUG) > {e}");

                    return false;
                }
            }
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
                        break;
                    }
                }
                catch { break; }
            }

            DisconnectFromServer();
        }

        private static bool CheckIfConnected()
        {
            if (!serverListener.connection.Connected) return false;
            else
            {
                if (serverListener.connection.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (serverListener.connection.Client.Receive(buff, SocketFlags.Peek) == 0) return false;
                    else return true;
                }

                else return true;
            }
        }

        public static void DisconnectFromServer()
        {
            if (isConnectedToServer)
            {
                serverListener.connection.Dispose();
                isConnectedToServer = false;
                isTryingToConnect = false;

                Action r1 = delegate
                {
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        PersistentPatches.DisconnectToMenu();
                    }
                };

                DialogManager.PushNewDialog(new RT_Dialog_Error_Loop(new string[]
                {
                "Connection to the server has been lost!",
                "Game will now quit to menu"
                }, r1));

                ClientValues.CleanValues();
                ServerValues.CleanValues();
                ChatManager.ClearChat();
            }
        }
    }
}
