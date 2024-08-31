using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class FactionGoodwillData
    {
        public int _tile;

        public string _owner;

        public Goodwill _goodwill;

        //Settlements

        public List<int> _settlementTiles = new List<int>();

        public Goodwill[] _settlementGoodwills;

        //Sites

        public List<int> _siteTiles = new List<int>();
        
        public Goodwill[] _siteGoodwills;
    }
}
