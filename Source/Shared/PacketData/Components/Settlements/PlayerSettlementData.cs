using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerSettlementData
    {
        public SettlementStepMode stepMode;

        public SettlementFile settlementData = new SettlementFile();
    }
    public class PlayerShipData : PlayerSettlementData 
    {
        public float theta;
        public float radius;
        public float phi;
    }
}
