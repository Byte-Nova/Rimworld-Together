using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class WorldValuesFile
    {
        public string seedString;
        public int persistentRandomValue;
        public string planetCoverage;
        public string rainfall;
        public string temperature;
        public string population;
        public string pollution;
        public List<string> factions = new List<string>();

        public string tileBiomeDeflate;
        public string tileElevationDeflate;
        public string tileHillinessDeflate;
        public string tileTemperatureDeflate;
        public string tileRainfallDeflate;
        public string tileSwampinessDeflate;
        public string tileFeatureDeflate;
        public string tilePollutionDeflate;
        public string tileRoadOriginsDeflate;
        public string tileRoadAdjacencyDeflate;
        public string tileRoadDefDeflate;
        public string tileRiverOriginsDeflate;
        public string tileRiverAdjacencyDeflate;
        public string tileRiverDefDeflate;
    }
}
