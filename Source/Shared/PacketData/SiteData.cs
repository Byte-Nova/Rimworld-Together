using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public SiteStepMode _stepMode;

        public SiteFile _file = new SiteFile();

        public SiteRewardConfigData _rewardConfig;

        public SiteRewardFile[] _rewardFiles;
    }
}
