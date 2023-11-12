using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;
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
            if (ServerValues.isAdmin) return;
            else
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
        }

        public static void SendSaveToServer(string fileName)
        {
            string filePath = Path.Combine(new string[] { Main.savesPath, fileName + ".rws" });

            SaveFileJSON saveFileJSON = new SaveFileJSON();
            saveFileJSON.saveData = File.ReadAllBytes(filePath);

            if (ClientValues.isDisconnecting) saveFileJSON.saveMode = ((int)CommonEnumerators.SaveStepMode.Disconnect).ToString();
            else if (ClientValues.isQuiting) saveFileJSON.saveMode = ((int)CommonEnumerators.SaveStepMode.Quit).ToString();
            else if (ClientValues.isInTransfer) saveFileJSON.saveMode = ((int)CommonEnumerators.SaveStepMode.Transfer).ToString();
            else saveFileJSON.saveMode = ((int)CommonEnumerators.SaveStepMode.Autosave).ToString();

            Packet packet = Packet.CreatePacketFromJSON("SaveFilePacket", saveFileJSON);
            Network.Network.serverListener.SendData(packet);
        }

        public static void ReceiveSaveFromServer(Packet packet)
        {
            customSaveName = $"Server - {Network.Network.ip} - {ChatManager.username}";

            SaveFileJSON saveFileJSON = (SaveFileJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            string filePath = Path.Combine(new string[] { Main.savesPath, customSaveName + ".rws" });
            File.WriteAllBytes(filePath, saveFileJSON.saveData);

            GameDataSaveLoader.LoadGame(customSaveName);
        }
    }
}
