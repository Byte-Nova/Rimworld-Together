using System.Collections.Generic;

namespace Shared
{
    public class BookComponent 
    {
        public string Title = "null";

        public string Description = "null";

        public string DescriptionFlavor = "null";

        public Dictionary<string, float> SkillData = new Dictionary<string,float>();

        public Dictionary<string, float> ResearchData = new Dictionary<string, float>(); // Schematics and Tomes only

        public float JoyFactor = 1f; //Text Books only
        
        public float MentalBreakChance = -1f; // Tomes only
    }
}