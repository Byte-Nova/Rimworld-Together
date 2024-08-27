using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class MarketValuesFile
    {
        public bool IsEnabled = true;

        public List<ThingData> MarketStock = new List<ThingData>();
    }
}
