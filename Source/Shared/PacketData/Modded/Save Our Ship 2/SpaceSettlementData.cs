using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SpaceSettlementData : SettlementData
    {
        public float radius;
        public float phi;
        public float theta;
    }
}