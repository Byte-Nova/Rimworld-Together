using Verse;

namespace GameClient
{
    public class ModConfigs : ModSettings
    {
        public bool transferBool;
        public bool siteRewardsBool;
        public bool verboseBool;
        public bool muteChatSoundBool;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref muteChatSoundBool, "chatSoundBool");
            Scribe_Values.Look(ref transferBool, "transferBool");
            Scribe_Values.Look(ref siteRewardsBool, "siteRewardsBool");
            Scribe_Values.Look(ref verboseBool, "verboseBool");
            base.ExposeData();

            ClientValues.muteSoundBool = muteChatSoundBool;
            ClientValues.autoDenyTransfers = transferBool;
            ClientValues.autoRejectSiteRewards = siteRewardsBool;
            ClientValues.verboseBool = verboseBool;
        }
    }
}
