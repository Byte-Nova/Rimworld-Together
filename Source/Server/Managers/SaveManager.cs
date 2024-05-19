using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SaveManager
    {
        public static void ReceiveSavePartFromClient(ServerClient client, Packet packet)
        {
            string baseClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsavetemp");

            FileTransferData fileTransferData = (FileTransferData)Serializer.ConvertBytesToObject(packet.contents);

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
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.RequestSavePartPacket));
                client.listener.EnqueuePacket(rPacket);
            }
        }

        public static void SendSavePartToClient(ServerClient client)
        {
            string baseClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Master.savesPath, client.username + ".mpsavetemp");

            //if this is the first packet
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

            //if this is the last packet
            if (client.listener.uploadManager.isLastPart)
                client.listener.uploadManager = null;
        }

        private static void OnUserSave(ServerClient client, FileTransferData fileTransferData)
        {
            Master.plugins.Emit("onUserSave_post", client, fileTransferData);

            if (fileTransferData.instructions == (int)SaveMode.Disconnect)
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
                ResponseShortcutManager.SendIllegalPacket(client, 
                    $"Player {client.username}'s save was attempted to be reset while the player doesn't have a save"); 

                return;
            }
            
            client.listener.disconnectFlag = true;

            string[] saves = Directory.GetFiles(Master.savesPath);

            string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == client.username);
            if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

            Logger.WriteToConsole($"[Delete save] > {client.username}", LogMode.Warning);

            MapFileData[] userMaps = MapManager.GetAllMapsFromUsername(client.username);
            foreach (MapFileData map in userMaps) MapManager.DeleteMap(map);

            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(client.username);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(client.username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                SettlementData settlementData = new SettlementData();
                settlementData.tile = settlementFile.tile;
                settlementData.owner = settlementFile.owner;

                SettlementManager.RemoveSettlement(client, settlementData);
            }
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
