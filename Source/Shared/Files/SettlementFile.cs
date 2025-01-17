using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SettlementFile
    {
        public int Tile;

        public string UID;

        public string Label;
        
        public Goodwill Goodwill;
    }
}