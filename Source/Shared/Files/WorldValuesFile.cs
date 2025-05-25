using System;

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

        public string[] Tiles;

        public PlanetFeatureDetails[] Features;

        public RoadDetails[] Roads;

        public RiverDetails[] Rivers;

        public PollutionDetails[] PollutedTiles;

        public PlanetNPCFactionDetails[] NPCFactions;

        public PlanetNPCSettlementDetails[] NPCSettlements;

        public override string ToString()
        {
            return $"WorldValuesFile:|{PersistentRandomValue}|{SeedString}";
        }
    }
}
