using static Shared.CommonEnumerators;

namespace GameServer.Updater
{
    [Serializable]
    public class WorldData
    {
        public WorldStepMode worldStepMode;

        public WorldValuesFile worldValuesFile;
    }
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

        public RoadDetails[] Roads;

        public RiverDetails[] Rivers;

        public PollutionDetails[] PollutedTiles;

        public PlanetNPCFaction[] NPCFactions;

        public PlanetNPCSettlement[] NPCSettlements;
    }

    [Serializable]
    public class PlanetFeature
    {
        public string defName;
        public string featureName;
        public float[] drawCenter;
        public float maxDrawSizeInTiles;
    }
    [Serializable]
    public class RoadDetails
    {
        public string roadDefName;
        public int tileA;
        public int tileB;
    }
    [Serializable]
    public class RiverDetails
    {
        public string riverDefName;
        public int tileA;
        public int tileB;
    }
    [Serializable]
    public class PollutionDetails
    {
        public int tile;
        public float quantity;
    }
    [Serializable]
    public class PlanetNPCFaction
    {
        public string factionDefName;
        public string factionName;
        public float[] factionColor;
    }
    [Serializable]
    public class PlanetNPCSettlement
    {
        public int tile;
        public string name;
        public string factionDefName;
    }
}
