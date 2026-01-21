using System.Linq;
using GameClient.Dialogs;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using Shared;

namespace GameClient.Managers
{
    public static class ConnectionManager
    {
        public static void ShowWelcomeDialogs() 
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_YesNo("Choose a login method:",
                delegate { RT_Dialog_Base.PushNewDialog(new RT_Dialog_ServerListing()); },
                delegate { ShowConnectDialogs(); },
                "Server Browser",
                "Login"
                ));
        }

        public static void ShowConnectDialogs()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Inputs("Connection Details", new string[] { "IP", "Port" }, new bool[] { false, false },
                delegate { ParseConnectionDetails(); }));
        }

        public static void ParseConnectionDetails()
        {
            bool isInvalid = false;

            if (!StringChecker.CheckIfStringValid(RT_Dialog_Inputs.DialogInputResults[0])) isInvalid = true;
            if (!StringChecker.CheckIfStringValid(RT_Dialog_Inputs.DialogInputResults[1])) isInvalid = true;
            if (!RT_Dialog_Inputs.DialogInputResults[1].All(char.IsDigit)) isInvalid = true;
            if (RT_Dialog_Inputs.DialogInputResults[1].Count() > 5) isInvalid = true;

            if (isInvalid) RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Server details are invalid! Please try again!" }));
            else
            {
                ClientNetwork.Ip = RT_Dialog_Inputs.DialogInputResults[0];
                ClientNetwork.Port = RT_Dialog_Inputs.DialogInputResults[1];

                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                ClientNetwork _ = new ClientNetwork();
            }
        }
    }
}