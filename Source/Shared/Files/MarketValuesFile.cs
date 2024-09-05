using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class MarketValuesFile
    {
        public List<ThingDataFile> MarketStock = new List<ThingDataFile>();
    }
}
