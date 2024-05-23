using RimWorld;
using Shared;

namespace GameClient
{
    //Class that handles how the client will answer to incoming server commands

    public static class CommandManager
    {
        //Parses the received packet into a command to execute

        public static void ParseCommand(Packet packet)
        {
            CommandData commandData = (CommandData)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(commandData.commandType))
            {
                case (int)CommonEnumerators.CommandType.Grant:
                    OnGrantCommand(commandData);
                    break;

                case (int)CommonEnumerators.CommandType.Revoke:
                    OnRevokeCommand(commandData);
                    break;

                case (int)CommonEnumerators.CommandType.Broadcast:
                    OnBroadcastCommand(commandData);
                    break;

                case (int)CommonEnumerators.CommandType.ForceSave:
                    OnForceSaveCommand();
                    break;
            }
        }

        //Executes the command depending on the type

        private static void OnGrantCommand(CommandData commandData)
        {
            switch(commandData.commandDetails) {
                case "Admin":
                    OnAdminCommand();
                    break;
                case "Operator":
                    OnOpCommand();
                    break;
            }
        }

        private static void OnRevokeCommand(CommandData commandData)
        {
            switch (commandData.commandDetails)
            {
                case "Admin":
                    OnRevokeAdminCommand();
                    break;
                case "Operator":
                    OnDeopCommand();
                    break;
            }
        }

        private static void OnAdminCommand()
        {
            ServerValues.isAdmin = true;
            ClientValues.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are now an admin!"));
        }

        private static void OnRevokeAdminCommand()
        {
            ServerValues.isAdmin = false;
            ClientValues.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are no longer an admin!"));
        }


        private static void OnOpCommand()
        {
            ServerValues.isOperator = true;
            ClientValues.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are now an operator!"));
        }

        private static void OnDeopCommand()
        {
            ServerValues.isOperator = false;
            ClientValues.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are no longer an operator!"));
        }

        private static void OnBroadcastCommand(CommandData commandData)
        {
            RimworldManager.GenerateLetter("Server Broadcast", commandData.commandDetails, LetterDefOf.PositiveEvent);
        }

        private static void OnForceSaveCommand()
        {
            if (!ClientValues.isReadyToPlay) DisconnectionManager.DisconnectToMenu();
            else
            {
                ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.SaveQuitToMenu);
                SaveManager.ForceSave();
            }
        }
    }
}
