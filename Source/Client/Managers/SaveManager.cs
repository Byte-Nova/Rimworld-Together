using HarmonyLib;
using RimWorld;
using Shared;
using System.IO;
using System.Reflection;
using Verse;

namespace GameClient
{
    public static class SaveManager
    {
        public static string customSaveName = "ServerSave";
        private static string tempSaveFilePath;
        private static string saveFilePath;

        public static void ForceSave()
        {
            FieldInfo FticksSinceSave = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");
            FticksSinceSave.SetValue(Current.Game.autosaver, 0);

            ClientValues.autosaveCurrentTicks = 0;

            customSaveName = $"Server - {Network.ip} - {ClientValues.username}";
            GameDataSaveLoader.SaveGame(customSaveName);
        }

        public static void ReceiveSavePartFromServer(Packet packet)
        {
            FileTransferData fileTransferData = (FileTransferData)Serializer.ConvertBytesToObject(packet.contents);

            if (Network.listener.downloadManager == null)
            {
                Logger.Message($"Receiving save from server");

                customSaveName = $"Server - {Network.ip} - {ClientValues.username}";
                tempSaveFilePath = Path.Combine(new string[] { Master.savesFolderPath, customSaveName + ".rws.temp" });
                saveFilePath = Path.Combine(new string[] { Master.savesFolderPath, customSaveName + ".rws" });

                Network.listener.downloadManager = new DownloadManager();
                Network.listener.downloadManager.PrepareDownload(tempSaveFilePath, fileTransferData.fileParts);
            }

            Network.listener.downloadManager.WriteFilePart(fileTransferData.fileBytes);

            if (fileTransferData.isLastPart)
            {
                Network.listener.downloadManager.FinishFileWrite();
                Network.listener.downloadManager = null;

                byte[] compressedSave = File.ReadAllBytes(tempSaveFilePath);
                byte[] save = GZip.Decompress(compressedSave);
                File.WriteAllBytes(saveFilePath, save);
                File.Delete(tempSaveFilePath);

                GameDataSaveLoader.LoadGame(customSaveName);
            }

            else
            {
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.RequestSavePartPacket));
                Network.listener.EnqueuePacket(rPacket);
            }
        }

        public static void SendSavePartToServer(string fileName = null)
        {
            if (Network.listener.uploadManager == null)
            {
                ClientValues.ToggleSendingSaveToServer(true);

                Log.Message($"[Rimworld Together] > Sending save to server");
                if (ClientValues.saveMessageBool) Messages.Message("Save Syncing With Server...", MessageTypeDefOf.SilentInput);

                saveFilePath = Path.Combine(new string[] { Master.savesFolderPath, fileName + ".rws" });
                tempSaveFilePath = $"{saveFilePath}.temp";

                byte[] saveBytes = File.ReadAllBytes(saveFilePath); ;
                byte[] compressedSave = GZip.Compress(saveBytes);
                File.WriteAllBytes(tempSaveFilePath, compressedSave);

                Network.listener.uploadManager = new UploadManager();
                Network.listener.uploadManager.PrepareUpload(tempSaveFilePath);
            }

            FileTransferData fileTransferData = new FileTransferData();
            fileTransferData.fileSize = Network.listener.uploadManager.fileSize;
            fileTransferData.fileParts = Network.listener.uploadManager.fileParts;
            fileTransferData.fileBytes = Network.listener.uploadManager.ReadFilePart();
            fileTransferData.isLastPart = Network.listener.uploadManager.isLastPart;

            if (DisconnectionManager.isIntentionalDisconnect 
                && (DisconnectionManager.intentionalDisconnectReason == DisconnectionManager.DCReason.SaveQuitToMenu 
                || DisconnectionManager.intentionalDisconnectReason == DisconnectionManager.DCReason.SaveQuitToOS))
            {
                fileTransferData.additionalInstructions = ((int)CommonEnumerators.SaveMode.Disconnect).ToString();
            }
            else fileTransferData.additionalInstructions = ((int)CommonEnumerators.SaveMode.Autosave).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferData);
            Network.listener.EnqueuePacket(packet);

            if (Network.listener.uploadManager.isLastPart) 
            {
                ClientValues.ToggleSendingSaveToServer(false);
                Network.listener.uploadManager = null;

                Log.Message($"[Rimworld Together] > Save sent to server");
                if (ClientValues.saveMessageBool) Messages.Message("Save Synced With Server!", MessageTypeDefOf.SilentInput);

                File.Delete(tempSaveFilePath);
            }
        }
    }
}
