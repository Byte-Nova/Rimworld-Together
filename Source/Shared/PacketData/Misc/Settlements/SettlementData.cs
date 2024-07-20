using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SettlementData
    {
        public SettlementStepMode settlementStepMode;

        public int tile;

        public string owner;

        public Goodwill goodwill;
    }
}
