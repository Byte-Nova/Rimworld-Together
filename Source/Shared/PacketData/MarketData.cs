using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class MarketData
    {
        public MarketStepMode _stepMode;

        public int _quantityToManage;

        public int _indexToManage;

        public List<ThingData> _transferThings;
    }
}