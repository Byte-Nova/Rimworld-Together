using Shared.Files;

namespace TCPNetwork.Packets 
{

    public class RewardData 
    {
        public SiteRewardFile[] _rewardData { get; set; } = null;

        public override string ToString()
        {
            return $"RewardData:|{_rewardData?.Length ?? 0}";
        }
    }
}