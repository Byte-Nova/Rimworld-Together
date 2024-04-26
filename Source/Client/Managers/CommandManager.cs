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
                case (int)CommonEnumerators.CommandType.Op:
                    OnOpCommand();
                    break;

                case (int)CommonEnumerators.CommandType.Deop:
                    OnDeopCommand();
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

        private static void OnOpCommand()
        {
            ServerValues.isAdmin = true;
            ClientValues.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are now an admin!"));
        }

        private static void OnDeopCommand()
        {
            ServerValues.isAdmin = false;
            ClientValues.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are no longer an admin!"));
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
                ClientValues.SetIntentionalDisconnect( true, DisconnectionManager.DCReason.SaveQuitToMenu );
                SaveManager.ForceSave();
            }
        }
    }
}
