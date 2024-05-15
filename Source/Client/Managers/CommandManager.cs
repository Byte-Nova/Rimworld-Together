using Newtonsoft.Json;
using RimWorld;
using Shared;
using System.Collections.Generic;
using System.Linq;
using Verse;

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
                    OnForceSaveCommand(commandData);
                    break;

                case (int)CommonEnumerators.CommandType.SyncSettlements:
                    OnSyncSettlementsCommand(commandData);
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

        private static void OnForceSaveCommand(CommandData commandData)
        {
            if (!ClientValues.isReadyToPlay) DisconnectionManager.DisconnectToMenu();
            else
            {
                bool isDisconnecting = commandData.commandDetails != "isNotDisconnecting";

                ClientValues.SetIntentionalDisconnect(isDisconnecting, DisconnectionManager.DCReason.SaveQuitToMenu);
                SaveManager.ForceSave();
            }
        }

        private static void OnSyncSettlementsCommand(CommandData commandData)
        {
            if (Network.state == NetworkState.Connected)
            {
                List<string> tilesServer = new List<string>(commandData.commandDetails.Split(','));
                List<string> tilesClient = new();

                foreach (Map map in Find.Maps.ToArray())
                {
                    if (map.IsPlayerHome)
                    {
                        string tile = map.Tile.ToString();

                        tilesClient.Add(tile);

                        if (tilesServer.Contains(tile))
                        {
                            continue;
                        }

                        SettlementData settlementData = new SettlementData();
                        settlementData.tile = tile;
                        settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                        Network.listener.EnqueuePacket(packet);
                    }
                }

                foreach(string tileServer in tilesServer)
                {
                    if (tileServer.Length == 0)
                    {
                        continue;
                    }

                    if (!tilesClient.Contains(tileServer))
                    {
                        SettlementData settlementData = new SettlementData();
                        settlementData.tile = tileServer;
                        settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Remove).ToString();

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                        Network.listener.EnqueuePacket(packet);
                    }
                }
            }
        }
    }
}
