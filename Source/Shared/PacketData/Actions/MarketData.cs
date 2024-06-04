using System;
using Shared;

namespace Shared
{
    [Serializable]
    public class MarketData
    {
        public CommonEnumerators.MarketStepMode marketStepMode;
        public CommonEnumerators.MarketType marketType;

        public ItemData[] currentStock;
        public ItemData stockToManage;
    }
}