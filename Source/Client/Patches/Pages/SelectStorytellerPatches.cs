using System;
using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(Page_SelectStoryteller), nameof(Page_SelectStoryteller.PreOpen))]
    public static class Patch_Page_SelectStoryteller_PreOpen
    {
        [HarmonyPrefix]
        public static bool DoPre(ref DifficultyDef ___difficulty, ref Difficulty ___difficultyValues)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else
            {
                Find.GameInitData.permadeathChosen = true;
                Find.GameInitData.permadeath = true;

                if (!ClientValues.IsGeneratingFreshWorld)
                {
                    ___difficulty = DifficultyDefOf.Rough;
                    ___difficultyValues = new Difficulty(___difficulty);
                }

                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Page_SelectStoryteller), nameof(Page_SelectStoryteller.DoWindowContents))]
    public static class Patch_Page_SelectStoryteller_DoWindowContents
    {
        public static bool executedMessage;

        [HarmonyPrefix]
        public static bool DoPre(Rect rect, Page_SelectStoryteller __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;

            if (!ClientValues.IsGeneratingFreshWorld)
            {
                if (SessionValues.StorytellerFile.EnforceStoryteller)
                {
                    if (executedMessage) return true;
                    else
                    {
                        Action toDo = delegate
                        {
                            GameParameterManager.SetStoryteller(SessionValues.StorytellerFile);
                            GameParameterManager.SetDifficulty(SessionValues.DifficultyFile, true);
                            RT_Dialog_Base.PushNewDialog(__instance.next);
                            __instance.Close();

                            executedMessage = false;
                        };
                        RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Storyteller will be forced by the server" }, toDo));

                        executedMessage = true;
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Page_SelectStorytellerInGame), nameof(Page_SelectStorytellerInGame.PreClose))]
    public static class Patch_Page_SelectStorytellerInGame_PreClose
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;

            if (ClientValues.IsAdmin)
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Difficulty settings overriden due to being an admin" }));
                return true;
            }

            if (SessionValues.DifficultyFile.EnforceDifficulty || SessionValues.StorytellerFile.EnforceStoryteller)
            {
                Action toDo = delegate
                {
                    GameParameterManager.SetStoryteller(SessionValues.StorytellerFile);
                    GameParameterManager.SetDifficulty(SessionValues.DifficultyFile);
                };

                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Settings might change to reflect server enforcements" }, toDo));

                return false;
            }

            return true;
        }
    }
}
