using static Shared.CommonEnumerators;

namespace Shared
{

    public class PlayerSettlementData
    {
        public SettlementStepMode _stepMode { get; set; } = SettlementStepMode.Add;

        public SettlementFile _settlementFile { get; set; } = new SettlementFile();
    }
}
