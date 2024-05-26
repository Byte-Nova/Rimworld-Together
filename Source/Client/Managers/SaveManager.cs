using HarmonyLib;
using RimWorld;
using Shared;
using System.IO;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
using static GameClient.DisconnectionManager;
using System.Xml;
using System.Xml.XPath;
using System;

namespace GameClient
{
    public static class SaveManager
    {
        public static string customSaveName => $"Server - {Network.ip} - {ClientValues.username}";
        private static string saveFilePath => Path.Combine(Master.savesFolderPath, customSaveName + ".rws");
        private static string tempSaveFilePath => saveFilePath + ".mpsave";
        private static string serverSaveFilePath => saveFilePath + ".rws.temp";

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
                File.WriteAllBytes(serverSaveFilePath, save);
                File.Delete(tempSaveFilePath);

                Logger.Message("Comparing remote vs local save (if exists)");

                if (float.Parse(GetRealPlayTimeInteractingFromSave(serverSaveFilePath)) >=
                float.Parse(GetRealPlayTimeInteractingFromSave(saveFilePath)))
                {
                    Logger.Message("Loading remote save");
                    File.Delete(saveFilePath);
                    File.Move(serverSaveFilePath, saveFilePath);
                }
                else
                {
                    Logger.Message("Loading local save");
                    File.Delete(serverSaveFilePath);
                }

                GameDataSaveLoader.LoadGame(customSaveName);
                return;
            }

            Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.RequestSavePartPacket));
            Network.listener.EnqueuePacket(rPacket);
        }

        private static string GetRealPlayTimeInteractingFromSave(string filePath)
        {
            if (!File.Exists(filePath)) return "0";

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XPathNavigator nav = doc.CreateNavigator();
            
            return nav.SelectSingleNode("/savegame/game/info/realPlayTimeInteracting").Value;
        }

        public static void SendSavePartToServer()
        {
            //if this is the first packet
            if (Network.listener.uploadManager == null)
            {
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
                File.Delete(tempSaveFilePath);
            }
        }
    }
}
