using RimWorld;
using Shared;
using Verse;

namespace GameClient
{
    //Class that handles how the client will answer to incoming server commands

    public static class CommandManager
    {
        //Parses the received packet into a command to execute

        public static void ParseCommand(Packet packet)
        {
            CommandDetailsJSON commandDetailsJSON = (CommandDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(commandDetailsJSON.commandType))
            {
                case (int)CommonEnumerators.CommandType.Op:
                    OnOpCommand();
                    break;

                case (int)CommonEnumerators.CommandType.Deop:
                    OnDeopCommand();
                    break;

                case (int)CommonEnumerators.CommandType.Broadcast:
                    OnBroadcastCommand(commandDetailsJSON);
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
            DialogManager.PushNewDialog(new RT_Dialog_OK("RimworldTogether.PlayerAdmin".Translate()));
        }

        private static void OnDeopCommand()
        {
            ServerValues.isAdmin = false;
            ClientValues.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("RimworldTogether.PlayerNoLongerAdmin".Translate()));
        }

        private static void OnBroadcastCommand(CommandDetailsJSON commandDetailsJSON)
        {
            RimworldManager.GenerateLetter("Server Broadcast", commandDetailsJSON.commandDetails, LetterDefOf.PositiveEvent);
        }

        private static void OnForceSaveCommand()
        {
            if (!ClientValues.isReadyToPlay) DisconnectionManager.DisconnectToMenu();
            else
            {
                ClientValues.isDisconnecting = true;
                SaveManager.ForceSave();
            }
        }
    }
}
