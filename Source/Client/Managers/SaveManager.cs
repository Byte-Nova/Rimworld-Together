using HarmonyLib;
using RimWorld;
using Shared;
using System.IO;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class SaveManager
    {
        public static string customSaveName = "ServerSave";

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
                Logger.WriteToConsole("Receiving save from server", LogMode.Message);

                customSaveName = $"Server - {Network.ip} - {ClientValues.username}";
                string filePath = Path.Combine(new string[] { Master.savesFolderPath, customSaveName + ".rws" });

                Network.listener.downloadManager = new DownloadManager();
                Network.listener.downloadManager.PrepareDownload(filePath, fileTransferData.fileParts);
            }

            Network.listener.downloadManager.WriteFilePart(fileTransferData.fileBytes);

            if (fileTransferData.isLastPart)
            {
                //remove the waiting for login response dialog
                DialogManager.clearStack();
                Network.listener.downloadManager.FinishFileWrite();
                Network.listener.downloadManager = null;

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
                Logger.WriteToConsole("Sending save to server", LogMode.Message);

                string filePath = Path.Combine(new string[] { Master.savesFolderPath, fileName + ".rws" });

                Network.listener.uploadManager = new UploadManager();
                Network.listener.uploadManager.PrepareUpload(filePath);
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
            }
        }
    }
}
