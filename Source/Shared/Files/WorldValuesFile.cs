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

        // key - Faction name
        // value - Faction Details
        public Dictionary<string, byte[]> factions = new Dictionary<string, byte[]>();


        //List of serialized SettlementData
        public List<byte[]> SettlementDatas = new List<byte[]>();

        // key - Deflate Label
        // value - World Deflate
        public Dictionary<string, string> deflateDictionary = new Dictionary<string, string>();


    }
}
