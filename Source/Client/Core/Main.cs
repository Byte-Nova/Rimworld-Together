using HarmonyLib;
using System.Reflection;
using Verse;

namespace GameClient
{
    //Class that works as an entry point for the mod

    public class Main
    {
        private static readonly string modID = "Rimworld Together";

        [StaticConstructorOnStartup]
        public static class RimworldTogether
        {
            static RimworldTogether() 
            {
                ApplyHarmonyPathches();

                Master.PrepareCulture();
                Master.PreparePaths();
                Master.CreateUnityDispatcher();

                FactionValues.SetPlayerFactionDefs();
                CaravanManagerHelper.SetCaravanDefs();
                PreferenceManager.LoadClientPreferences();
            }
        }

        private static void ApplyHarmonyPathches()
        {
            Harmony harmony = new Harmony(modID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}