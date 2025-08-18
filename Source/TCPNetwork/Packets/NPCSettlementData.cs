using Shared;
using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class NPCSettlementData
    {
        public SettlementStepMode _stepMode { get; set; } = SettlementStepMode.Add;

        public PlanetNPCSettlementDetails _settlementData { get; set; } = new PlanetNPCSettlementDetails();

        public override string ToString()
        {
            return $"NPCSettlementData:|{_stepMode}|{_settlementData}";
        }
    }
}