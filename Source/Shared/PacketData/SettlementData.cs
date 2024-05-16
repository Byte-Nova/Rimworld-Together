using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SettlementData
    {
        public SettlementStepMode settlementStepMode;

        public string tile;

        public string owner;

        public string value;

        public Goodwills Goodwill;
    }
}
