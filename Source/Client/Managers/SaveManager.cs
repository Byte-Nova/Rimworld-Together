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

        public static void ParsePacket(Packet packet)
        {
            SaveData data = Serializer.ConvertBytesToObject<SaveData>(packet.contents);
            if (data._stepMode == SaveStepMode.Receive) ReceiveSavePartFromServer(data);
            else if (data._stepMode == SaveStepMode.Send) SendSavePartToServer();
            else throw new Exception();
        }

        public static void ForceSave()
        {
            FieldInfo FticksSinceSave = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");
            FticksSinceSave.SetValue(Current.Game.autosaver, 0);

            ClientValues.autosaveCurrentTicks = 0;

            GameDataSaveLoader.SaveGame(customSaveName);
        }

        public static void RequestResetSave()
        {
            SaveData data = new SaveData();
            data._stepMode = SaveStepMode.Reset;

            Packet packet = Packet.CreatePacketFromObject(nameof(SaveManager), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void ReceiveSavePartFromServer(SaveData data)
        {
            //If this is the first packet
            if (Network.listener.downloadManager == null)
            {
                Logger.Message($"Receiving save from server");

                Network.listener.downloadManager = new DownloadManager();
                Network.listener.downloadManager.PrepareDownload(tempSaveFilePath, data._fileParts);
            }

            Network.listener.downloadManager.WriteFilePart(data._fileBytes);

            //If this is the last packet
            if (data._isLastPart)
            {
                Network.listener.downloadManager.FinishFileWrite();
                Network.listener.downloadManager = null;

                byte[] fileBytes = File.ReadAllBytes(tempSaveFilePath);
                fileBytes = GZip.Decompress(fileBytes);

                File.WriteAllBytes(serverSaveFilePath, fileBytes);
                File.Delete(tempSaveFilePath);

                if(data._instructions != (int)SaveMode.Strict && File.Exists(saveFilePath)) 
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
                SaveData rData = new SaveData();
                rData._stepMode = SaveStepMode.Send;

                Packet rPacket = Packet.CreatePacketFromObject(nameof(SaveManager), rData);
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
            SaveData data = new SaveData();
            data._fileSize = Network.listener.uploadManager.fileSize;
            data._fileParts = Network.listener.uploadManager.fileParts;
            data._fileBytes = Network.listener.uploadManager.ReadFilePart();
            data._isLastPart = Network.listener.uploadManager.isLastPart;
            data._stepMode = SaveStepMode.Receive;

            //Set the instructions of the packet
            if (isIntentionalDisconnect && (intentionalDisconnectReason == DCReason.SaveQuitToMenu || intentionalDisconnectReason == DCReason.SaveQuitToOS))
            {
                data._instructions = (int)SaveMode.Disconnect;
            }
            else data._instructions = (int)SaveMode.Autosave;

            Packet packet = Packet.CreatePacketFromObject(nameof(SaveManager), data);
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
