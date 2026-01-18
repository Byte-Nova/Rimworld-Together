using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using HarmonyLib;
using Shared.Misc;
using System;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame", typeof(string))]
    public static class SaveOnlineGame
    {
        [HarmonyPrefix]
        public static bool DoPre(ref string fileName, ref int ___lastSaveTick)
        {
            try
            {
                if (SessionHandler.IsSavingGame) return false;
                if (SessionHandler.SynchronousMap != null) return false;

                SessionHandler.IsSavingGame = true;

                GameParameterManager.SetScenario(SessionHandler.CurrentScenario);
                GameParameterManager.SetStoryteller(SessionHandler.CurrentStoryteller);
                GameParameterManager.SetDifficulty(SessionHandler.CurrentDifficulty);

                string filePath = GenFilePaths.FilePathForSavedGame(fileName);
                SaveManager.LatestSavePath = filePath;
                try
                {
                    SafeSaver.Save(filePath, "savegame", delegate
                    {
                        ScribeMetaHeaderUtility.WriteMetaHeader();
                        Game target = Current.Game;
                        Scribe_Deep.Look(ref target, "game");
                    }, Find.GameInfo.permadeathMode);
                    ___lastSaveTick = Find.TickManager.TicksGame;
                }
                catch (Exception e) { Printer.Error("Exception while saving game: " + e); }

                Printer.Message("Sending maps to server", LogImportanceMode.Verbose);
                MapManager.SendPlayerMapsToServer();

                Printer.Message("Sending save to server", LogImportanceMode.Verbose);
                SaveManager.SendSaveToServer();

                RT_Dialog_Wait.Instance.Close();
            }
            catch (Exception e) { Printer.Error($"{e}"); }

            SessionHandler.IsSavingGame = false;

            return false;
        }
    }
}
