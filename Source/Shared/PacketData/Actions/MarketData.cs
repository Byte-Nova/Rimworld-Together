using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class MarketData
    {
        public MarketStepMode marketStepMode;

        public int indexToManage;
        public int quantityToManage;

        public List<byte[]> currentStockBytes;

        public List<byte[]> transferThingBytes;
    }
}