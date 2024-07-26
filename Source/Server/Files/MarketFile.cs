using Shared;

namespace GameServer
{
    [Serializable]
    public class MarketFile
    {
        public List<ThingData> MarketStock = new List<ThingData>();
    }
}
