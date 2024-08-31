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

            if (settlementData.settlementData.isShip)
            {
                SpaceSettlementData data = Serializer.ConvertBytesToObject<SpaceSettlementData>(packet.contents);
                switch (settlementData.stepMode)
                {
                    case SettlementStepMode.Add:
                        SpaceAddSettlement(client, data);
                        break;

                    case SettlementStepMode.Remove:
                        RemoveSettlement(client, data);
                        break;
                }
            }
            else
            {
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
                settlementFile.isShip = settlementData.settlementData.isShip;
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
                if (settlementFile.tile == tileToGet)
                {
                    if (settlementFile.isShip)
                    {
                        SpaceSettlementFile fileData = Serializer.SerializeFromFile<SpaceSettlementFile>(settlement);
                        return fileData;
                    } else 
                    {
                        return settlementFile;
                    }
                }
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
                if (settlementFile.owner == usernameToGet)
                {
                    if (settlementFile.isShip)
                    {
                        SpaceSettlementFile fileData = Serializer.SerializeFromFile<SpaceSettlementFile>(settlement);
                        return fileData;
                    }
                    else
                    {
                        return settlementFile;
                    }
                }
            }

            return null;
        }

        public static SettlementFile[] GetAllSettlements()
        {
            List<SettlementFile> settlementList = new List<SettlementFile>();

            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach (string settlementFile in settlements)
            {
                if (!settlementFile.EndsWith(fileExtension)) continue;
                SettlementFile settlement = Serializer.SerializeFromFile<SettlementFile>(settlementFile);
                if (settlement.isShip) 
                {
                    settlement = Serializer.SerializeFromFile<SpaceSettlementFile>(settlementFile);
                }
                settlementList.Add(settlement);
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

        //SOS2
        public static void SpaceAddSettlement(ServerClient client, SpaceSettlementData settlementData)
        {
            if (CheckIfTileIsInUse(settlementData.settlementData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"[SOS2]Player {client.userFile.Username} attempted to add a ship at tile {settlementData.settlementData.tile}, but that tile already has a settlement");
            else
            {
                settlementData.settlementData.owner = client.userFile.Username;

                SpaceSettlementFile settlementFile = new SpaceSettlementFile();
                settlementFile.tile = settlementData.settlementData.tile;
                settlementFile.owner = client.userFile.Username;
                settlementFile.phi = settlementData.phi;
                settlementFile.radius = settlementData.radius;
                settlementFile.theta = settlementData.theta;
                settlementFile.isShip = settlementData.settlementData.isShip;
                Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, settlementFile.tile + fileExtension), settlementFile);

                settlementData.stepMode = SettlementStepMode.Add;
                foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementData.settlementData.goodwill = GoodwillManager.GetSettlementGoodwill(cClient, settlementFile);

                        Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.SpaceSettlementPacket), settlementData);
                        cClient.listener.EnqueuePacket(rPacket);
                    }
                }

                Logger.Warning($"[SOS2][Added space settlement] > {settlementFile.tile} > {client.userFile.Username}");
            }
        }
    }

}
