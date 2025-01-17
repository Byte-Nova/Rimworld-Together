using HarmonyLib;
using RimWorld;
using Shared;
using System.IO;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
using static GameClient.Managers.DisconnectionManager;
using System.Xml;
using System.Xml.XPath;
using System;
using GameClient.Core;
using GameClient.Misc;
using GameClient.Values;
using GameClient.TCP;

namespace GameClient.Managers
{
    [RTManager]
    public static class SaveManager
    {
        public static string customSaveName => $"Server - {Network.ip} - {Network.port} - {ClientValues.username}";

        private static string saveFilePath => Path.Combine(Master.savesFolderPath, customSaveName + ".rws");

        private static string tempSaveFilePath => saveFilePath + ".mpsave";

        private static string serverSaveFilePath => saveFilePath + ".rws.temp";

        public static void ParsePacket(Packet packet)
        {
            SaveData data = Serializer.ConvertBytesToObject<SaveData>(packet.contents);
            if (data._stepMode == SaveStepMode.Receive) ReceiveSavePartFromServer(data);
            else if (data._stepMode == SaveStepMode.Send) SendSavePartToServer();
            else throw new NotImplementedException();
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
                Printer.Message($"Receiving save from server");

                Network.listener.downloadManager = new DownloadManager(tempSaveFilePath);
                Network.listener.downloadManager.PrepareDownload();
            }

            Network.listener.downloadManager.WriteFilePart(data._fileBytes);

            if (data._isLastPart) OnLastPartReceived(data);
            else OnPartReceived();
        }

        private static void OnLastPartReceived(SaveData data)
        {
            Network.listener.downloadManager.FinishFileWrite();
            Network.listener.downloadManager = null;

            byte[] fileBytes = File.ReadAllBytes(tempSaveFilePath);
            fileBytes = GZip.DecompressBytes(fileBytes);

            File.WriteAllBytes(serverSaveFilePath, fileBytes);
            File.Delete(tempSaveFilePath);

            if (data._instructions != (int)SaveMode.Strict && File.Exists(saveFilePath))
            {
                if (GetRealPlayTimeInteractingFromSave(serverSaveFilePath) >= GetRealPlayTimeInteractingFromSave(saveFilePath))
                {
                    Printer.Message("Loading remote save");
                    File.Delete(saveFilePath);
                    File.Move(serverSaveFilePath, saveFilePath);
                }

                else
                {
                    Printer.Message("Loading local save");
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

        private static void OnPartReceived()
        {
            SaveData rData = new SaveData();
            rData._stepMode = SaveStepMode.Send;

            Packet rPacket = Packet.CreatePacketFromObject(nameof(SaveManager), rData);
            Network.listener.EnqueuePacket(rPacket);
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
            if (Network.listener.uploadManager == null)
            {
                ClientValues.ToggleSendingSaveToServer(true);

                byte[] saveBytes = File.ReadAllBytes(saveFilePath);
                saveBytes = GZip.CompressBytes(saveBytes);

                File.WriteAllBytes(tempSaveFilePath, saveBytes);
                Network.listener.uploadManager = new UploadManager(tempSaveFilePath);
                Network.listener.uploadManager.PrepareUpload();
            }

            SaveData data = new SaveData();
            data._fileBytes = Network.listener.uploadManager.ReadFilePart();
            data._isLastPart = Network.listener.uploadManager.isLastPart;
            data._stepMode = SaveStepMode.Receive;

            // Set the instructions of the packet
            if (isIntentionalDisconnect && (intentionalDisconnectReason == DCReason.SaveQuitToMenu || intentionalDisconnectReason == DCReason.SaveQuitToOS))
            {
                data._instructions = (int)SaveMode.Disconnect;
            }
            else data._instructions = (int)SaveMode.Autosave;

            Packet packet = Packet.CreatePacketFromObject(nameof(SaveManager), data);
            Network.listener.EnqueuePacket(packet);

            if (Network.listener.uploadManager.isLastPart) OnLastPartReceived();
        }

        private static void OnLastPartReceived()
        {
            ClientValues.ToggleSendingSaveToServer(false);
            Network.listener.uploadManager = null;
            File.Delete(tempSaveFilePath);
        }
    }
}
