using Verse;

namespace GameClient
{
    //Main class that is used to handle the connection with the server

    public static class Network
    {
        //IP and Port that the connection will be bound to
        public static string ip = "";
        public static string port = "";

        //TCP listener that will handle the connection with the server
        public static Listener listener;

        //Useful booleans to check connection status with the server
        public static bool isConnectedToServer;
        public static bool isTryingToConnect;

        //Entry point function of the network class

        public static void StartConnection()
        {
            if (TryConnectToServer())
            {
                DialogManager.PopWaitDialog();
                SiteManager.SetSiteDefs();

                Threader.GenerateThread(Threader.Mode.Listener);
                Threader.GenerateThread(Threader.Mode.Sender);
                Threader.GenerateThread(Threader.Mode.Health);
                Threader.GenerateThread(Threader.Mode.KASender);

                Log.Message($"[Rimworld Together] > Connected to server");
            }

            else
            {
                DialogManager.PopWaitDialog();

                RT_Dialog_Error d1 = new RT_Dialog_Error("The server did not respond in time");
                DialogManager.PushNewDialog(d1);

                CleanValues();
            }
        }

        //Tries to connect into the specified server

        private static bool TryConnectToServer()
        {
            if (isTryingToConnect || isConnectedToServer) return false;
            else
            {
                try
                {
                    isTryingToConnect = true;

                    isConnectedToServer = true;

                    listener = new Listener(new(ip, int.Parse(port)));

                    return true;
                }
                catch { return false; }
            }
        }

        //Disconnects client from the server

        public static void DisconnectFromServer()
        {
            listener.DestroyConnection();

            Log.Message($"[Rimworld Together] > Disconnected from server");

            if (ClientValues.isQuiting) DisconnectionManager.QuitGame();
            else
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("Connection to the server has been lost!",
                    delegate { DisconnectionManager.DisconnectToMenu(); }));
            }
        }

        //Clears all related values

        public static void CleanValues()
        {
            isTryingToConnect = false;
            isConnectedToServer = false;
        }
    }
}
