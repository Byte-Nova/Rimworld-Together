using System;
using System.Collections.Generic;
using Shared;

namespace Shared
{
    [Serializable]
    public class MarketData
    {
        public CommonEnumerators.MarketStepMode marketStepMode;

        public int indexToManage;
        public int quantityToManage;

        public List<byte[]> currentStockBytes;

        public List<byte[]> transferThingBytes;
    }
}