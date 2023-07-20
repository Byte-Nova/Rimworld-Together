using HarmonyLib;
using RimWorld;
using System;
using System.IO;
using System.Reflection;
using Verse;

namespace RimworldTogether
{
    public class SavePatch
    {
        public static string customSaveName = "ServerSave";

        public enum SaveMode { Disconnect, Quit, Autosave, Transfer, Event }

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

        public static void SendSaveToServer(string fileName)
        {
            string filePath = Path.Combine(new string[] { Main.savesPath, fileName + ".rws" });

            SaveFileJSON saveFileJSON = new SaveFileJSON();
            saveFileJSON.saveData = GZip.Compress(File.ReadAllBytes(filePath));

            if (ClientValues.isDisconnecting) saveFileJSON.saveMode = ((int)SaveMode.Disconnect).ToString();
            else if (ClientValues.isQuiting) saveFileJSON.saveMode = ((int)SaveMode.Quit).ToString();
            else if (ClientValues.isInTransfer) saveFileJSON.saveMode = ((int)SaveMode.Transfer).ToString();
            else saveFileJSON.saveMode = ((int)SaveMode.Autosave).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(saveFileJSON) };
            Packet packet = new Packet("SaveFilePacket", contents);
            Network.SendData(packet);
        }

        public static void SendMapsToServer()
        {
            foreach(Map map in Find.Maps.ToArray())
            {
                if (map.IsPlayerHome)
                {
                    MapDetailsJSON toSend = new MapDetailsJSON();
                    toSend.mapTile = map.Tile.ToString();
                    toSend.deflatedMapData = RimworldManager.CompressMapToString(map, true, true, true, true);

                    string[] contents = new string[] { Serializer.SerializeToString(toSend) };
                    Packet packet = new Packet("MapPacket", contents);
                    Network.SendData(packet);
                }
            }
        }

        public static void ReceiveSaveFromServer(Packet packet)
        {
            string filePath = Path.Combine(new string[] { Main.savesPath, customSaveName + ".rws" });
            File.WriteAllBytes(filePath, GZip.Decompress(packet.contents[0]));
            GameDataSaveLoader.LoadGame(customSaveName);
        }
    }

    [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame", typeof(string))]
    public static class SaveOnlineGame
    {
        [HarmonyPrefix]
        public static bool DoPre(ref string fileName, ref int ___lastSaveTick)
        {
            if (Network.isConnectedToServer)
            {
                PersistentPatches.ForcePermadeath();
                PersistentPatches.ManageDevOptions();
                PersistentPatches.ManageGameDifficulty();

                fileName = SavePatch.customSaveName;

                try
                {
                    SafeSaver.Save(GenFilePaths.FilePathForSavedGame(fileName), "savegame", delegate
                    {
                        ScribeMetaHeaderUtility.WriteMetaHeader();
                        Game target = Current.Game;
                        Scribe_Deep.Look(ref target, "game");
                    }, Find.GameInfo.permadeathMode);
                    ___lastSaveTick = Find.TickManager.TicksGame;
                }
                catch (Exception ex) { Log.Error("Exception while saving game: " + ex); }

                return false;
            }

            else return true;
        }

        [HarmonyPostfix]
        public static void DoPost(ref string fileName)
        {
            if (Network.isConnectedToServer)
            {
                if (ClientValues.isDisconnecting || ClientValues.isQuiting) SavePatch.SendMapsToServer();
                SavePatch.SendSaveToServer(fileName);
            }
        }
    }

    [HarmonyPatch(typeof(Autosaver), "AutosaverTick")]
    public static class Autosave
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (!Network.isConnectedToServer) return true;
            else
            {
                ClientValues.autosaveCurrentTicks++;
                if (ClientValues.autosaveCurrentTicks >= ClientValues.autosaveInternalTicks && !GameDataSaveLoader.SavingIsTemporarilyDisabled)
                {
                    SavePatch.ForceSave();

                    ClientValues.autosaveCurrentTicks = 0;
                }

                return false;
            }
        }
    }
}
