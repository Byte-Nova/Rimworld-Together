using System.Collections.Generic;

namespace Shared
{
    public class BookData 
    {
        public string title = "null";

        public string description = "null";

        public string descriptionFlavor = "null";

        public Dictionary<string, float> skillData = new Dictionary<string,float>();

        public Dictionary<string, float> researchData = new Dictionary<string, float>(); // Schematics and Tomes only

        public float joyFactor = 1f; //Text Books only
        
        public float mentalBreakChance = -1f; // Tomes only
    }
}