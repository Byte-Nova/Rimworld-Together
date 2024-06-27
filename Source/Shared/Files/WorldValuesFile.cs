using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class WorldValuesFile
    {
        //Misc

        public int PersistentRandomValue;

        //World Values

        public string SeedString;
        public float PlanetCoverage;
        public int Rainfall;
        public int Temperature;
        public int Population;
        public float Pollution;

        //World features

        public PlanetFeature[] Features;

        public PlanetNPCFaction[] NPCFactions;

        public PlanetNPCSettlement[] NPCSettlements;

        public RoadDetails[] Roads;

        public RiverDetails[] Rivers;
    }
}
