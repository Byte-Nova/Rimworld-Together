using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerSettlementData
    {
        public SettlementStepMode _stepMode;

        public SettlementFile _settlementFile = new SettlementFile();
    }
}
