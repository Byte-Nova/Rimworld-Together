using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class MarketData
    {
        public MarketStepMode stepMode;

        public int quantityToManage;

        public int indexToManage;

        public List<ThingData> transferThings;
    }
}