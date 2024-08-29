using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SettlementFile
    {
        public int Tile;

        public string Owner;
        
        public Goodwill Goodwill;
    }
}