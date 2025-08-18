using System;
using Newtonsoft.Json;
using Shared.Misc;

namespace Shared
{
    [Serializable]
    public class PlanetFeatureDetails
    {
        public string Name { get; set;  }
        
        [JsonIgnore]
        private string CachedDefName = null;
        public string DefName 
        {           
            get
            {
                return CachedDefName;
            }
            set
            {
                CachedDefName = Pools.StringPool.GetOrAddString(value);
            }
            
        }

        public float[] DrawCenter { get; set; }

        public float MaxDrawSizeInTiles { get; set; }
    }
}