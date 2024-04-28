﻿using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GameClient
{
    [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame", typeof(string))]
    public static class SaveOnlineGame
    {
        [HarmonyPrefix]
        public static bool DoPre(ref string fileName, ref int ___lastSaveTick)
        {
            if (!Network.isConnectedToServer) return true;
            if (ClientValues.isSavingGame || ClientValues.isSendingSaveToServer) return false;

            ClientValues.ToggleSavingGame(true);

            ClientValues.ForcePermadeath();
            ClientValues.ManageDevOptions();
            CustomDifficultyManager.EnforceCustomDifficulty();

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

            MapManager.SendPlayerMapsToServer();
            SaveManager.SendSavePartToServer(fileName);

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

        [HarmonyPatch(typeof(TaleReference), "GenerateText")]
        class TaleReferenceGenerateText
        {
            static bool Prefix(TaleReference __instance, ref TaggedString __result)
            {
                if (!(__instance is EditedTaleReference reference)) return true;
                __result = new TaggedString(reference.editedTale);
                return false;
            }
        }

        [HarmonyPatch(typeof(TaleReference), "ExposeData")]
        class TaleReferenceExposeData
        {
            static void Postfix(TaleReference __instance)
            {
                if (__instance is EditedTaleReference reference)
                {
                    Scribe_Values.Look(ref reference.editedTale, "editedTale", "Default Tale", false);
                }
            }
        }
    }
}
