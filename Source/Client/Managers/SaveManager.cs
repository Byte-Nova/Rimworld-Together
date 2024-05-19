using HarmonyLib;
using RimWorld;
using Shared;
using System.IO;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
using static GameClient.DisconnectionManager;

namespace GameClient
{
    public static class SaveManager
    {
        public static string customSaveName => $"Server - {Network.ip} - {ClientValues.username}";
        private static string saveFilePath => Path.Combine(Master.savesFolderPath, customSaveName + ".rws");
        private static string tempSaveFilePath => saveFilePath + ".temp";

        public static void ForceSave()
        {
            FieldInfo FticksSinceSave = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");
            FticksSinceSave.SetValue(Current.Game.autosaver, 0);

            ClientValues.autosaveCurrentTicks = 0;

            GameDataSaveLoader.SaveGame(customSaveName);
        }

        public static void ReceiveSavePartFromServer(Packet packet)
        {
            FileTransferData fileTransferData = (FileTransferData)Serializer.ConvertBytesToObject(packet.contents);

            //If this is the first packet
            if (Network.listener.downloadManager == null)
            {
                Logger.Message($"Receiving save from server");

                Network.listener.downloadManager = new DownloadManager();
                Network.listener.downloadManager.PrepareDownload(tempSaveFilePath, fileTransferData.fileParts);
            }

            Network.listener.downloadManager.WriteFilePart(fileTransferData.fileBytes);

            //If this is the last packet
            if (fileTransferData.isLastPart)
            {
                Network.listener.downloadManager.FinishFileWrite();
                Network.listener.downloadManager = null;

                byte[] compressedSave = File.ReadAllBytes(tempSaveFilePath);
                byte[] save = GZip.Decompress(compressedSave);
                File.WriteAllBytes(saveFilePath, save);
                File.Delete(tempSaveFilePath);

                GameDataSaveLoader.LoadGame(customSaveName);
                return;
            }

            Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.RequestSavePartPacket));
            Network.listener.EnqueuePacket(rPacket);
        }

        public static void SendSavePartToServer()
        {
            //if this is the first packet
            if (Network.listener.uploadManager == null)
            {
                Log.Message($"[Rimworld Together] > Sending save to server");
                if (ClientValues.saveMessageBool) Messages.Message("Save Syncing With Server...", MessageTypeDefOf.SilentInput);

                ClientValues.ToggleSendingSaveToServer(true);

                byte[] saveBytes = File.ReadAllBytes(saveFilePath); ;
                byte[] compressedSave = GZip.Compress(saveBytes);
                File.WriteAllBytes(tempSaveFilePath, compressedSave);

                Network.listener.uploadManager = new UploadManager();
                Network.listener.uploadManager.PrepareUpload(tempSaveFilePath);
            }

            //Create a new file part packet
            FileTransferData fileTransferData = new FileTransferData();
            fileTransferData.fileSize = Network.listener.uploadManager.fileSize;
            fileTransferData.fileParts = Network.listener.uploadManager.fileParts;
            fileTransferData.fileBytes = Network.listener.uploadManager.ReadFilePart();
            fileTransferData.isLastPart = Network.listener.uploadManager.isLastPart;

            //Set the instructions of the packet
            if (isIntentionalDisconnect && (intentionalDisconnectReason == DCReason.SaveQuitToMenu || intentionalDisconnectReason == DCReason.SaveQuitToOS))
                fileTransferData.instructions = (int)SaveMode.Disconnect;
            else 
                fileTransferData.instructions = (int)SaveMode.Autosave;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferData);
            Network.listener.EnqueuePacket(packet);

            //if this is the last packet
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
