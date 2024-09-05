using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SaveManager
    {
        //Variables

        public readonly static string fileExtension = ".mpsave";
        private readonly static string tempFileExtension = ".mpsavetemp";

        public static void ReceiveSavePartFromClient(ServerClient client, Packet packet)
        {
            string baseClientSavePath = Path.Combine(Master.savesPath, client.userFile.Username + fileExtension);
            string tempClientSavePath = Path.Combine(Master.savesPath, client.userFile.Username + tempFileExtension);

            FileTransferData fileTransferData = Serializer.ConvertBytesToObject<FileTransferData>(packet.contents);

            //if this is the first packet
            if (client.listener.downloadManager == null)
            {
                client.listener.downloadManager = new DownloadManager();
                client.listener.downloadManager.PrepareDownload(tempClientSavePath, fileTransferData._fileParts);
            }

            client.listener.downloadManager.WriteFilePart(fileTransferData._fileBytes);

            //if this is the last packet
            if (fileTransferData._isLastPart)
            {
                client.listener.downloadManager.FinishFileWrite();
                client.listener.downloadManager = null;

                byte[] completedSave = File.ReadAllBytes(tempClientSavePath);
                File.WriteAllBytes(baseClientSavePath, completedSave);
                File.Delete(tempClientSavePath);

                OnUserSave(client, fileTransferData);
            }

            else
            {
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.RequestSavePartPacket));
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

            FileTransferData fileTransferData = new FileTransferData();
            fileTransferData._fileSize = client.listener.uploadManager.fileSize;
            fileTransferData._fileParts = client.listener.uploadManager.fileParts;
            fileTransferData._fileBytes = client.listener.uploadManager.ReadFilePart();
            fileTransferData._isLastPart = client.listener.uploadManager.isLastPart;
            if(!Master.serverConfig.SyncLocalSave) fileTransferData._instructions = (int)SaveMode.Strict;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferData);
            client.listener.EnqueuePacket(packet);

            //if this is the last packet
            if (client.listener.uploadManager.isLastPart)
                client.listener.uploadManager = null;
        }

        private static void OnUserSave(ServerClient client, FileTransferData fileTransferData)
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

            //Delete save file
            try { File.Delete(Path.Combine(Master.savesPath, username + fileExtension)); }
            catch { Logger.Warning($"Failed to find {username}'s save"); }

            //Delete map files
            MapData[] userMaps = MapManager.GetAllMapsFromUsername(username);
            foreach (MapData map in userMaps) MapManager.DeleteMap(map);

            //Delete site files
            SiteFile[] playerSites = SiteManagerHelper.GetAllSitesFromUsername(username);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            //Delete settlement files
            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                PlayerSettlementData settlementData = new PlayerSettlementData();
                settlementData._settlementData.Tile = settlementFile.Tile;
                settlementData._settlementData.Owner = settlementFile.Owner;

                SettlementManager.RemoveSettlement(client, settlementData);
            }

            Logger.Warning($"[Reseted player data] > {username}");
        }
    }
}
