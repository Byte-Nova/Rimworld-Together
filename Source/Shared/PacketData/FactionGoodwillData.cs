using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class FactionGoodwillData
    {
        public string tile;

        public string owner;

        public Goodwills goodwill;

        public List<string> settlementTiles = new List<string>();
        public Goodwills[] settlementGoodwills = new Goodwills[0];

        public List<string> siteTiles = new List<string>();
        public Goodwills[] siteGoodwills = new Goodwills[0];
    }
}
