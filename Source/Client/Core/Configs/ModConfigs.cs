using Verse;

namespace GameClient
{
    public class ModConfigs : ModSettings
    {
        public bool muteChatSoundBool;

        public bool rejectTransfersBool;

        public bool rejectSiteRewardsBool;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref muteChatSoundBool, "muteChatSoundBool");
            Scribe_Values.Look(ref rejectTransfersBool, "rejectTransfersBool");
            Scribe_Values.Look(ref rejectSiteRewardsBool, "rejectSiteRewardsBool");
            
            base.ExposeData();

            ClientValues.muteSoundBool = muteChatSoundBool;
            ClientValues.rejectTransferBool = rejectTransfersBool;
            ClientValues.rejectSiteRewardsBool = rejectSiteRewardsBool;
        }
    }
}
