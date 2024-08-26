using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerSettlementData
    {
        public SettlementStepMode settlementStepMode;

        public OnlineSettlementFile settlementData = new OnlineSettlementFile();
    }
}
