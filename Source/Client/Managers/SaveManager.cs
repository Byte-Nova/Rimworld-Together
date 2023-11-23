using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.JSON;
using Shared.Misc;
using Shared.Network;
using System.IO;
using System.Reflection;
using Verse;

namespace RimworldTogether.GameClient.Managers
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
                ClientValues.ToggleSaving(false);
            }
        }

        public static void ReceiveSavePartFromServer(Packet packet)
        {
            FileTransferJSON fileTransferJSON = (FileTransferJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            if (Network.Network.serverListener.downloadManager == null)
            {
                Log.Message($"[Rimworld Together] > Receiving save from server");

                customSaveName = $"Server - {Network.Network.ip} - {ChatManager.username}";
                string filePath = Path.Combine(new string[] { Main.savesPath, customSaveName + ".rws" });

                Network.Network.serverListener.downloadManager = new DownloadManager();
                Network.Network.serverListener.downloadManager.PrepareDownload(filePath, fileTransferJSON.fileParts);
            }

            Network.Network.serverListener.downloadManager.WriteFilePart(fileTransferJSON.fileBytes);

            if (fileTransferJSON.isLastPart)
            {
                Network.Network.serverListener.downloadManager.FinishFileWrite();
                Network.Network.serverListener.downloadManager = null;

                GameDataSaveLoader.LoadGame(customSaveName);
            }

            else
            {
                Packet rPacket = Packet.CreatePacketFromJSON("RequestSavePartPacket");
                Network.Network.serverListener.SendData(rPacket);
            }
        }

        public static void SendSavePartToServer(string fileName = null)
        {
            if (Network.Network.serverListener.uploadManager == null)
            {
                Log.Message($"[Rimworld Together] > Sending save to server");

                string filePath = Path.Combine(new string[] { Main.savesPath, fileName + ".rws" });

                Network.Network.serverListener.uploadManager = new UploadManager();
                Network.Network.serverListener.uploadManager.PrepareUpload(filePath);
            }

            FileTransferJSON fileTransferJSON = new FileTransferJSON();
            fileTransferJSON.fileSize = Network.Network.serverListener.uploadManager.fileSize;
            fileTransferJSON.fileParts = Network.Network.serverListener.uploadManager.fileParts;
            fileTransferJSON.fileBytes = Network.Network.serverListener.uploadManager.ReadFilePart();
            fileTransferJSON.isLastPart = Network.Network.serverListener.uploadManager.isLastPart;

            if (ClientValues.isDisconnecting) fileTransferJSON.additionalInstructions = ((int)CommonEnumerators.SaveStepMode.Disconnect).ToString();
            else if (ClientValues.isQuiting) fileTransferJSON.additionalInstructions = ((int)CommonEnumerators.SaveStepMode.Quit).ToString();
            else if (ClientValues.isInTransfer) fileTransferJSON.additionalInstructions = ((int)CommonEnumerators.SaveStepMode.Transfer).ToString();
            else fileTransferJSON.additionalInstructions = ((int)CommonEnumerators.SaveStepMode.Autosave).ToString();

            Packet packet = Packet.CreatePacketFromJSON("ReceiveSavePartPacket", fileTransferJSON);
            Network.Network.serverListener.SendData(packet);

            if (Network.Network.serverListener.uploadManager.isLastPart)
            {
                Network.Network.serverListener.uploadManager = null;
            }
        }
    }
}
