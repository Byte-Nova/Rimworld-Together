using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class SaveManager
    {
        // Variables

        public readonly static string fileExtension = ".mpsave";

        public readonly static string tempFileExtension = ".mpsavetemp";

        [HandlesPacket(PacketHeader.SaveManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            SaveData data = Serializer.ConvertBytesToObject<SaveData>(bytes);

            if (data._stepMode == SaveStepMode.Receive) SaveReceiverManager.ReceiveSaveFromClient(client, data);
            else if (data._stepMode == SaveStepMode.Send) SaveSenderManager.SendSaveToClient(client);
            else if (data._stepMode == SaveStepMode.Reset) ResetClientSave(client);
            else ResponseShortcutManager.SendIllegalPacket(client, "Received invalid step mode");
        }

        public static void OnUserSave(ServerClient client, SaveData fileTransferData)
        {
            if (fileTransferData._instructions == (int)SaveMode.Disconnect) client.Listener.DisconnectFlag = true;

            InformationDisplayer.DisplaySaveGame(client);
        }

        public static bool CheckIfUserHasSave(ServerClient client)
        {
            string[] saves = Directory.GetFiles(Master.SavesPath);
            foreach (string save in saves)
            {
                if (!save.EndsWith(fileExtension)) continue;
                if (Path.GetFileNameWithoutExtension(save) == client.UserFile.Uid) return true;
            }

            return false;
        }

        public static byte[] GetUserSaveFromUsername(string username)
        {
            string[] saves = Directory.GetFiles(Master.SavesPath);
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
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Uid}'s save was attempted to be reset while the player doesn't have a save");
                return;
            }
            client.Listener.DisconnectFlag = true;

            ResetPlayerData(client, client.UserFile.Uid);
        }

        public static void ResetPlayerData(ServerClient client, string uid)
        {
            BackupManager.BackupUser(uid);

            if (client != null) client.Listener.DisconnectFlag = true;

            // Delete save file
            try { File.Delete(Path.Combine(Master.SavesPath, uid + fileExtension)); }
            catch { Printer.Warning($"Failed to find {client.UserFile.Label}'s save"); }

            // Delete site files
            SiteFile[] playerSites = SiteManagerHelper.GetAllSitesFromUID(uid);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            // Delete settlement files
            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(uid);
            foreach (SettlementFile settlement in playerSettlements)
            {
                PlayerSettlementData settlementData = new PlayerSettlementData();
                settlementData._settlementFile.Tile = settlement.Tile;
                settlementData._settlementFile.UID = settlement.UID;

                SettlementManager.RemoveSettlement(client, settlementData);
            }

            InformationDisplayer.DisplayResetPlayer(uid);
        }
    }

    public static class SaveSenderManager
    {
        public static void SendSaveToClient(ServerClient client)
        {
            string baseClientSavePath = Path.Combine(Master.SavesPath, client.UserFile.Uid + SaveManager.fileExtension);

            InformationDisplayer.DisplayLoadGame(client);

            SaveData data = new SaveData();
            data._fileBytes = File.ReadAllBytes(baseClientSavePath);
            data._stepMode = SaveStepMode.Receive;
            if (!Master.ServerConfig.SyncLocalSave) data._instructions = SaveMode.Strict;

            client.Listener.EnqueuePacket(PacketHeader.SaveManager, data);
        }
    }

    public static class SaveReceiverManager
    {
        public static void ReceiveSaveFromClient(ServerClient client, SaveData data)
        {
            string baseClientSavePath = Path.Combine(Master.SavesPath, client.UserFile.Uid + SaveManager.fileExtension);
            string tempClientSavePath = Path.Combine(Master.TempPath, client.UserFile.Uid + SaveManager.tempFileExtension);

            File.WriteAllBytes(tempClientSavePath, data._fileBytes);

            OnSaveReceived(client, data, baseClientSavePath, tempClientSavePath);
        }

        private static void OnSaveReceived(ServerClient client, SaveData data, string baseClientSavePath, string tempClientSavePath)
        {
            byte[] completedSave = File.ReadAllBytes(tempClientSavePath);
            File.WriteAllBytes(baseClientSavePath, completedSave);
            File.Delete(tempClientSavePath);

            SaveManager.OnUserSave(client, data);
        }
    }
}
