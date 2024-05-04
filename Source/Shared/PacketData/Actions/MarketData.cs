using System;

namespace Shared
{
    [Serializable]
    public class MarketData
    {
        public int marketStepMode;
        public int marketType;

        public ItemData[] currentStock;
        public ItemData stockToManage;
    }
}