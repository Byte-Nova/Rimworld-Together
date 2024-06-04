using System;
using Shared;

namespace Shared
{
    [Serializable]
    public class MarketData
    {
        public CommonEnumerators.MarketStepMode marketStepMode;

        public ItemData[] currentStock;

        public int indexToManage;
        public ItemData stockToManage;
    }
}