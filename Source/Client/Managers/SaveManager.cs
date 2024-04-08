using HarmonyLib;
using RimWorld;
using Shared;
using System.IO;
using System.Reflection;
using System.Threading;
using Verse;

namespace GameClient
{
    public static class SaveManager
    {
        public static string customSaveName = "ServerSave";

        private static readonly Semaphore semaphore = new Semaphore(1, 1);

        public static void ForceSave()
        {
            semaphore.WaitOne();

            FieldInfo FticksSinceSave = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");
            FticksSinceSave.SetValue(Current.Game.autosaver, 0);

            ClientValues.autosaveCurrentTicks = 0;

            Current.Game.autosaver.DoAutosave();

            semaphore.Release();
        }

        public static void ReceiveSavePartFromServer(Packet packet)
        {
            FileTransferData fileTransferData = (FileTransferData)Serializer.ConvertBytesToObject(packet.contents);

            if (Network.listener.downloadManager == null)
            {
                Log.Message($"[Rimworld Together] > Receiving save from server");

                customSaveName = $"Server - {Network.ip} - {ChatManager.username}";
                string filePath = Path.Combine(new string[] { Master.savesFolderPath, customSaveName + ".rws" });

                Network.listener.downloadManager = new DownloadManager();
                Network.listener.downloadManager.PrepareDownload(filePath, fileTransferData.fileParts);
            }

            Network.listener.downloadManager.WriteFilePart(fileTransferData.fileBytes);

            if (fileTransferData.isLastPart)
            {
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
                Log.Message($"[Rimworld Together] > Sending save to server");

                string filePath = Path.Combine(new string[] { Master.savesFolderPath, fileName + ".rws" });

                Network.listener.uploadManager = new UploadManager();
                Network.listener.uploadManager.PrepareUpload(filePath);
            }

            FileTransferData fileTransferData = new FileTransferData();
            fileTransferData.fileSize = Network.listener.uploadManager.fileSize;
            fileTransferData.fileParts = Network.listener.uploadManager.fileParts;
            fileTransferData.fileBytes = Network.listener.uploadManager.ReadFilePart();
            fileTransferData.isLastPart = Network.listener.uploadManager.isLastPart;

            if (ClientValues.isDisconnecting || ClientValues.isQuiting) fileTransferData.additionalInstructions = ((int)CommonEnumerators.SaveMode.Disconnect).ToString();
            else fileTransferData.additionalInstructions = ((int)CommonEnumerators.SaveMode.Autosave).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferData);
            Network.listener.EnqueuePacket(packet);

            if (Network.listener.uploadManager.isLastPart) Network.listener.uploadManager = null;
        }
    }
}
