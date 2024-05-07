using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame", typeof(string))]
    public static class SaveOnlineGame
    {
        [HarmonyPrefix]
        public static bool DoPre(ref string fileName, ref int ___lastSaveTick)
        {
            try
            {
                if (!Network.isConnectedToServer) return true;
                if (ClientValues.isSavingGame || ClientValues.isSendingSaveToServer) return false;

                ClientValues.ToggleSavingGame(true);
                ClientValues.ForcePermadeath();
                ClientValues.ManageDevOptions();
                CustomDifficultyManager.EnforceCustomDifficulty();

                Log.Message("Creating local save");
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
                catch (Exception e) { Log.Error("Exception while saving game: " + e); }

                Log.Message("Sending maps to server");
                MapManager.SendPlayerMapsToServer();

                Log.Message("Sending first save chunk to server");
                SaveManager.SendSavePartToServer(fileName);
            }
            catch (Exception e) { Log.Error($"{e}"); }

            ClientValues.ToggleSavingGame(false);

            return false;
        }
    }

    [HarmonyPatch(typeof(Autosaver), "DoAutosave")]
    public static class Autosave
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (!Network.isConnectedToServer) return true;
            else return false;
        }
    }

    [HarmonyPatch(typeof(Autosaver), "AutosaverTick")]
    public static class AutosaveTick
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
                    SaveManager.ForceSave();
                }

                return false;
            }
        }
    }
}
