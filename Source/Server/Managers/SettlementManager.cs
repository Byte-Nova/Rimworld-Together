using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SettlementManager
    {
        //Variables

        public readonly static string fileExtension = ".mpsettlement";

        public static void ParseSettlementPacket(ServerClient client, Packet packet)
        {
            PlayerSettlementData settlementData = Serializer.ConvertBytesToObject<PlayerSettlementData>(packet.contents);

            switch (settlementData.stepMode)
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
            if (CheckIfTileIsInUse(settlementData.settlementData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to add a settlement at tile {settlementData.settlementData.tile}, but that tile already has a settlement");
            else
            {
                settlementData.settlementData.owner = client.userFile.Username;

                SettlementFile settlementFile = new SettlementFile();
                settlementFile.tile = settlementData.settlementData.tile;
                settlementFile.owner = client.userFile.Username;
                Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, settlementFile.tile + fileExtension), settlementFile);

                settlementData.stepMode = SettlementStepMode.Add;
                foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementData.settlementData.goodwill = GoodwillManager.GetSettlementGoodwill(cClient, settlementFile);

                        Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                        cClient.listener.EnqueuePacket(rPacket);
                    }
                }

                Logger.Warning($"[Added settlement] > {settlementFile.tile} > {client.userFile.Username}");
            }
        }

        public static void RemoveSettlement(ServerClient client, PlayerSettlementData settlementData)
        {
            if (!CheckIfTileIsInUse(settlementData.settlementData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.settlementData.tile} was attempted to be removed, but the tile doesn't contain a settlement");

            SettlementFile settlementFile = GetSettlementFileFromTile(settlementData.settlementData.tile);

            if (client != null)
            {
                if (settlementFile.owner != client.userFile.Username) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.settlementData.tile} attempted to be removed by {client.userFile.Username}, but {settlementFile.owner} owns the settlement");
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
                File.Delete(Path.Combine(Master.settlementsPath, settlementFile.tile + fileExtension));

                Logger.Warning($"[Remove settlement] > {settlementFile.tile}");
            }

            void SendRemovalSignal()
            {
                settlementData.stepMode = SettlementStepMode.Remove;
                
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                NetworkHelper.SendPacketToAllClients(packet, client);
            }
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach(string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementJSON = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementJSON.tile == tileToCheck) return true;
            }

            return false;
        }

        public static SettlementFile GetSettlementFileFromTile(int tileToGet)
        {
            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.tile == tileToGet) return settlementFile;
            }

            return null;
        }

        public static SettlementFile GetSettlementFileFromUsername(string usernameToGet)
        {
            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.owner == usernameToGet) return settlementFile;
            }

            return null;
        }

        public static SettlementFile[] GetAllSettlements()
        {
            List<SettlementFile> settlementList = new List<SettlementFile>();

            string[] settlements = Directory.GetFiles(Master.settlementsPath);
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

            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(fileExtension)) continue;

                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.owner == usernameToCheck) settlementList.Add(settlementFile);
            }

            return settlementList.ToArray();
        }
    }
}
