using System;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Network.Listener;
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

        public static void StartConnection()
        {
            if (TryConnectToServer())
            {
                DialogManager.PopWaitDialog();
                ClientValues.ManageDevOptions();
                SiteManager.SetSiteDefs();

                Threader.GenerateThread(Threader.Mode.Listener);
                Threader.GenerateThread(Threader.Mode.Health);
                Threader.GenerateThread(Threader.Mode.KASender);

                Log.Message($"[Rimworld Together] > Connected to server");
            }

            else
            {
                DialogManager.PopWaitDialog();

                RT_Dialog_Error d1 = new RT_Dialog_Error("The server did not respond in time");
                DialogManager.PushNewDialog(d1);

                ClearAllValues();
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

                    isConnectedToServer = true;

                    serverListener = new ServerListener(new(ip, int.Parse(port)));

                    return true;
                }
                catch { return false; }
            }
        }

        public static void DisconnectFromServer()
        {
            Action toDo = delegate
            {
                serverListener.connection.Dispose();

                Action r1 = delegate
                {
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        DisconnectionManager.DisconnectToMenu();
                    }
                };

                DialogManager.PushNewDialog(new RT_Dialog_Error_Loop(new string[]
                {
                        "Connection to the server has been lost!",
                        "Game will now quit to menu"
                }, r1));

                ClearAllValues();

                Log.Message($"[Rimworld Together] > Disconnected from server");
            };

            Main.threadDispatcher.Enqueue(toDo);
        }

        public static void ClearAllValues()
        {
            isTryingToConnect = false;
            isConnectedToServer = false;

            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ChatManager.ClearChat();
        }
    }
}
