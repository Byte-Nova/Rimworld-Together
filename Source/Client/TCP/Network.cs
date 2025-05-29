using System.IO;
using System;
using System.Net.Sockets;
using GameClient.Core.Preferences;
using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using static Shared.CommonEnumerators;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace GameClient.TCP
{
    //Main class that is used to handle the connection with the server

    public static class Network
    {
        //Variables that points what the state of the network might be for the client

        public static ClientNetworkState State;

        //IP and Port that the connection will be bound to

        public static string Ip { get; set; } = "";

        public static string Port { get; set; } = "";

        //TCP listener that will handle the connection with the server

        public static Listener Listener { get; private set; }

        //Entry point function of the network class

        public static void StartConnection()
        {
            if (TryConnectToServer())
            {
                ConnectionDataHandler.SaveConnectionData(Ip, Port);
                ClientValues.ManageDevOptions();

                State = ClientNetworkState.Connected;

                Printer.Message($"Connected to server");
            }

            else
            {
                RT_Dialog_Wait.Instance.Close();
                RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "The server did not respond in time" });
                RT_Dialog_Base.PushNewDialog(d1);
                DisconnectFromServerInstant();
            }
        }

        //Tries to connect into the specified server

        public static bool TryConnectToServer()
        {
            if (State != ClientNetworkState.Disconnected) return false;

            try
            {
                State = ClientNetworkState.Connecting;
                Listener = new Listener(new(Ip, int.Parse(Port)));
            }
            catch { return false; }

            return true;
        }

        //Disconnects client from the server

        public static void DisconnectFromServerInstant()
        {
            CleanNetworkVariables();
            DisconnectionManager.HandleDisconnect();
        }
        
        public static void CleanNetworkVariables()
        {
            State = ClientNetworkState.Disconnected;

            if (Listener != null)
            {
                Listener.DestroyConnection();
                Listener = null;
            }
        }
    }
}
