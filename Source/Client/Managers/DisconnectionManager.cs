using Verse;
using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Values;

namespace GameClient.Managers
{
    //Class that contains all the disconnection functions that the mod uses

    public static class DisconnectionManager
    {
        public enum DCReason { None, SaveQuitToMenu, SaveQuitToOS, QuitToMenu, ConnectionLost }

        public static DCReason IntentionalDisconnectReason { get; private set; }

        public static bool IsIntentionalDisconnect { get; private set; }

        public static void SetIntentionalDisconnect(bool mode, DisconnectionManager.DCReason reason = DisconnectionManager.DCReason.None)
        {
            IsIntentionalDisconnect = mode;
            IntentionalDisconnectReason = reason;
        }

        //Executes different actions depending on the disconnection mode

        public static void HandleDisconnect()
        {
            if (IsIntentionalDisconnect)
            {
                string reason = "ERROR";

                switch (IntentionalDisconnectReason)
                {
                    case DCReason.None:
                        reason = "No reason given";
                        DisconnectToMenu();
                        break;

                    case DCReason.QuitToMenu:
                        reason = "Quit to menu";
                        DisconnectToMenu();
                        break;

                    case DCReason.SaveQuitToMenu:
                        reason = "Save and Quit to Menu";
                        RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Your progress has been saved!" }, DisconnectToMenu));
                        break;

                    case DCReason.SaveQuitToOS:
                        reason = "Save and Quit to OS";
                        RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Your progress has been saved!" }, QuitGame));
                        break;

                    case DCReason.ConnectionLost:
                        reason = "Connection to server lost";
                        DisconnectToMenu();
                        break;

                    default:
                        reason = $"{IntentionalDisconnectReason}";
                        DisconnectToMenu();
                        break;
                }

                Printer.Message($"Disconnected from server: {reason}");
            }

            else
            {
                Printer.Message($"Disconnected from server: Connection Lost");

                if (Current.ProgramState != ProgramState.Entry)
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_YesNo("Connection lost. Save game?",
                        delegate { SaveManager.ForceSave(); DisconnectToMenu(); }, delegate { DisconnectToMenu(); }));
                }
                else DisconnectToMenu();
            }
        }

        //Kicks the client into the main menu

        public static void DisconnectToMenu()
        {
            ClientValues.CleanValues();
            SessionValues.CleanValues();
            ChatManager.CleanChat();

            RT_Dialog_Wait.Instance.Close();

            if (Current.ProgramState != ProgramState.Entry)
            {
                LongEventHandler.QueueLongEvent(delegate { }, "Entry", "", doAsynchronously: false, null);
            }
        }

        public static void QuitGame() { Root.Shutdown(); }

        public static void RestartGame() { GenCommandLine.Restart(); }
    }
}
