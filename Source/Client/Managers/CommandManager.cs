using RimWorld;
using Shared;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles how the client will answer to incoming server commands

    public static class CommandManager
    {
        //Parses the received packet into a command to execute

        public static void ParsePacket(Packet packet)
        {
            CommandData commandData = Serializer.ConvertBytesToObject<CommandData>(packet.contents);

            switch(commandData._commandMode)
            {
                case CommandMode.Op:
                    OnOpCommand();
                    break;

                case CommandMode.Deop:
                    OnDeopCommand();
                    break;

                case CommandMode.Broadcast:
                    OnBroadcastCommand(commandData);
                    break;

                case CommandMode.ForceSave:
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
            RimworldManager.GenerateLetter("Server Broadcast", ChatManagerHelper.ParseMessage(commandData._details, true), LetterDefOf.PositiveEvent);
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
