using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class FactionGoodwillData
    {
        public int tile;

        public string owner;

        public Goodwill goodwill;

        public List<int> settlementTiles = new List<int>();

        public Goodwill[] settlementGoodwills;

        public List<int> siteTiles = new List<int>();
        
        public Goodwill[] siteGoodwills;
    }
}
