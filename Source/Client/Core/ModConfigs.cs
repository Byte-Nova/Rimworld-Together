using RimworldTogether.GameClient.Values;
using Verse;

namespace RimworldTogether.GameClient.Core
{
    public class ModConfigs : ModSettings
    {
        public bool transferBool;
        public bool siteRewardsBool;
        public bool verboseBool;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref transferBool, "transferBool");
            Scribe_Values.Look(ref siteRewardsBool, "siteRewardsBool");
            Scribe_Values.Look(ref verboseBool, "verboseBool");
            base.ExposeData();

            ClientValues.autoDenyTransfers = transferBool;
            ClientValues.autoRejectSiteRewards = siteRewardsBool;
            ClientValues.verboseBool = verboseBool;
        }
    }
}
