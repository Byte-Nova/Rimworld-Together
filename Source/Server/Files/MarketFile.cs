using Shared;

namespace GameServer
{
    [Serializable]
    public class MarketFile
    {
        public List<ItemData> MarketStock = new List<ItemData>();
    }
}
