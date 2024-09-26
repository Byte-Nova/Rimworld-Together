namespace Shared 
{
    public class SiteInfoFile 
    {
        public string DefName;
        public string OverrideName = "";
        public string OverrideDescription = "";

        public string[] DefNameCost;
        public int[] Cost;

        public RewardFile[] Rewards;

        public SiteInfoFile Clone() 
        {
            byte[] data = Serializer.ConvertObjectToBytes(this);
            return Serializer.ConvertBytesToObject<SiteInfoFile>(data);
        }
    }
}