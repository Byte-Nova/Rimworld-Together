using Shared;
using static Shared.CommonEnumerators;
using System.Linq.Expressions;

namespace GameServer
{
    public static class SaveManager
    {
        public static void ReceiveSavePartFromClient(ServerClient client, Packet packet)
        {
            string baseClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsavetemp");

            FileTransferData fileTransferData = (FileTransferData)Serializer.ConvertBytesToObject(packet.contents);

            if (client.listener.downloadManager == null)
            {
                client.listener.downloadManager = new DownloadManager();
                client.listener.downloadManager.PrepareDownload(tempClientSavePath, fileTransferData.fileParts);
            }

            client.listener.downloadManager.WriteFilePart(fileTransferData.fileBytes);

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
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.RequestSavePartPacket));
                client.listener.EnqueuePacket(rPacket);
            }
        }

        public static void SendSavePartToClient(ServerClient client)
        {
            string baseClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsavetemp");

            if (client.listener.uploadManager == null)
            {
                Logger.WriteToConsole($"[Load save] > {client.username} | {client.SavedIP}");

                client.listener.uploadManager = new UploadManager();
                client.listener.uploadManager.PrepareUpload(baseClientSavePath);
            }

            FileTransferData fileTransferData = new FileTransferData();
            fileTransferData.fileSize = client.listener.uploadManager.fileSize;
            fileTransferData.fileParts = client.listener.uploadManager.fileParts;
            fileTransferData.fileBytes = client.listener.uploadManager.ReadFilePart();
            fileTransferData.isLastPart = client.listener.uploadManager.isLastPart;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferData);
            client.listener.EnqueuePacket(packet);

            if (client.listener.uploadManager.isLastPart)
            {
                client.listener.uploadManager = null;
            }
        }

        private static void OnUserSave(ServerClient client, FileTransferData fileTransferData)
        {
            if (fileTransferData.additionalInstructions == ((int)CommonEnumerators.SaveMode.Disconnect).ToString())
            {
                client.listener.disconnectFlag = true;
                Logger.WriteToConsole($"[Save game] > {client.username} > Disconnect");
            }
            else Logger.WriteToConsole($"[Save game] > {client.username} > Autosave");
        }

        public static bool CheckIfUserHasSave(ServerClient client)
        {
            string[] saves = Directory.GetFiles(Master.savesPath);
            foreach(string save in saves)
            {
                if (!save.EndsWith(".mpsave")) continue;
                if (Path.GetFileNameWithoutExtension(save) == client.username) return true;
            }

            return false;
        }

        public static byte[] GetUserSaveFromUsername(string username)
        {
            string[] saves = Directory.GetFiles(Master.savesPath);
            foreach (string save in saves)
            {
                if (!save.EndsWith(".mpsave")) continue;
                if (Path.GetFileNameWithoutExtension(save) == username) return File.ReadAllBytes(save);
            }

            return null;
        }

        public static void ResetClientSave(ServerClient client)
        {
            if (!CheckIfUserHasSave(client))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username}'s save was attempted to be reset while the player doesn't have a save");
                return;
            }

            string playerArchivedSavePath = Path.Combine(Master.archivedSavesPath, client.username);

            if (Directory.Exists(playerArchivedSavePath)) Directory.Delete(playerArchivedSavePath,true);
            Directory.CreateDirectory(playerArchivedSavePath);

            string mapsArchivePath = Path.Combine(playerArchivedSavePath, "Maps");
            string savesArchivePath = Path.Combine(playerArchivedSavePath, "Saves");
            string settlementsArchivePath = Path.Combine(playerArchivedSavePath, "Settlements");
            string SitesArchivePath = Path.Combine(playerArchivedSavePath, "Sites");

            Directory.CreateDirectory(mapsArchivePath);
            Directory.CreateDirectory(savesArchivePath);
            Directory.CreateDirectory(settlementsArchivePath);
            Directory.CreateDirectory(SitesArchivePath);

            client.listener.disconnectFlag = true;

            string[] saves = Directory.GetFiles(Master.savesPath);

            try{ File.Move(Path.Combine(Master.savesPath, client.username + ".mpsave"), Path.Combine(savesArchivePath , client.username + ".mpsave")); }
            catch { Logger.WriteToConsole($"Failed to find {client.username}'s save", Logger.LogMode.Warning); }
            Logger.WriteToConsole($"[Delete save] > {client.username}", Logger.LogMode.Warning);

            //move Map files to archive
            MapFileData[] userMaps = MapManager.GetAllMapsFromUsername(client.username);
            foreach (MapFileData map in userMaps)
                File.Move(Path.Combine(Master.mapsPath, map.mapTile + ".mpmap"), Path.Combine(mapsArchivePath, map.mapTile + ".mpmap"));

            //Move site files to archive
            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(client.username);
            foreach (SiteFile site in playerSites)
                File.Move(Path.Combine(Master.sitesPath, site.tile + ".json"), Path.Combine(mapsArchivePath, site.tile + ".json"));

            //Move SettlementFile to archive
            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(client.username);
            foreach (SettlementFile settlementFile in playerSettlements)
                File.Move(Path.Combine(Master.settlementsPath, settlementFile.tile + ".json"), Path.Combine(settlementsArchivePath, settlementFile.tile + ".json"));
        }

        public static void DeletePlayerData(string username)
        {
            ServerClient connectedUser = UserManager.GetConnectedClientFromUsername(username);
            if (connectedUser != null) connectedUser.listener.disconnectFlag = true;

            string[] saves = Directory.GetFiles(Master.savesPath);
            string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == username);
            if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

            MapFileData[] userMaps = MapManager.GetAllMapsFromUsername(username);
            foreach (MapFileData map in userMaps) MapManager.DeleteMap(map);

            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(username);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                SettlementData settlementData = new SettlementData();
                settlementData.tile = settlementFile.tile;
                settlementData.owner = settlementFile.owner;

                SettlementManager.RemoveSettlement(null, settlementData, false);
            }

            Logger.WriteToConsole($"[Deleted player data] > {username}", LogMode.Warning);
        }
    }
}
