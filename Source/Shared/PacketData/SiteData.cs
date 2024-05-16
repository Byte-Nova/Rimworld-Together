using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public SiteStepMode siteStep;

        public string tile;

        public string type;

        public string owner;

        public byte[] workerData;

        public Goodwills goodwill;

        public bool isFromFaction;

        public List<string> sitesWithRewards = new List<string>();
    }
}
