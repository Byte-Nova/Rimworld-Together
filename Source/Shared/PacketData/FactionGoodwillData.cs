using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class FactionGoodwillData
    {
        public int tile;

        public string owner;

        public string goodwill;

        public List<int> settlementTiles = new List<int>();
        public List<string> settlementGoodwills = new List<string>();

        public List<int> siteTiles = new List<int>();
        public List<string> siteGoodwills = new List<string>();
    }
}
