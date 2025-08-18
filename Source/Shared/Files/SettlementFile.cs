using System;
using static Shared.CommonEnumerators;

namespace Shared.Files
{
    [Serializable]
    public class SettlementFile
    {
        public int Tile { get; set; } = -1;

        public string UID { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
        
        public Goodwill Goodwill { get; set; } = Goodwill.Neutral;
    }
}