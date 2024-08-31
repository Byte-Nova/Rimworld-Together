using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class HumanData
    {
        //Bio

        public string defName;

        public string name;

        public string biologicalAge;

        public string chronologicalAge;

        public string gender;

        public string factionDef;

        public string kindDef;

        public string hairDefName;

        public string hairColor;

        public string headTypeDefName;

        public string skinColor;

        public string beardDefName;

        public string bodyTypeDefName;

        public string FaceTattooDefName;

        public string BodyTattooDefName;

        //Hediffs

        public List<string> hediffDefNames = new List<string>();

        public List<string> hediffPartDefName = new List<string>();

        public List<string> hediffSeverity = new List<string>();

        public List<float> hediffImmunity = new List<float>();

        public List<float> hediffTendQuality = new List<float>();

        public List<float> hediffTotalTendQuality = new List<float>();

        public List<int> hediffTendDuration = new List<int>();

        public List<bool> heddifPermanent = new List<bool>();

        //Xenotypes

        public string xenotypeDefName;

        public string customXenotypeName;

        //Genes

        public List<string> xenogeneDefNames = new List<string>();

        public List<string> endogeneDefNames = new List<string>();

        //Stories

        public string childhoodStory;

        public string adulthoodStory;

        //Skills

        public List<string> skillDefNames = new List<string>();

        public List<string> skillLevels = new List<string>();

        public List<string> passions = new List<string>();

        //Traits

        public List<string> traitDefNames = new List<string>();

        public List<string> traitDegrees = new List<string>();

        //Apparel

        public List<ThingData> equippedApparel = new List<ThingData>();

        public List<bool> apparelWornByCorpse = new List<bool>();

        //Equipment

        public ThingData equippedWeapon;

        public List<ThingData> inventoryItems = new List<ThingData>();

        //Transform

        public string[] position;

        public int rotation;

        //Misc

        public string favoriteColor;
        
        public float growthPoints;
    }
}