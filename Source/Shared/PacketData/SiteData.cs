using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public SiteStepMode _stepMode;

        public SiteIdendityFile _siteFile = new SiteIdendityFile();

        public SiteRewardConfigData _siteConfigFile;
    }
}
