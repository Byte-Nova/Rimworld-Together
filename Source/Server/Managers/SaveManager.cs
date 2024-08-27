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
                client.listener.downloadManager.PrepareDownload(tempClientSavePath, fileTransferData.fileParts);
            }

            client.listener.downloadManager.WriteFilePart(fileTransferData.fileBytes);

            //if this is the last packet
            if (fileTransferData.isLastPart)
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
            fileTransferData.fileSize = client.listener.uploadManager.fileSize;
            fileTransferData.fileParts = client.listener.uploadManager.fileParts;
            fileTransferData.fileBytes = client.listener.uploadManager.ReadFilePart();
            fileTransferData.isLastPart = client.listener.uploadManager.isLastPart;
            if(!Master.serverConfig.SyncLocalSave) fileTransferData.instructions = (int)SaveMode.Strict;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferData);
            client.listener.EnqueuePacket(packet);

            //if this is the last packet
            if (client.listener.uploadManager.isLastPart)
                client.listener.uploadManager = null;
        }

        private static void OnUserSave(ServerClient client, FileTransferData fileTransferData)
        {
            if (fileTransferData.instructions == (int)SaveMode.Disconnect)
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

            //Locate and make sure there's no other backup save in the server
            string playerArchivedSavePath = Path.Combine(Master.backupUsersPath, client.userFile.Username);
            if (Directory.Exists(playerArchivedSavePath)) Directory.Delete(playerArchivedSavePath,true);
            Directory.CreateDirectory(playerArchivedSavePath);

            //Assign save paths to the backup files
            string mapsArchivePath = Path.Combine(playerArchivedSavePath, "Maps");
            string savesArchivePath = Path.Combine(playerArchivedSavePath, "Saves");
            string sitesArchivePath = Path.Combine(playerArchivedSavePath, "Sites");
            string settlementsArchivePath = Path.Combine(playerArchivedSavePath, "Settlements");

            //Create directories for the backup files
            Directory.CreateDirectory(mapsArchivePath);
            Directory.CreateDirectory(savesArchivePath);
            Directory.CreateDirectory(sitesArchivePath);
            Directory.CreateDirectory(settlementsArchivePath);

            //Copy save file to archive
            try { File.Copy(Path.Combine(Master.savesPath, client.userFile.Username + fileExtension), Path.Combine(savesArchivePath , client.userFile.Username + fileExtension)); }
            catch { Logger.Warning($"Failed to find {client.userFile.Username}'s save"); }

            //Copy map files to archive
            MapData[] userMaps = MapManager.GetAllMapsFromUsername(client.userFile.Username);
            foreach (MapData map in userMaps)
            {
                File.Copy(Path.Combine(Master.mapsPath, map.mapTile + MapManager.fileExtension), 
                    Path.Combine(mapsArchivePath, map.mapTile + MapManager.fileExtension));
            }

            //Copy site files to archive
            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(client.userFile.Username);
            foreach (SiteFile site in playerSites)
            {
                File.Copy(Path.Combine(Master.sitesPath, site.tile + SiteManager.fileExtension), 
                    Path.Combine(sitesArchivePath, site.tile + SiteManager.fileExtension));
            }

            //Copy settlement files to archive
            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(client.userFile.Username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                File.Copy(Path.Combine(Master.settlementsPath, settlementFile.tile + SettlementManager.fileExtension), 
                    Path.Combine(settlementsArchivePath, settlementFile.tile + SettlementManager.fileExtension));
            }

            ResetPlayerData(client, client.userFile.Username);
        }

        public static void ResetPlayerData(ServerClient client, string username)
        {
            if (client != null) client.listener.disconnectFlag = true;

            //Delete save file
            try { File.Delete(Path.Combine(Master.savesPath, username + fileExtension)); }
            catch { Logger.Warning($"Failed to find {username}'s save"); }

            //Delete map files
            MapData[] userMaps = MapManager.GetAllMapsFromUsername(username);
            foreach (MapData map in userMaps) MapManager.DeleteMap(map);

            //Delete site files
            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(username);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            //Delete settlement files
            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                PlayerSettlementData settlementData = new PlayerSettlementData();
                settlementData.settlementData.tile = settlementFile.tile;
                settlementData.settlementData.owner = settlementFile.owner;

                SettlementManager.RemoveSettlement(client, settlementData);
            }

            Logger.Warning($"[Reseted player data] > {username}");
        }
    }
}
