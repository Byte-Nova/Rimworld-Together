using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public SiteStepMode siteStepMode;

        public SiteFile siteFile = new SiteFile();

        public Goodwill goodwill;

        public List<int> sitesWithRewards = new List<int>();
    }
}
