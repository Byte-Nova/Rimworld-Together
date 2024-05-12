using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public string siteStep;

        public int tile;

        public string type;

        public string owner;

        public byte[] workerData;

        public string goodwill;

        public bool isFromFaction;

        //list of tiles of sites that have rewards
        public List<int> sitesWithRewards = new List<int>();
    }
}
