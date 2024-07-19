using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class NPCSettlementData
    {
        public SettlementStepMode stepMode;
        public PlanetNPCSettlement details;
    }
}