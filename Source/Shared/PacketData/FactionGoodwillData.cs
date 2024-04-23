using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class FactionGoodwillData
    {
        public string tile;

        public string owner;

        public string goodwill;

        public List<string> settlementTiles = new List<string>();
        public List<string> settlementGoodwills = new List<string>();

        public List<string> siteTiles = new List<string>();
        public List<string> siteGoodwills = new List<string>();
    }
}
