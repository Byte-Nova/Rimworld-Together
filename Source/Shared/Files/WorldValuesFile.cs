using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class WorldValuesFile
    {
        //World Values

        public string SeedString;
        public float PlanetCoverage;
        public int Rainfall;
        public int Temperature;
        public int Population;
        public float Pollution;

        //Misc

        public int PersistentRandomValue;

        //NPC factions

        public string[] NPCFactionDefNames;

        //NPC settlements

        public WorldAISettlement[] NPCSettlements;
    }
}
