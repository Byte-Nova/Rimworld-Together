using static Shared.CommonEnumerators;

namespace GameClient
{
    //Main class that is used to handle the connection with the server

    public static class Network
    {
        //Variables that points what the state of the network might be for the client

        public static ClientNetworkState state;

        //IP and Port that the connection will be bound to

        public static string ip = "";

        public static string port = "";

        //TCP listener that will handle the connection with the server

        public static Listener listener;

        //Entry point function of the network class

        public static void StartConnection()
        {
            if (TryConnectToServer())
            {
                ClientValues.ManageDevOptions();

                Threader.GenerateThread(Threader.Mode.Listener);
                Threader.GenerateThread(Threader.Mode.Sender);
                Threader.GenerateThread(Threader.Mode.Health);
                Threader.GenerateThread(Threader.Mode.KASender);

                if (!ClientValues.isQuickConnecting) LoginManager.ShowLoginOrRegisterDialogs();

                Logger.Message($"Connected to server");
                state = ClientNetworkState.Connected;
            }

            else
            {
                DialogManager.PopWaitDialog();
                RT_Dialog_Error d1 = new RT_Dialog_Error("The server did not respond in time");
                DialogManager.PushNewDialog(d1);
                DisconnectFromServer();
            }
        }

        //Tries to connect into the specified server

        public static bool TryConnectToServer()
        {
            if (state != ClientNetworkState.Disconnected) return false;

            try 
            {
                state = ClientNetworkState.Connecting;
                listener = new Listener(new(ip, int.Parse(port)));
            } 
            catch { return false; }

            return true;
        }

        //Disconnects client from the server
        
        public static void DisconnectFromServer()
        {
            CleanNetworkVariables();
            DisconnectionManager.HandleDisconnect();
        }

        public static void CleanNetworkVariables()
        {
            state = ClientNetworkState.Disconnected;

            if (listener != null)
            {
                listener.DestroyConnection();
                listener = null;
            }
        }
    }
}
