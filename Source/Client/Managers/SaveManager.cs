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
using System.Collections.Generic;

namespace GameClient.Managers
{
    public static class SaveManager
    {
        // Variables

        public static string CustomSaveName => $"Server - {Network.Ip} - {Network.Port} - {ClientValues.Username}";

        public static string SaveFilePath => Path.Combine(Master.SavesFolderPath, CustomSaveName + ".rws");

        public static string TempSaveFilePath => SaveFilePath + ".mpsave";

        public static string ServerSaveFilePath => SaveFilePath + ".rws.temp";

        [HandlesPacket(PacketHeader.SaveManager)]
        private static void ParsePacket(byte[] bytes)
        {
            SaveData data = Serializer.ConvertBytesToObject<SaveData>(bytes);

            if (data._stepMode == SaveStepMode.Receive) SaveReceiverManager.ReceiveSaveFromServer(data);
            else if (data._stepMode == SaveStepMode.Send) SaveSenderManager.SendSaveToServer();
            else throw new NotImplementedException();
        }

        public static void ForceSave()
        {
            FieldInfo FticksSinceSave = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");
            FticksSinceSave.SetValue(Current.Game.autosaver, 0);

            GameDataSaveLoader.SaveGame(CustomSaveName);
        }

        public static void RequestResetSave()
        {
            SaveData data = new SaveData();
            data._stepMode = SaveStepMode.Reset;

            Network.Listener.EnqueuePacket(PacketHeader.SaveManager, data);
        }

        public static double GetRealPlayTimeInteractingFromSave(string filePath)
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

        public static Dictionary<string, string> GetAllSaveFiles() 
        {
            var result = new Dictionary<string, string>();
            foreach (string file in Directory.GetFiles(Master.SavesFolderPath))
            {
                if(Path.GetExtension(file) == ".rws")
                    result.Add(Path.GetFileNameWithoutExtension(file), file);
            }
            return result;
        }
    }

    public static class SaveSenderManager
    {
        public static void SendSaveToServer()
        {
            byte[] saveBytes = File.ReadAllBytes(SaveManager.SaveFilePath);
            saveBytes = GZip.CompressBytes(saveBytes);

            SaveData data = new SaveData();
            data._fileBytes = saveBytes;
            data._stepMode = SaveStepMode.Receive;

            // Set the instructions of the packet
            if (IsIntentionalDisconnect && (IntentionalDisconnectReason == DCReason.SaveQuitToMenu || IntentionalDisconnectReason == DCReason.SaveQuitToOS))
            {
                data._instructions = (int)SaveMode.Disconnect;
            }
            else data._instructions = SaveMode.Autosave;

            Network.Listener.EnqueuePacket(PacketHeader.SaveManager, data);
        }
    }

    public static class SaveReceiverManager
    {
        public static void ReceiveSaveFromServer(SaveData data)
        {
            Printer.Message($"Receiving save from server", LogImportanceMode.Verbose);

            File.WriteAllBytes(SaveManager.TempSaveFilePath, data._fileBytes);

            OnSaveReceived(data);
        }

        private static void OnSaveReceived(SaveData data)
        {
            byte[] fileBytes = File.ReadAllBytes(SaveManager.TempSaveFilePath);
            fileBytes = GZip.DecompressBytes(fileBytes);

            File.WriteAllBytes(SaveManager.ServerSaveFilePath, fileBytes);
            File.Delete(SaveManager.TempSaveFilePath);

            if (data._instructions != SaveMode.Strict && File.Exists(SaveManager.SaveFilePath))
            {
                if (SaveManager.GetRealPlayTimeInteractingFromSave(SaveManager.ServerSaveFilePath) >= 
                    SaveManager.GetRealPlayTimeInteractingFromSave(SaveManager.SaveFilePath))
                {
                    Printer.Message("Loading remote save", LogImportanceMode.Verbose);
                    File.Delete(SaveManager.SaveFilePath);
                    File.Move(SaveManager.ServerSaveFilePath, SaveManager.SaveFilePath);
                }

                else
                {
                    Printer.Message("Loading local save", LogImportanceMode.Verbose);
                    File.Delete(SaveManager.ServerSaveFilePath);
                }
            }

            else
            {
                File.Delete(SaveManager.SaveFilePath);
                File.Move(SaveManager.ServerSaveFilePath, SaveManager.SaveFilePath);
            }

            GameDataSaveLoader.LoadGame(SaveManager.CustomSaveName);
        }
    }
}
