using Verse;

namespace GameClient
{
    public enum NetworkState
    {
        Disconnected = 0,
        Connecting,
        Connected
    }
    
    //Main class that is used to handle the connection with the server
    public static class Network
    {
        //Variable that points what the state of the network might be for the client
        public static NetworkState state;

        //IP and Port that the connection will be bound to
        public static string ip = "";
        public static string port = "";

        //TCP listener that will handle the connection with the server
        public static Listener? listener;

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

                Logger.Message($"Connected to server");
                state = NetworkState.Connected;
                return;
            }

            DialogManager.PopWaitDialog();
            RT_Dialog_Error d1 = new RT_Dialog_Error("RTServerDidntResponded".Translate());
            DialogManager.PushNewDialog(d1);
            DisconnectFromServer();
        }

        //Tries to connect into the specified server

        public static bool TryConnectToServer()
        {
            if (state != NetworkState.Disconnected) return false;

            try 
            {
                state = NetworkState.Connecting;
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
            state = NetworkState.Disconnected;

            if (listener != null)
            {
                listener.DestroyConnection();
                listener = null;
            }
        }
    }
}
