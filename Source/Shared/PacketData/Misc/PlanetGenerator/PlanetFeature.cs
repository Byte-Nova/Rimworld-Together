using System;

namespace Shared
{
    [Serializable]
    public class PlanetFeature
    {
        public string defName;

        public string featureName;

        public float[] drawCenter;
        
        public float maxDrawSizeInTiles;
    }
}