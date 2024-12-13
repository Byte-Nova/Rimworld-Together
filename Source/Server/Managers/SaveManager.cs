using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SaveManager
    {
        //Variables

        public readonly static string fileExtension = ".mpsave";

        private readonly static string tempFileExtension = ".mpsavetemp";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            SaveData data = Serializer.ConvertBytesToObject<SaveData>(packet.contents);
            if (data._stepMode == SaveStepMode.Receive) ReceiveSavePartFromClient(client, data);
            else if (data._stepMode == SaveStepMode.Send) SendSavePartToClient(client);
            else if (data._stepMode == SaveStepMode.Reset) ResetClientSave(client);
            else ResponseShortcutManager.SendIllegalPacket(client, "Received invalid step mode");
        }

        public static void ReceiveSavePartFromClient(ServerClient client, SaveData data)
        {
            string baseClientSavePath = Path.Combine(Master.savesPath, client.userFile.Username + fileExtension);
            string tempClientSavePath = Path.Combine(Master.savesPath, client.userFile.Username + tempFileExtension);

            //if this is the first packet
            if (client.listener.downloadManager == null)
            {
                client.listener.downloadManager = new DownloadManager();
                client.listener.downloadManager.PrepareDownload(tempClientSavePath, data._fileParts);
            }

            client.listener.downloadManager.WriteFilePart(data._fileBytes);

            //if this is the last packet
            if (data._isLastPart)
            {
                client.listener.downloadManager.FinishFileWrite();
                client.listener.downloadManager = null;

                byte[] completedSave = File.ReadAllBytes(tempClientSavePath);
                File.WriteAllBytes(baseClientSavePath, completedSave);
                File.Delete(tempClientSavePath);

                OnUserSave(client, data);
            }

            else
            {
                SaveData rData = new SaveData();
                rData._stepMode = SaveStepMode.Send;

                Packet rPacket = Packet.CreatePacketFromObject(nameof(SaveManager), rData);
                client.listener.EnqueuePacket(rPacket);
            }
        }

        public static void SendSavePartToClient(ServerClient client)
        {
            string baseClientSavePath = Path.Combine(Master.savesPath, client.userFile.Username + fileExtension);
            string tempClientSavePath = Path.Combine(Master.savesPath, client.userFile.Username + tempFileExtension);

            //if this is the first packet
            if (client.listener.uploadManager == null)
            {
                Logger.Message($"[Load save] > {client.userFile.Username} | {client.userFile.SavedIP}");

                client.listener.uploadManager = new UploadManager();
                client.listener.uploadManager.PrepareUpload(baseClientSavePath);
            }

            SaveData data = new SaveData();
            data._fileSize = client.listener.uploadManager.fileSize;
            data._fileParts = client.listener.uploadManager.fileParts;
            data._fileBytes = client.listener.uploadManager.ReadFilePart();
            data._isLastPart = client.listener.uploadManager.isLastPart;
            data._stepMode = SaveStepMode.Receive;
            if(!Master.serverConfig.SyncLocalSave) data._instructions = (int)SaveMode.Strict;

            Packet packet = Packet.CreatePacketFromObject(nameof(SaveManager), data);
            client.listener.EnqueuePacket(packet);

            //if this is the last packet
            if (client.listener.uploadManager.isLastPart)
                client.listener.uploadManager = null;
        }

        private static void OnUserSave(ServerClient client, SaveData fileTransferData)
        {
            if (fileTransferData._instructions == (int)SaveMode.Disconnect)
            {
                client.listener.disconnectFlag = true;
                Logger.Message($"[Save game] > {client.userFile.Username} > Disconnect");
            }
            else Logger.Message($"[Save game] > {client.userFile.Username} > Autosave");
        }

        public static bool CheckIfUserHasSave(ServerClient client)
        {
            string[] saves = Directory.GetFiles(Master.savesPath);
            foreach(string save in saves)
            {
                if (!save.EndsWith(fileExtension)) continue;
                if (Path.GetFileNameWithoutExtension(save) == client.userFile.Username) return true;
            }

            return false;
        }

        public static byte[] GetUserSaveFromUsername(string username)
        {
            string[] saves = Directory.GetFiles(Master.savesPath);
            foreach (string save in saves)
            {
                if (!save.EndsWith(fileExtension)) continue;
                if (Path.GetFileNameWithoutExtension(save) == username) return File.ReadAllBytes(save);
            }

            return null;
        }

        public static void ResetClientSave(ServerClient client)
        {
            if (!CheckIfUserHasSave(client))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username}'s save was attempted to be reset while the player doesn't have a save");
                return;
            }
            client.listener.disconnectFlag = true;

            ResetPlayerData(client, client.userFile.Username);
        }

        public static void ResetPlayerData(ServerClient client, string username)
        {
            BackupManager.BackupUser(username);

            if (client != null) client.listener.disconnectFlag = true;

            // Delete save file
            try { File.Delete(Path.Combine(Master.savesPath, username + fileExtension)); }
            catch { Logger.Warning($"Failed to find {username}'s save"); }

            // Delete map files
            MapFile[] userMaps = MapManager.GetAllMapsFromUsername(username);
            foreach (MapFile map in userMaps) MapManager.DeleteMap(map);

            // Delete caravan files
            CaravanFile[] userCaravans = CaravanManagerHelper.GetCaravansFromOwner(username);
            foreach (CaravanFile caravan in userCaravans) CaravanManager.RemoveCaravan(username, caravan);

            // Delete site files
            SiteIdendityFile[] playerSites = SiteManagerHelper.GetAllSitesFromUsername(username);
            foreach (SiteIdendityFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            // Delete settlement files
            SettlementFile[] playerSettlements = PlayerSettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlement in playerSettlements)
            {
                PlayerSettlementData settlementData = new PlayerSettlementData();
                settlementData._settlementData.Tile = settlement.Tile;
                settlementData._settlementData.Owner = settlement.Owner;

                PlayerSettlementManager.RemoveSettlement(client, settlementData);
            }

            Logger.Warning($"[Reset player data] > {username}");
        }
    }
}
