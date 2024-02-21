using Verse;

namespace GameClient
{
    public class ModConfigs : ModSettings
    {
        public bool transferBool;
        public bool siteRewardsBool;
        public bool verboseBool;
        public bool extremeVerboseBool;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref transferBool, "transferBool");
            Scribe_Values.Look(ref siteRewardsBool, "siteRewardsBool");
            Scribe_Values.Look(ref verboseBool, "verboseBool");
            Scribe_Values.Look(ref extremeVerboseBool, "extremeVerboseBool");
            base.ExposeData();

            ClientValues.autoDenyTransfers = transferBool;
            ClientValues.autoRejectSiteRewards = siteRewardsBool;
            ClientValues.verboseBool = verboseBool;
            ClientValues.extremeVerboseBool = extremeVerboseBool;
        }
    }
}
