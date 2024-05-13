using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class WorldData
    {
        public WorldStepMode worldStepMode;

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

        // string of the world Objects class (for settlements and their locations)
        public string WorldObjects = "";
    }
}
