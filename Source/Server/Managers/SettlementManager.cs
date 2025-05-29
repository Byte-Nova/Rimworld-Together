using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class SettlementManager
    {
        //Variables

        public readonly static string fileExtension = ".mpsettlement";

        [HandlesPacket(PacketHeader.SettlementManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            PlayerSettlementData settlementData = Serializer.ConvertBytesToObject<PlayerSettlementData>(bytes);

            switch (settlementData._stepMode)
            {
                case SettlementStepMode.Add:
                    AddSettlement(client, settlementData);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSettlement(client, settlementData);
                    break;
            }
        }

        public static void AddSettlement(ServerClient client, PlayerSettlementData settlementData)
        {
            if (CheckIfTileIsInUse(settlementData._settlementFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Label} attempted to add a settlement at tile {settlementData._settlementFile.Tile}, but that tile already has a settlement");
            else
            {
                SettlementFile settlementFile = new SettlementFile();
                settlementFile.Tile = settlementData._settlementFile.Tile;
                settlementFile.UID = client.UserFile.Uid;
                settlementFile.Label = client.UserFile.Label;
                settlementData._settlementFile = settlementFile;

                Serializer.SerializeToFile(Path.Combine(Master.SettlementsPath, settlementFile.Tile + fileExtension), settlementFile);

                settlementData._stepMode = SettlementStepMode.Add;
                foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementData._settlementFile.Goodwill = GoodwillManager.GetSettlementGoodwill(cClient, settlementFile);

                        cClient.Listener.EnqueuePacket(PacketHeader.SettlementManager, settlementData);
                    }
                }

                InformationDisplayer.DisplayAddSettlement(settlementFile.Tile.ToString());
            }
        }

        public static void RemoveSettlement(ServerClient client, PlayerSettlementData settlementData)
        {
            if (!CheckIfTileIsInUse(settlementData._settlementFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData._settlementFile.Tile} was attempted to be removed, but the tile doesn't contain a settlement");

            SettlementFile settlementFile = GetSettlementFileFromTile(settlementData._settlementFile.Tile);

            if (client != null)
            {
                if (settlementFile.UID != client.UserFile.Uid)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData._settlementFile.Tile} attempted to be removed by " +
                        $"{client.UserFile.Uid}, but {settlementFile.UID} owns the settlement");
                }

                else
                {
                    Delete();
                    SendRemovalSignal();
                }
            }

            else
            {
                Delete();
                SendRemovalSignal();
            }

            void Delete()
            {
                File.Delete(Path.Combine(Master.SettlementsPath, settlementFile.Tile + fileExtension));

                InformationDisplayer.DisplayRemoveSettlement(settlementFile.Tile.ToString());
            }

            void SendRemovalSignal()
            {
                settlementData._stepMode = SettlementStepMode.Remove;

                NetworkHelper.SendPacketToAllClients(PacketHeader.SettlementManager, settlementData, client);
            }
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementJSON = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementJSON.Tile == tileToCheck) return true;
            }

            return false;
        }

        public static SettlementFile GetSettlementFileFromTile(int tileToGet)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.Tile == tileToGet) return settlementFile;
            }

            return null;
        }

        public static SettlementFile GetSettlementFileFromUsername(string usernameToGet)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.UID == usernameToGet) return settlementFile;
            }

            return null;
        }

        public static SettlementFile[] GetAllSettlements()
        {
            List<SettlementFile> settlementList = new List<SettlementFile>();

            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;
                settlementList.Add(Serializer.SerializeFromFile<SettlementFile>(settlement));
            }

            return settlementList.ToArray();
        }

        public static SettlementFile[] GetAllSettlementsFromUsername(string usernameToCheck)
        {
            List<SettlementFile> settlementList = new List<SettlementFile>();

            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.UID == usernameToCheck) settlementList.Add(settlementFile);
            }

            return settlementList.ToArray();
        }
    }
}
