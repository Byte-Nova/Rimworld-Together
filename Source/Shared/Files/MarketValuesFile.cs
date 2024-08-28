using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class MarketValuesFile
    {
        public List<ThingData> MarketStock = new List<ThingData>();
    }
}
