using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class MarketValuesFile
    {
        public List<ThingFile> MarketStock = new List<ThingFile>();
    }
}
