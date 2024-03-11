using HugsLib;
using Verse;

namespace GameClient
{
    //Class that works as an entry point for the mod

    public class Main : ModBase
    {
        //Unique mod identifier for RimWorld to use

        public override string ModIdentifier => "RimworldTogether";

        [StaticConstructorOnStartup]
        public static class RimworldTogether
        {
            static RimworldTogether() 
            {
                Master.PrepareCulture();
                Master.PreparePaths();
                Master.CreateUnityDispatcher();

                FactionValues.SetPlayerFactionDefs();

                PreferenceManager.LoadClientPreferences();
            }
        }
    }
}