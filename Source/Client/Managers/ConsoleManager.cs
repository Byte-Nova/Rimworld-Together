using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Values;
using RimWorld;
using Shared;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    //Class that handles how the client will answer to incoming server commands
    public static class ConsoleManager
    {
        //Parses the received packet into a command to execute

        [HandlesPacket(PacketHeader.ConsoleManager)]
        private static void ParsePacket(byte[] bytes)
        {
            CommandData data = Serializer.ConvertBytesToObject<CommandData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._commandMode)
            {
                case CommandMode.Op:
                    OnOpCommand();
                    break;

                case CommandMode.Deop:
                    OnDeopCommand();
                    break;

                case CommandMode.Broadcast:
                    OnBroadcastCommand(data);
                    break;

                case CommandMode.ForceSave:
                    OnForceSaveCommand();
                    break;
            }
        }

        //Executes the command depending on the type

        private static void OnOpCommand()
        {
            ClientValues.IsAdmin = true;
            ClientValues.ManageDevOptions();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "You are now an admin!" }));
        }

        private static void OnDeopCommand()
        {
            ClientValues.IsAdmin = false;
            ClientValues.ManageDevOptions();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "You are no longer an admin!" }));
        }

        private static void OnBroadcastCommand(CommandData commandData)
        {
            RimworldManager.GenerateLetter("Server Broadcast", ChatManagerHelper.ParseMessage(commandData._details, true), LetterDefOf.PositiveEvent);
        }

        private static void OnForceSaveCommand()
        {
            if (!ClientValues.IsReadyToPlay) DisconnectionManager.DisconnectToMenu();
            else
            {
                DisconnectionManager.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.SaveQuitToMenu);
                SaveManager.ForceSave();
            }
        }
    }
}
