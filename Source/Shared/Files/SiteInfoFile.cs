namespace Shared.Files
{
    public class SiteInfoFile 
    {
        public string DefName { get; set; } = string.Empty;

        public string[] DefNameCost { get; set; } = null;

        public int[] Cost { get; set; } = null;

        public SiteRewardFile[] Rewards { get; set; } = null;

        public SiteInfoFile Clone() 
        {
            byte[] data = Serializer.ConvertObjectToBytes(this);
            return Serializer.ConvertBytesToObject<SiteInfoFile>(data);
        }
    }
}