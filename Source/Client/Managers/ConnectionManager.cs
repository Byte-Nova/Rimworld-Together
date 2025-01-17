using System.Linq;
using Shared;
using GameClient.Core.Preferences;
using GameClient.Dialogs;
using GameClient.Files;
using GameClient.Misc;
using GameClient.TCP;

namespace GameClient.Managers
{
    [RTManager]
    public static class ConnectionManager
    {
        public static void ShowConnectDialogs()
        {
            RT_Dialog_2Input dialog = new RT_Dialog_2Input("Connection Details", "IP", "Port", 
                delegate { ParseConnectionDetails(); }, null);

            ConnectionDataFile connectionData = ConnectionDataManager.LoadConnectionData();
            DialogManager.dialog2Input.inputOneResult = connectionData.IP;
            DialogManager.dialog2Input.inputTwoResult = connectionData.Port;

            DialogManager.PushNewDialog(dialog);
        }

        public static void ParseConnectionDetails()
        {
            bool isInvalid = false;

            if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultOne)) isInvalid = true;
            if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultTwo)) isInvalid = true;
            if (DialogManager.dialog2ResultTwo.Count() > 5) isInvalid = true;
            if (!DialogManager.dialog2ResultTwo.All(char.IsDigit)) isInvalid = true;

            if (isInvalid) DialogManager.PushNewDialog(new RT_Dialog_Error("Server details are invalid! Please try again!"));
            else
            {
                Network.ip = DialogManager.dialog2ResultOne;
                Network.port = DialogManager.dialog2ResultTwo;
                ConnectionDataManager.SaveConnectionData(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                Threader.GenerateThread(Threader.Mode.Start);
            }
        }
    }
}