using Verse;
using static Shared.CommonEnumerators;

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
                SiteManager.SetSiteDefs();

                Threader.GenerateThread(Threader.Mode.Listener);
                Threader.GenerateThread(Threader.Mode.Sender);
                Threader.GenerateThread(Threader.Mode.Health);
                Threader.GenerateThread(Threader.Mode.KASender);

                if (!ClientValues.isQuickConnecting) DialogShortcuts.ShowLoginOrRegisterDialogs();

                Logger.WriteToConsole("Connected to server", LogMode.Message);
            }

            else
            {
                DialogManager.PopDialog();

                RT_Dialog_OK d1 = new RT_Dialog_OK("ERROR", "The server did not respond in time");
                DialogManager.PushNewDialog(d1);

                CleanValues();
            }
        }

        //Tries to connect into the specified server

        public static bool TryConnectToServer()
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

            DisconnectionManager.HandleDisconnect();
        }


        //Clears all related values

        public static void CleanValues()
        {
            isTryingToConnect = false;
            isConnectedToServer = false;
        }
    }
}
