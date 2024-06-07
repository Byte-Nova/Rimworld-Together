using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public string siteStep;

        public string tile;

        public string type;

        public string owner;

        public byte[] workerData;

        public string goodwill;

        public bool isFromFaction;

        public List<string> sitesWithRewards = new List<string>();
    }
}
