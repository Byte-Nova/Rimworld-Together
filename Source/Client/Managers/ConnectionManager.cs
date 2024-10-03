using System;
using System.Linq;

namespace GameClient
{
    public static class ConnectionManager
    {
        public static void ShowConnectDialogs()
        {
            RT_Dialog_2Input dialog = new RT_Dialog_2Input(
            "Connection Details", "IP", "Port",
            delegate { ParseConnectionDetails(false); },
            null);

            ConnectionDataFile connectionData = PreferenceManager.LoadConnectionData();
            DialogManager.dialog2Input.inputOneResult = connectionData.IP;
            DialogManager.dialog2Input.inputTwoResult = connectionData.Port;

            DialogManager.PushNewDialog(dialog);
        }

        public static void ParseConnectionDetails(bool throughBrowser)
        {
            bool isInvalid = false;

            string[] answerSplit = null;
            if (throughBrowser)
            {
                answerSplit = ClientValues.serverBrowserContainer[DialogManager.dialogButtonListingResultInt].Split('|');

                if (string.IsNullOrWhiteSpace(answerSplit[0])) isInvalid = true;
                if (string.IsNullOrWhiteSpace(answerSplit[1])) isInvalid = true;
                if (answerSplit[1].Count() > 5) isInvalid = true;
                if (!answerSplit[1].All(Char.IsDigit)) isInvalid = true;
            }

            else
            {
                if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultOne)) isInvalid = true;
                if (string.IsNullOrWhiteSpace(DialogManager.dialog2ResultTwo)) isInvalid = true;
                if (DialogManager.dialog2ResultTwo.Count() > 5) isInvalid = true;
                if (!DialogManager.dialog2ResultTwo.All(Char.IsDigit)) isInvalid = true;
            }

            if (!isInvalid)
            {
                if (throughBrowser)
                {
                    Network.ip = answerSplit[0];
                    Network.port = answerSplit[1];
                    PreferenceManager.SaveConnectionData(answerSplit[0], answerSplit[1]);
                }

                else
                {
                    Network.ip = DialogManager.dialog2ResultOne;
                    Network.port = DialogManager.dialog2ResultTwo;
                    PreferenceManager.SaveConnectionData(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);
                }

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                Network.StartConnection();
            }

            else
            {
                RT_Dialog_Error d1 = new RT_Dialog_Error("Server details are invalid! Please try again!");
                DialogManager.PushNewDialog(d1);
            }
        }
    }
}