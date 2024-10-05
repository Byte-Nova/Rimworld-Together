namespace Shared 
{
    public class SiteConfigFile 
    {
        public string _DefName;
        public string _RewardDefName;

        public SiteConfigFile(string defName, string rewardDefName)
        {
            _DefName = defName;
            _RewardDefName = rewardDefName;
        }
    }
}