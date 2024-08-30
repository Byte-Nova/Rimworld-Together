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
            FileTransferData fileTransferData = Serializer.ConvertBytesToObject<FileTransferData>(packet.contents);

            //If this is the first packet
            if (Network.listener.downloadManager == null)
            {
                Logger.Message($"Receiving save from server");

                Network.listener.downloadManager = new DownloadManager();
                Network.listener.downloadManager.PrepareDownload(tempSaveFilePath, fileTransferData._fileParts);
            }

            Network.listener.downloadManager.WriteFilePart(fileTransferData._fileBytes);

            //If this is the last packet
            if (fileTransferData._isLastPart)
            {
                Network.listener.downloadManager.FinishFileWrite();
                Network.listener.downloadManager = null;

                byte[] fileBytes = File.ReadAllBytes(tempSaveFilePath);
                fileBytes = GZip.Decompress(fileBytes);

                File.WriteAllBytes(serverSaveFilePath, fileBytes);
                File.Delete(tempSaveFilePath);

                if(fileTransferData._instructions != (int)SaveMode.Strict && File.Exists(saveFilePath)) 
                { 
                    if (GetRealPlayTimeInteractingFromSave(serverSaveFilePath) >= GetRealPlayTimeInteractingFromSave(saveFilePath))
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
                }

                else
                {
                    File.Delete(saveFilePath);
                    File.Move(serverSaveFilePath, saveFilePath);
                }

                GameDataSaveLoader.LoadGame(customSaveName);
            }

            else
            {
                Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.RequestSavePartPacket));
                Network.listener.EnqueuePacket(rPacket);
            }
        }

        private static double GetRealPlayTimeInteractingFromSave(string filePath)
        {
            if (!File.Exists(filePath)) return 0;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                XPathNavigator nav = doc.CreateNavigator();

                return double.Parse(nav.SelectSingleNode("/savegame/game/info/realPlayTimeInteracting").Value);
            }
            catch { return 0; }
        }

        public static void SendSavePartToServer()
        {
            //if this is the first packet
            if (Network.listener.uploadManager == null)
            {
                ClientValues.ToggleSendingSaveToServer(true);

                byte[] saveBytes = File.ReadAllBytes(saveFilePath);
                saveBytes = GZip.Compress(saveBytes);

                File.WriteAllBytes(tempSaveFilePath, saveBytes);
                Network.listener.uploadManager = new UploadManager();
                Network.listener.uploadManager.PrepareUpload(tempSaveFilePath);
            }

            //Create a new file part packet
            FileTransferData fileTransferData = new FileTransferData();
            fileTransferData._fileSize = Network.listener.uploadManager.fileSize;
            fileTransferData._fileParts = Network.listener.uploadManager.fileParts;
            fileTransferData._fileBytes = Network.listener.uploadManager.ReadFilePart();
            fileTransferData._isLastPart = Network.listener.uploadManager.isLastPart;

            //Set the instructions of the packet
            if (isIntentionalDisconnect && (intentionalDisconnectReason == DCReason.SaveQuitToMenu || intentionalDisconnectReason == DCReason.SaveQuitToOS))
            {
                fileTransferData._instructions = (int)SaveMode.Disconnect;
            }
            else fileTransferData._instructions = (int)SaveMode.Autosave;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ReceiveSavePartPacket), fileTransferData);
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
