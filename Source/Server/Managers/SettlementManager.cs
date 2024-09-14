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

            if (settlementData._settlementData.isShip)
            {
                PlayerShipData data = Serializer.ConvertBytesToObject<PlayerShipData>(packet.contents);
                switch (settlementData._stepMode)
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
        }

        public static void AddSettlement(ServerClient client, PlayerSettlementData settlementData)
        {
            if (CheckIfTileIsInUse(settlementData._settlementData.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to add a settlement at tile {settlementData._settlementData.Tile}, but that tile already has a settlement");
            else
            {
                settlementData._settlementData.Owner = client.userFile.Username;

                SettlementFile settlementFile = new SettlementFile();
                settlementFile.Tile = settlementData._settlementData.Tile;
                settlementFile.Owner = client.userFile.Username;
                settlementFile.isShip = settlementData._settlementData.isShip;
                Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, settlementFile.Tile + fileExtension), settlementFile);

                settlementData._stepMode = SettlementStepMode.Add;
                foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementData._settlementData.Goodwill = GoodwillManager.GetSettlementGoodwill(cClient, settlementFile);

                        Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                        cClient.listener.EnqueuePacket(rPacket);
                    }
                }

                Logger.Warning($"[Added settlement] > {settlementFile.Tile} > {client.userFile.Username}");
            }
        }

        public static void RemoveSettlement(ServerClient client, PlayerSettlementData settlementData)
        {
            if (!CheckIfTileIsInUse(settlementData._settlementData.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData._settlementData.Tile} was attempted to be removed, but the tile doesn't contain a settlement");

            SettlementFile settlementFile = GetSettlementFileFromTile(settlementData._settlementData.Tile);

            if (client != null)
            {
                if (settlementFile.Owner != client.userFile.Username) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData._settlementData.Tile} attempted to be removed by {client.userFile.Username}, but {settlementFile.Owner} owns the settlement");
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
                File.Delete(Path.Combine(Master.settlementsPath, settlementFile.Tile + fileExtension));

                Logger.Warning($"[Remove settlement] > {settlementFile.Tile}");
            }

            void SendRemovalSignal()
            {
                settlementData._stepMode = SettlementStepMode.Remove;
                
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
                if (settlementJSON.Tile == tileToCheck) return true;
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
                if (settlementFile.Tile == tileToGet)
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
                if (settlementFile.Owner == usernameToGet)
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
                if (settlementFile.Owner == usernameToCheck) settlementList.Add(settlementFile);
            }

            return settlementList.ToArray();
        }

        //SOS2
        public static void SpaceAddSettlement(ServerClient client, PlayerShipData settlementData)
        {
            if (CheckIfTileIsInUse(settlementData._settlementData.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"[SOS2]Player {client.userFile.Username} attempted to add a ship at tile {settlementData._settlementData.Tile}, but that tile already has a settlement");
            else
            {
                settlementData._settlementData.Owner = client.userFile.Username;

                SpaceSettlementFile settlementFile = new SpaceSettlementFile();
                settlementFile.Tile = settlementData._settlementData.Tile;
                settlementFile.Owner = client.userFile.Username;
                settlementFile.phi = settlementData._phi;
                settlementFile.radius = settlementData._radius;
                settlementFile.theta = settlementData._theta;
                settlementFile.isShip = settlementData._settlementData.isShip;
                Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, settlementFile.Tile + fileExtension), settlementFile);

                settlementData._stepMode = SettlementStepMode.Add;
                foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementData._settlementData.Goodwill = GoodwillManager.GetSettlementGoodwill(cClient, settlementFile);

                        Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.SpaceSettlementPacket), settlementData);
                        cClient.listener.EnqueuePacket(rPacket);
                    }
                }

                Logger.Warning($"[SOS2][Added space settlement] > {settlementFile.Tile} > {client.userFile.Username}");
            }
        }
    }

}
