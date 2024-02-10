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
using Shared.Network;

namespace RimworldTogether.GameServer.Managers
{
    public static class SaveManager
    {
        public static void ReceiveSavePartFromClient(ServerClient client, Packet packet)
        {
            string baseClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsavetemp");

            FileTransferJSON fileTransferJSON = (FileTransferJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            if (client.downloadManager == null)
            {
                client.downloadManager = new DownloadManager();
                client.downloadManager.PrepareDownload(tempClientSavePath, fileTransferJSON.fileParts);
            }

            client.downloadManager.WriteFilePart(fileTransferJSON.fileBytes);

            if (fileTransferJSON.isLastPart)
            {
                client.downloadManager.FinishFileWrite();
                client.downloadManager = null;

                byte[] saveBytes = File.ReadAllBytes(tempClientSavePath);
                byte[] compressedSave = GZip.Compress(saveBytes);

                File.WriteAllBytes(baseClientSavePath, compressedSave);
                File.Delete(tempClientSavePath);

                OnUserSave(client, fileTransferJSON);
            }

            else
            {
                Packet rPacket = Packet.CreatePacketFromJSON("RequestSavePartPacket");
                client.clientListener.SendData(rPacket);
            }
        }

        public static void SendSavePartToClient(ServerClient client)
        {
            string baseClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsavetemp");

            if (client.uploadManager == null)
            {
                Logger.WriteToConsole($"[Load save] > {client.username} | {client.SavedIP}");

                byte[] decompressedSave = GZip.Decompress(File.ReadAllBytes(baseClientSavePath));
                File.WriteAllBytes(tempClientSavePath, decompressedSave);

                client.uploadManager = new UploadManager();
                client.uploadManager.PrepareUpload(tempClientSavePath);
            }

            FileTransferJSON fileTransferJSON = new FileTransferJSON();
            fileTransferJSON.fileSize = client.uploadManager.fileSize;
            fileTransferJSON.fileParts = client.uploadManager.fileParts;
            fileTransferJSON.fileBytes = client.uploadManager.ReadFilePart();
            fileTransferJSON.isLastPart = client.uploadManager.isLastPart;

            Packet packet = Packet.CreatePacketFromJSON("ReceiveSavePartPacket", fileTransferJSON);
            client.clientListener.SendData(packet);

            if (client.uploadManager.isLastPart)
            {
                File.Delete(tempClientSavePath);
                client.uploadManager = null;
            }
        }

        private static void OnUserSave(ServerClient client, FileTransferJSON fileTransferJSON)
        {
            if (fileTransferJSON.additionalInstructions == ((int)CommonEnumerators.SaveMode.Disconnect).ToString())
            {
                CommandManager.SendDisconnectCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > To menu");
            }

            else if (fileTransferJSON.additionalInstructions == ((int)CommonEnumerators.SaveMode.Quit).ToString())
            {
                CommandManager.SendQuitCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > Quiting");
            }

            else if (fileTransferJSON.additionalInstructions == ((int)CommonEnumerators.SaveMode.Transfer).ToString())
            {
                Logger.WriteToConsole($"[Save game] > {client.username} > Item transfer");
            }

            else Logger.WriteToConsole($"[Save game] > {client.username} > Autosave");
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
