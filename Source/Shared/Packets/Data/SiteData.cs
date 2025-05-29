using static Shared.CommonEnumerators;

namespace Shared
{

    public class SiteData
    {
        public SiteStepMode _stepMode { get; set; } = SiteStepMode.Accept;

        public SiteFile _file { get; set; } = new SiteFile();

        public SiteRewardConfigData _rewardConfig { get; set; } = null;

        public SiteRewardFile[] _rewardFiles { get; set; } = null;

        public MapFile _siteMap { get; set; } = null;
    }
}
