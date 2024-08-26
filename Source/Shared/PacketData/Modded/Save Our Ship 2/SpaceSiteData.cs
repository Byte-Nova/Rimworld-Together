using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SpaceSiteData : SettlementData
    {
        public float radius;
        public float phi;
        public float theta;

        public string name;
    }
}