﻿using Verse;

namespace GameClient
{
    public class ModConfigs : ModSettings
    {
        public bool verboseBool;
        public bool muteChatSoundBool;
        public bool rejectTransfersBool;
        public bool rejectSiteRewardsBool;
        public bool saveMessageBool;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref verboseBool, "verboseBool");
            Scribe_Values.Look(ref muteChatSoundBool, "muteChatSoundBool");
            Scribe_Values.Look(ref rejectTransfersBool, "rejectTransfersBool");
            Scribe_Values.Look(ref rejectSiteRewardsBool, "rejectSiteRewardsBool");
            Scribe_Values.Look(ref saveMessageBool, "saveMessageBool");
            base.ExposeData();

            ClientValues.verboseBool = verboseBool;
            ClientValues.muteSoundBool = muteChatSoundBool;
            ClientValues.rejectTransferBool = rejectTransfersBool;
            ClientValues.rejectSiteRewardsBool = rejectSiteRewardsBool;
            ClientValues.saveMessageBool = saveMessageBool;
        }
    }
}
