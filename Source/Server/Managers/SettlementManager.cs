﻿using Shared;

namespace GameServer
{
    public static class SettlementManager
    {
        public static void ParseSettlementPacket(ServerClient client, Packet packet)
        {
            SettlementDetailsJSON settlementDetailsJSON = (SettlementDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(settlementDetailsJSON.settlementStepMode))
            {
                case (int)CommonEnumerators.SettlementStepMode.Add:
                    AddSettlement(client, settlementDetailsJSON);
                    break;

                case (int)CommonEnumerators.SettlementStepMode.Remove:
                    RemoveSettlement(client, settlementDetailsJSON);
                    break;
            }
        }

        public static void AddSettlement(ServerClient client, SettlementDetailsJSON settlementDetailsJSON)
        {
            if (CheckIfTileIsInUse(settlementDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to add a settlement at tile {settlementDetailsJSON.tile}, but that tile already has a settlement");
            else
            {
                settlementDetailsJSON.owner = client.username;

                SettlementFile settlementFile = new SettlementFile();
                settlementFile.tile = settlementDetailsJSON.tile;
                settlementFile.owner = client.username;
                Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, settlementFile.tile + ".json"), settlementFile);

                settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();
                foreach (ServerClient cClient in Network.connectedClients.ToArray())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementDetailsJSON.value = LikelihoodManager.GetSettlementLikelihood(cClient, settlementFile).ToString();

                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementDetailsJSON);
                        cClient.listener.EnqueuePacket(rPacket);
                    }
                }

                Logger.WriteToConsole($"[Added settlement] > {settlementFile.tile} > {client.username}", Logger.LogMode.Warning);
            }
        }

        public static void RemoveSettlement(ServerClient client, SettlementDetailsJSON settlementDetailsJSON, bool sendRemoval = true)
        {
            if (!CheckIfTileIsInUse(settlementDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementDetailsJSON.tile} was attempted to be removed, but the tile doesn't contain a settlement");

            SettlementFile settlementFile = GetSettlementFileFromTile(settlementDetailsJSON.tile);

            if (sendRemoval)
            {
                if (settlementFile.owner != client.username) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementDetailsJSON.tile} attempted to be removed by {client.username}, but {settlementFile.owner} owns the settlement");
                else
                {
                    File.Delete(Path.Combine(Master.settlementsPath, settlementFile.tile + ".json"));

                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Remove).ToString();
                    Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementDetailsJSON);
                    foreach (ServerClient cClient in Network.connectedClients.ToArray())
                    {
                        if (cClient == client) continue;
                        else cClient.listener.EnqueuePacket(rPacket);
                    }

                    Logger.WriteToConsole($"[Remove settlement] > {settlementDetailsJSON.tile} > {client.username}", Logger.LogMode.Warning);
                }
            }

            else
            {
                File.Delete(Path.Combine(Master.settlementsPath, settlementFile.tile + ".json"));

                Logger.WriteToConsole($"[Remove settlement] > {settlementFile.tile}", Logger.LogMode.Warning);
            }
        }

        public static bool CheckIfTileIsInUse(string tileToCheck)
        {
            string[] settlements = Directory.GetFiles(Master.settlementsPath);
            foreach(string settlement in settlements)
            {
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
                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.owner == usernameToCheck) settlementList.Add(settlementFile);
            }

            return settlementList.ToArray();
        }
    }
}
