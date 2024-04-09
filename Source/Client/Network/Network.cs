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
                SiteManager.SetSiteDefs();

                Threader.GenerateThread(Threader.Mode.Listener);
                Threader.GenerateThread(Threader.Mode.Sender);
                Threader.GenerateThread(Threader.Mode.Health);
                Threader.GenerateThread(Threader.Mode.KASender);

                if (!ClientValues.isQuickConnecting) DialogShortcuts.ShowLoginOrRegisterDialogs();

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

      if ( ClientValues.isIntentionalDisconnect ) {
        string reason = "ERROR";
        switch ( ClientValues.intentionalDisconnectReason ) {
        case ClientValues.DCReason.None:
          reason = "No reason given.";
          DisconnectionManager.DisconnectToMenu();
          break;
        case ClientValues.DCReason.QuitToMenu:
          reason = "Quit to menu.";
          DialogManager.PushNewDialog( new RT_Dialog_OK( "Returning to main menu.", delegate { DisconnectionManager.DisconnectToMenu(); }));
          break;
        case ClientValues.DCReason.QuitToOS:
          reason = "Quit to OS.";
          DialogManager.PushNewDialog( new RT_Dialog_OK( "Quitting game.", delegate { DisconnectionManager.QuitGame(); }));
          break;
        case ClientValues.DCReason.SaveQuitToMenu:
          reason = "Save and Quit to Menu.";
          DialogManager.PushNewDialog( new RT_Dialog_OK( "Your progress has been saved!", delegate { DisconnectionManager.DisconnectToMenu(); }));
          break;
        case ClientValues.DCReason.SaveQuitToOS:
          reason = "Save and Quit to OS.";
          DialogManager.PushNewDialog( new RT_Dialog_OK( "Your progress has been saved!", delegate { DisconnectionManager.QuitGame(); }));
          break;
        case ClientValues.DCReason.LoginError:
          reason = "Login Error.";
          DisconnectionManager.DisconnectToMenu();
          break;
        case ClientValues.DCReason.ReturnToMenuSilently:
          reason = "Silent Return to Menu.";
          DisconnectionManager.DisconnectToMenu();
          break;
        default:
          reason = $"{ClientValues.intentionalDisconnectReason}";
          DisconnectionManager.DisconnectToMenu();
          break;
        }
        Log.Message( $"[Rimworld Together] > Disconnected from server : {reason}" );
      } else {
        Log.Message( $"[Rimworld Together] > Disconnected from server : Connection Lost" );
        DialogManager.PushNewDialog( new RT_Dialog_Error( "Your connection to the server has been lost...", delegate { DisconnectionManager.DisconnectToMenu(); } ) );
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
