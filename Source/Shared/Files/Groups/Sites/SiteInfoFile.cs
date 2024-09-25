namespace Shared 
{
    public class SiteInfoFile 
    {
        public string DefName;
        public string overrideName = "";
        public string overrideDescription = "";

        public string[] DefNameCost;
        public int[] Cost;

        public RewardFile Rewards = new RewardFile();

        public SiteInfoFile Clone() 
        {
            byte[] data = Serializer.ConvertObjectToBytes(this);
            return Serializer.ConvertBytesToObject<SiteInfoFile>(data);
        }
    }
}