using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class NPCSettlementData
    {
        public SettlementStepMode _stepMode;

        public PlanetNPCSettlement _settlementData = new PlanetNPCSettlement();
    }
}