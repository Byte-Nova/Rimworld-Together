using System;
using Newtonsoft.Json;
using Shared.Misc;

namespace Shared
{
    [Serializable]
    public class RoadDetails
    {
        [JsonIgnore] public string CachedRoadDefName = null;
        
        public string RoadDefName
        {
            get
            {
                return CachedRoadDefName;
            }
            set
            {
                CachedRoadDefName = Pools.StringPool.GetOrAddString(value);
            }
        }

        public int FromTile { get; set; }

        public int ToTile { get; set; }
    }
}