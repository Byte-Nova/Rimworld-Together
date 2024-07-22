using HarmonyLib;
using RimWorld;

namespace GameClient
{
    //TODO
    //FIND OUT IF WE CAN PATCH THE WHOLE QUEST SYSTEM WITHOUT BREAKING IT USING ONLY 1 PATCH INSTEAD OF ALL THIS

    [HarmonyPatch(typeof(QuestManager), nameof(QuestManager.Add))]
    public static class PatchQuestGenerate
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == NetworkState.Disconnected) return true;
            else return false;
        }
    }

    [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.CanAcceptQuest))]
    public static class PatchQuestAccept
    {
        [HarmonyPostfix]
        public static void DoPost(ref bool __result)
        {
            if (Network.state == NetworkState.Disconnected) return;
            else __result = false;
        }
    }

    [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.CanPawnAcceptQuest))]
    public static class PatchPawnQuestAccept
    {
        [HarmonyPostfix]
        public static void DoPost(ref bool __result)
        {
            if (Network.state == NetworkState.Disconnected) return;
            else __result = false;
        }
    }

    [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.SendLetterQuestAvailable))]
    public static class PatchQuestLetter
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == NetworkState.Disconnected) return true;
            else return false;
        }
    }
}
