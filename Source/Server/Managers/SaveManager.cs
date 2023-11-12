using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.JSON;
using Shared.Misc;


namespace RimworldTogether.GameServer.Managers
{
    public static class SaveManager
    {
        public static void SaveUserGame(ServerClient client, Packet packet)
        {
            SaveFileJSON saveFileJSON = (SaveFileJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            byte[] compressedSave = GZip.Compress(saveFileJSON.saveData);

            File.WriteAllBytes(Path.Combine(Program.savesPath, client.username + ".mpsave"), compressedSave);

            if (saveFileJSON.saveMode == ((int)CommonEnumerators.SaveMode.Disconnect).ToString())
            {
                CommandManager.SendDisconnectCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > To menu");
            }

            else if (saveFileJSON.saveMode == ((int)CommonEnumerators.SaveMode.Quit).ToString())
            {
                CommandManager.SendQuitCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > Quiting");
            }

            else if (saveFileJSON.saveMode == ((int)CommonEnumerators.SaveMode.Transfer).ToString())
            {
                Logger.WriteToConsole($"[Save game] > {client.username} > Item transfer");
            }

            else Logger.WriteToConsole($"[Save game] > {client.username} > Autosave");
        }

        public static void LoadUserGame(ServerClient client)
        {
            byte[] decompressedSave = GZip.Decompress(File.ReadAllBytes(Path.Combine(Program.savesPath, client.username + ".mpsave")));
            SaveFileJSON saveFileJSON = new SaveFileJSON();
            saveFileJSON.saveData = decompressedSave;

            Packet packet = Packet.CreatePacketFromJSON("LoadFilePacket", saveFileJSON);
            client.clientListener.SendData(packet);

            Logger.WriteToConsole($"[Load game] > {client.username}");
        }

        public static bool CheckIfUserHasSave(ServerClient client)
        {
            string[] saves = Directory.GetFiles(Program.savesPath);
            foreach(string save in saves)
            {
                if (Path.GetFileNameWithoutExtension(save) == client.username)
                {
                    return true;
                }
            }

            return false;
        }

        public static byte[] GetUserSaveFromUsername(string username)
        {
            string[] saves = Directory.GetFiles(Program.savesPath);
            foreach (string save in saves)
            {
                if (Path.GetFileNameWithoutExtension(save) == username)
                {
                    return File.ReadAllBytes(save);
                }
            }

            return null;
        }

        public static void ResetClientSave(ServerClient client)
        {
            if (!CheckIfUserHasSave(client)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                client.disconnectFlag = true;

                string[] saves = Directory.GetFiles(Program.savesPath);

                string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == client.username);
                if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

                Logger.WriteToConsole($"[Delete save] > {client.username}", Logger.LogMode.Warning);

                MapFileJSON[] userMaps = MapManager.GetAllMapsFromUsername(client.username);
                foreach (MapFileJSON map in userMaps) MapManager.DeleteMap(map);

                SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(client.username);
                foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

                SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(client.username);
                foreach (SettlementFile settlementFile in playerSettlements)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = settlementFile.tile;
                    settlementDetailsJSON.owner = settlementFile.owner;

                    SettlementManager.RemoveSettlement(client, settlementDetailsJSON);
                }
            }
        }

        public static void DeletePlayerDetails(string username)
        {
            ServerClient connectedUser = UserManager.GetConnectedClientFromUsername(username);
            if (connectedUser != null) connectedUser.disconnectFlag = true;

            string[] saves = Directory.GetFiles(Program.savesPath);
            string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == username);
            if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

            MapFileJSON[] userMaps = MapManager.GetAllMapsFromUsername(username);
            foreach (MapFileJSON map in userMaps) MapManager.DeleteMap(map);

            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(username);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                settlementDetailsJSON.tile = settlementFile.tile;
                settlementDetailsJSON.owner = settlementFile.owner;

                SettlementManager.RemoveSettlement(null, settlementDetailsJSON, false);
            }

            Logger.WriteToConsole($"[Deleted player details] > {username}", Logger.LogMode.Warning);
        }
    }
}
