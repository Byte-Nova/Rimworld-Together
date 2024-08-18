using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public SiteStepMode siteStepMode;

        public int tile;

        public int type;

        public string owner;

        public byte[] workerData;

        public Goodwill goodwill;

        public bool isFromFaction;

        public List<int> sitesWithRewards = new List<int>();
    }
}
