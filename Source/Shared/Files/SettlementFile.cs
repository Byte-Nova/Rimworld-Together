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

        public override string ToString()
        {
            return $"SettlementFile:|{Tile}|{UID}|{Label}|{Goodwill}";
        }
    }
}