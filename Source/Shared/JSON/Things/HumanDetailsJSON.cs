using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class HumanDetailsJSON
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

        public List<ItemDetailsJSON> equippedApparel = new List<ItemDetailsJSON>();
        public List<bool> apparelWornByCorpse = new List<bool>();

        //Equipment

        public ItemDetailsJSON equippedWeapon;
        public List<ItemDetailsJSON> inventoryItems = new List<ItemDetailsJSON>();

        //Misc

        public string favoriteColor;
        public string[] position;
        public int rotation;
    }
}