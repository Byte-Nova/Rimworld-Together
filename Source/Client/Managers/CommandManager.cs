using RimWorld;

namespace RimworldTogether
{
    public static class CommandManager
    {
        public enum CommandType { Op, Deop, Ban, Disconnect, Quit, Broadcast }

        public static void ParseCommand(Packet packet)
        {
            CommandDetailsJSON commandDetailsJSON = Serializer.SerializeFromString<CommandDetailsJSON>(packet.contents[0]);

            switch(int.Parse(commandDetailsJSON.commandType))
            {
                case (int)CommandType.Op:
                    OnOpCommand();
                    break;

                case (int)CommandType.Deop:
                    OnDeopCommand();
                    break;

                case (int)CommandType.Ban:
                    OnBanCommand();
                    break;

                case (int)CommandType.Disconnect:
                    PersistentPatches.DisconnectToMenu();
                    break;

                case (int)CommandType.Quit:
                    PersistentPatches.QuitGame();
                    break;

                case (int)CommandType.Broadcast:
                    OnBroadcastCommand(commandDetailsJSON);
                    break;
            }
        }

        private static void OnOpCommand()
        {
            ServerValues.isAdmin = true;
            PersistentPatches.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are now an admin!"));
        }

        private static void OnDeopCommand()
        {
            ServerValues.isAdmin = false;
            PersistentPatches.ManageDevOptions();
            DialogManager.PushNewDialog(new RT_Dialog_OK("You are no longer an admin!"));
        }

        private static void OnBanCommand()
        {
            DialogManager.PushNewDialog(new RT_Dialog_OK("You have been banned from the server!"));
        }

        private static void OnBroadcastCommand(CommandDetailsJSON commandDetailsJSON)
        {
            LetterManager.GenerateLetter("Server Broadcast", commandDetailsJSON.commandDetails, LetterDefOf.PositiveEvent);
        }
    }
}
