using GameServer.Managers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimworldTogether;
using Shared.JSON;
using Shared.Misc;

namespace GameServer
{
    public static class SettlementManager
    {
        public enum SettlementStepMode { Add, Remove }

        public static void ParseSettlementPacket(Client client, Packet packet)
        {
            SettlementDetailsJSON settlementDetailsJSON = Serializer.SerializeFromString<SettlementDetailsJSON>(packet.contents[0]);

            switch (int.Parse(settlementDetailsJSON.settlementStepMode))
            {
                case (int)SettlementStepMode.Add:
                    AddSettlement(client, settlementDetailsJSON);
                    break;

                case (int)SettlementStepMode.Remove:
                    RemoveSettlement(client, settlementDetailsJSON);
                    break;
            }
        }

        public static bool CheckIfTileIsInUse(string tileToCheck)
        {
            string[] settlements = Directory.GetFiles(Program.settlementsPath);
            foreach(string settlement in settlements)
            {
                SettlementFile settlementJSON = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementJSON.tile == tileToCheck) return true;
            }

            return false;
        }

        public static SettlementFile GetSettlementFileFromTile(string tileToGet)
        {
            string[] settlements = Directory.GetFiles(Program.settlementsPath);
            foreach (string settlement in settlements)
            {
                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.tile == tileToGet) return settlementFile;
            }

            return null;
        }

        public static SettlementFile GetSettlementFileFromUsername(string usernameToGet)
        {
            string[] settlements = Directory.GetFiles(Program.settlementsPath);
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

            string[] settlements = Directory.GetFiles(Program.settlementsPath);
            foreach (string settlement in settlements)
            {
                settlementList.Add(Serializer.SerializeFromFile<SettlementFile>(settlement));
            }

            return settlementList.ToArray();
        }

        public static SettlementFile[] GetAllSettlementsFromUsername(string usernameToCheck)
        {
            List<SettlementFile> settlementList = new List<SettlementFile>();

            string[] settlements = Directory.GetFiles(Program.settlementsPath);
            foreach (string settlement in settlements)
            {
                SettlementFile settlementFile = Serializer.SerializeFromFile<SettlementFile>(settlement);
                if (settlementFile.owner == usernameToCheck) settlementList.Add(settlementFile);
            }

            return settlementList.ToArray();
        }

        public static void AddSettlement(Client client, SettlementDetailsJSON settlementDetailsJSON)
        {
            if (CheckIfTileIsInUse(settlementDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                settlementDetailsJSON.owner = client.username;

                SettlementFile settlement = new SettlementFile();
                settlement.tile = settlementDetailsJSON.tile;
                settlement.owner = client.username;
                Serializer.SerializeToFile(Path.Combine(Program.settlementsPath, settlement.tile + ".json"), settlement);

                settlementDetailsJSON.settlementStepMode = ((int)SettlementStepMode.Add).ToString();
                foreach (Client cClient in Network.connectedClients.ToArray())
                {
                    if (cClient == client) continue;
                    else
                    {
                        settlementDetailsJSON.value = LikelihoodManager.GetSettlementLikelihood(cClient, settlement).ToString();

                        string[] contents = new string[] { Serializer.SerializeToString(settlementDetailsJSON) };
                        Packet rPacket = new Packet("SettlementPacket", contents);
                        Network.SendData(cClient, rPacket);
                    }
                }

                Logger.WriteToConsole($"[Added settlement] > {settlement.tile} > {client.username}", Logger.LogMode.Warning);
            }
        }

        public static void RemoveSettlement(Client client, SettlementDetailsJSON settlementDetailsJSON)
        {
            if (!CheckIfTileIsInUse(settlementDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client);

            SettlementFile settlementJSON = GetSettlementFileFromTile(settlementDetailsJSON.tile);
            if (settlementJSON.owner != client.username) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                File.Delete(Path.Combine(Program.settlementsPath, settlementJSON.tile + ".json"));

                settlementDetailsJSON.settlementStepMode = ((int)SettlementStepMode.Remove).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(settlementDetailsJSON) };
                Packet rPacket = new Packet("SettlementPacket", contents);
                foreach (Client cClient in Network.connectedClients.ToArray())
                {
                    if (cClient == client) continue;
                    else Network.SendData(cClient, rPacket);
                }

                Logger.WriteToConsole($"[Removed settlement] > {settlementJSON.tile} > {client.username}", Logger.LogMode.Warning);
            }
        }
    }
}
