using System;

namespace Shared
{
    [Serializable]
    public class HardmodeValuesFile
    {
        public bool SaveOnColonistDeath = true;

        public bool SaveOnThreats = true;

        public bool SaveOnDowned = false;

        public bool SaveOnBiotechBossSpawn = false;
        
        public bool SaveOnMonolithLevelUp = false;

        public bool SaveOnPsychicRituals = false;
    }
}