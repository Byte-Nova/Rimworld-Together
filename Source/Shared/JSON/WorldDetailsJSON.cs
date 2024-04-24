﻿using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class WorldDetailsJSON
    {
        public string worldStepMode;

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

        //public List<FactionData> factions;

        // key - Deflate Label
        // value - World Deflate
        public Dictionary<string, string> deflateDictionary = new Dictionary<string, string>();

        // string of the world Objects class (for settlements and their locations)
        public string WorldObjects = "";
    }
}