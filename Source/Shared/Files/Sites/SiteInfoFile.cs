namespace Shared 
{
    public class SiteInfoFile 
    {
        public string DefName;

        public string[] DefNameCost;

        public int[] Cost;

        public SiteRewardFile[] Rewards;

        public SiteInfoFile Clone() 
        {
            byte[] data = Serializer.ConvertObjectToBytes(this);
            return Serializer.ConvertBytesToObject<SiteInfoFile>(data, false);
        }
    }
}