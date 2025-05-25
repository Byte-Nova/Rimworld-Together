using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{

    public class FactionGoodwillData
    {
        public int _tile { get; set; } = -1;

        public string _uid { get; set; } = string.Empty;

        public Goodwill _goodwill { get; set; } = Goodwill.Enemy;

        //Settlements

        public List<int> _settlementTiles { get; set; } = new List<int>();

        public Goodwill[] _settlementGoodwills { get; set; } = null;

        //Sites

        public List<int> _siteTiles { get; set; } = new List<int>();

        public Goodwill[] _siteGoodwills { get; set; } = null;

        public override string ToString()
        {
            return $"FactionGoodwillData:|{_tile}|{_uid}";
        }
    }
}
