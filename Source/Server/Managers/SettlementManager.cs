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
            SettlementData settlementData = (SettlementData)Serializer.ConvertBytesToObject(packet.contents);

            switch (settlementData.settlementStepMode)
            {
                case SettlementStepMode.Add:
                    AddSettlement(client, settlementData);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSettlement(client, settlementData);
                    break;
            }
        }

        public static void AddSettlement(ServerClient client, SettlementData settlementData)
        {
            if (CheckIfTileIsInUse(settlementData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to add a settlement at tile {settlementData.tile}, but that tile already has a settlement");
            else
            {
                settlementData.owner = client.userFile.Username;

                SettlementFile settlementFile = new SettlementFile();
                settlementFile.tile = settlementData.tile;
                settlementFile.owner = client.userFile.Username;
                Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, settlementFile.tile + fileExtension), settlementFile);

                settlementData.settlementStepMode = SettlementStepMode.Add;
                foreach (ServerClient cClient in Network.connectedClients.ToArray())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementData.goodwill = GoodwillManager.GetSettlementGoodwill(cClient, settlementFile);

                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                        cClient.listener.EnqueuePacket(rPacket);
                    }
                }

                Logger.Warning($"[Added settlement] > {settlementFile.tile} > {client.userFile.Username}");
            }
        }

        public static void RemoveSettlement(ServerClient client, SettlementData settlementData, bool sendRemoval = true)
        {
            if (!CheckIfTileIsInUse(settlementData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.tile} was attempted to be removed, but the tile doesn't contain a settlement");

            SettlementFile settlementFile = GetSettlementFileFromTile(settlementData.tile);

            if (sendRemoval)
            {
                if (client != null)
                {
                    if (settlementFile.owner != client.userFile.Username) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.tile} attempted to be removed by {client.userFile.Username}, but {settlementFile.owner} owns the settlement");
                    else
                    {
                        Delete();
                        SendRemovalSignal();
                        client.listener.disconnectFlag = true;
                    }
                }

                else
                {
                    Delete();
                    SendRemovalSignal();
                }
            }
            else Delete();

            void Delete()
            {
                File.Delete(Path.Combine(Master.settlementsPath, settlementFile.tile + fileExtension));

                Logger.Warning($"[Remove settlement] > {settlementFile.tile}");
            }

            void SendRemovalSignal()
            {
                settlementData.settlementStepMode = SettlementStepMode.Remove;
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                foreach (ServerClient cClient in Network.connectedClients.ToArray())
                {
                    if (cClient == client) continue;
                    else cClient.listener.EnqueuePacket(rPacket);
                }
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
