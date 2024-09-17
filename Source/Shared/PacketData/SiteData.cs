using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public SiteStepMode _stepMode;

        public SiteFile _siteFile = new SiteFile();

        public List<int> _sitesWithRewards = new List<int>();
    }
}
