using Verse;

namespace GameClient
{
    public class ModConfigs : ModSettings
    {
        public bool verboseBool;

        public bool extremeVerboseBool;

        public bool muteChatSoundBool;

        public bool rejectTransfersBool;

        public bool rejectSiteRewardsBool;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref verboseBool, "verboseBool");
            Scribe_Values.Look(ref extremeVerboseBool, "extremeVerboseBool");
            Scribe_Values.Look(ref muteChatSoundBool, "muteChatSoundBool");
            Scribe_Values.Look(ref rejectTransfersBool, "rejectTransfersBool");
            Scribe_Values.Look(ref rejectSiteRewardsBool, "rejectSiteRewardsBool");
            
            base.ExposeData();

            ClientValues.verboseBool = verboseBool;
            ClientValues.extremeVerboseBool = extremeVerboseBool;
            ClientValues.muteSoundBool = muteChatSoundBool;
            ClientValues.rejectTransferBool = rejectTransfersBool;
            ClientValues.rejectSiteRewardsBool = rejectSiteRewardsBool;
        }
    }
}
