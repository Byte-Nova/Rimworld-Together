using GameClient.Dialogs;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Shared.Files;
using System;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
using static UnityEngine.GraphicsBuffer;

namespace GameClient.Managers
{
    public static class DifficultyManager
    {
        public static void OpenDifficultyMenu()
        {
            string description = "Do you want to enforce the current difficulty?";
            Action actionYes = delegate
            {
                GameParameterManager.SendCurrentStoryteller(true);
                GameParameterManager.SendCurrentDifficulty(true);
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_YesNo(description, actionYes));
        }
    }
}
