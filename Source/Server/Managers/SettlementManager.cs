using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SettlementManager
    {
        public static void ParseSettlementPacket(ServerClient client, Packet packet)
        {
            SettlementData settlementData = (SettlementData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(settlementData.settlementStepMode))
            {
                case (int)CommonEnumerators.SettlementStepMode.Add:
                    AddSettlement(client, settlementData);
                    break;

                case (int)CommonEnumerators.SettlementStepMode.Remove:
                    RemoveSettlement(client, settlementData);
                    break;
            }
        }

        public static void AddSettlement(ServerClient client, SettlementData settlementData)
        {
            if (CheckIfTileIsInUse(settlementData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to add a settlement at tile {settlementData.tile}, but that tile already has a settlement");
            else
            {
                settlementData.owner = client.username;

                SettlementFile settlementFile = new SettlementFile();
                settlementFile.tile = settlementData.tile;
                settlementFile.owner = client.username;
                Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, settlementFile.tile + ".json"), settlementFile);

                settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();
                foreach (ServerClient cClient in Network.connectedClients.ToArray())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementData.value = GoodwillManager.GetSettlementGoodwill(cClient, settlementFile).ToString();

                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                        cClient.listener.EnqueuePacket(rPacket);
                    }
                }

                ConsoleManager.WriteToConsole($"[Added settlement] > {settlementFile.tile} > {client.username}", LogMode.Warning);
            }
        }

        public static void RemoveSettlement(ServerClient client, SettlementData settlementData, bool sendRemoval = true)
        {
            if (!CheckIfTileIsInUse(settlementData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.tile} was attempted to be removed, but the tile doesn't contain a settlement");

            SettlementFile settlementFile = GetSettlementFileFromTile(settlementData.tile);

            if (sendRemoval)
            {
                if (settlementFile.owner != client.username) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.tile} attempted to be removed by {client.username}, but {settlementFile.owner} owns the settlement");
                else
                {
                    File.Delete(Path.Combine(Master.settlementsPath, settlementFile.tile + ".json"));

                    settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Remove).ToString();
                    Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                    foreach (ServerClient cClient in Network.connectedClients.ToArray())
                    {
                        if (cClient == client) continue;
                        else cClient.listener.EnqueuePacket(rPacket);
                    }

                    ConsoleManager.WriteToConsole($"[Remove settlement] > {settlementData.tile} > {client.username}", LogMode.Warning);
                }
            }

            else
            {
                File.Delete(Path.Combine(Master.settlementsPath, settlementFile.tile + ".json"));

                ConsoleManager.WriteToConsole($"[Remove settlement] > {settlementFile.tile}", LogMode.Warning);
            }
        }

        public static bool CheckIfTileIsInUse(string tileToCheck)
        {
            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach(string settlement in settlements)
            {
                if (!settlement.EndsWith(".json")) continue;
                SettlementFile settlementJSON = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementJSON.tile == tileToCheck) return true;
            }

            return false;
        }

        public static SettlementFile GetSettlementFileFromTile(string tileToGet)
        {
            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach (string settlement in settlements)
            {
                if (!settlement.EndsWith(".json")) continue;
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
                if (!settlement.EndsWith(".json")) continue;
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
                if (!settlement.EndsWith(".json")) continue;
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
                if (!settlement.EndsWith(".json")) continue;
                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.owner == usernameToCheck) settlementList.Add(settlementFile);
            }

            return settlementList.ToArray();
        }
    }
}
