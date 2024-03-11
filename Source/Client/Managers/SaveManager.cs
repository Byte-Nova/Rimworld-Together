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

        public static void ForceSave()
        {
            if (ClientValues.isSaving) return;
            else
            {
                ClientValues.ToggleSaving(true);
                FieldInfo FticksSinceSave = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");
                FticksSinceSave.SetValue(Current.Game.autosaver, 0);
                Current.Game.autosaver.DoAutosave();
            }
        }

        public static void ReceiveSavePartFromServer(Packet packet)
        {
            FileTransferJSON fileTransferJSON = (FileTransferJSON)Serializer.ConvertBytesToObject(packet.contents);

            if (Network.listener.downloadManager == null)
            {
                Log.Message($"[Rimworld Together] > Receiving save from server");

                customSaveName = $"Server - {Network.ip} - {ChatManager.username}";
                string filePath = Path.Combine(new string[] { Master.savesFolderPath, customSaveName + ".rws" });

                Network.listener.downloadManager = new DownloadManager();
                Network.listener.downloadManager.PrepareDownload(filePath, fileTransferJSON.fileParts);
            }

            Network.listener.downloadManager.WriteFilePart(fileTransferJSON.fileBytes);

            if (fileTransferJSON.isLastPart)
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

            FileTransferJSON fileTransferJSON = new FileTransferJSON();
            fileTransferJSON.fileSize = Network.listener.uploadManager.fileSize;
            fileTransferJSON.fileParts = Network.listener.uploadManager.fileParts;
            fileTransferJSON.fileBytes = Network.listener.uploadManager.ReadFilePart();
            fileTransferJSON.isLastPart = Network.listener.uploadManager.isLastPart;

            if (ClientValues.isDisconnecting || ClientValues.isQuiting) fileTransferJSON.additionalInstructions = ((int)CommonEnumerators.SaveMode.Disconnect).ToString();
            else fileTransferJSON.additionalInstructions = ((int)CommonEnumerators.SaveMode.Autosave).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferJSON);
            Network.listener.EnqueuePacket(packet);

            if (Network.listener.uploadManager.isLastPart) Network.listener.uploadManager = null;
        }
    }
}
