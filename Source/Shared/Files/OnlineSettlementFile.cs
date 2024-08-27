using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OnlineSettlementFile
    {
        public int tile;

        public string owner;
        
        public Goodwill goodwill;
    }
}