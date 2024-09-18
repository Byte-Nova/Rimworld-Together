using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class HumanDataFile
    {
        //Bio

        public string DefName;

        public string Name;

        public string BiologicalAge;

        public string ChronologicalAge;

        public string Gender;

        public string FactionDef;

        public string KindDef;

        public string HairDefName;

        public string HairColor;

        public string HeadTypeDefName;

        public string SkinColor;

        public string BeardDefName;

        public string BodyTypeDefName;

        public string FaceTattooDefName;

        public string BodyTattooDefName;

        //Hediffs

        public List<string> HediffDefNames = new List<string>();

        public List<string> HediffPartDefName = new List<string>();

        public List<string> HediffSeverity = new List<string>();

        public List<float> HediffImmunity = new List<float>();

        public List<float> HediffTendQuality = new List<float>();

        public List<float> HediffTotalTendQuality = new List<float>();

        public List<int> HediffTendDuration = new List<int>();

        public List<bool> HeddifPermanent = new List<bool>();

        //Xenotypes

        public string XenotypeDefName;

        public string CustomXenotypeName;

        //Genes

        public List<string> XenogeneDefNames = new List<string>();

        public List<string> EndogeneDefNames = new List<string>();

        //Stories

        public string ChildhoodStory;

        public string AdulthoodStory;

        //Skills

        public List<string> SkillDefNames = new List<string>();

        public List<string> SkillLevels = new List<string>();

        public List<string> Passions = new List<string>();

        //Traits

        public List<string> TraitDefNames = new List<string>();

        public List<string> TraitDegrees = new List<string>();

        //Apparel

        public List<ThingDataFile> EquippedApparel = new List<ThingDataFile>();

        public List<bool> ApparelWornByCorpse = new List<bool>();

        //Equipment

        public ThingDataFile EquippedWeapon;

        public List<ThingDataFile> InventoryItems = new List<ThingDataFile>();

        //Transform

        public string[] Position;

        public int Rotation;

        //Misc

        public string FavoriteColor;
        
        public float GrowthPoints;
    }
}