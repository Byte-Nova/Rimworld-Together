using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON.Things
{
    [Serializable]
    public class HumanDetailsJSON
    {
        public string defName;
        public string name;
        public string biologicalAge;
        public string chronologicalAge;
        public string gender;

        public string hairDefName;
        public string hairColor;
        public string headTypeDefName;
        public string skinColor;
        public string beardDefName;
        public string bodyTypeDefName;
        public string FaceTattooDefName;
        public string BodyTattooDefName;

        public List<string> hediffDefNames = new List<string>();
        public List<string> hediffPartDefName = new List<string>();
        public List<string> hediffSeverity = new List<string>();
        public List<bool> heddifPermanent = new List<bool>();

        public string xenotypeDefName;
        public string customXenotypeName;

        public List<string> geneDefNames = new List<string>();
        public List<string> endogeneDefNames = new List<string>();
        public List<string> geneAbilityDefNames = new List<string>();

        public string favoriteColor;
        public string childhoodStory;
        public string adulthoodStory;

        public List<string> skillDefNames = new List<string>();
        public List<string> skillLevels = new List<string>();
        public List<string> passions = new List<string>();

        public List<string> traitDefNames = new List<string>();
        public List<string> traitDegrees = new List<string>();

        public List<string> deflatedApparels = new List<string>();
        public List<bool> apparelWornByCorpse = new List<bool>();

        public string deflatedWeapon;

        public List<string> deflatedInventoryItems = new List<string>();

        public string position;
    }
}