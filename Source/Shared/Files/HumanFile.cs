using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class HumanFile
    {
        public string Hash;

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

        public string FavoriteColor;
        
        public float GrowthPoints;

        public HediffComponent[] Hediffs = new HediffComponent[0];

        public XenotypeComponent Xenotype = new XenotypeComponent();

        public XenogeneComponent[] Xenogenes  = new XenogeneComponent[0];

        public EndogeneComponent[] Endogenes  = new EndogeneComponent[0];

        public StoryComponent Stories = new StoryComponent();

        public SkillComponent[] Skills  = new SkillComponent[0];

        public TraitComponent[] Traits  = new TraitComponent[0];

        public ThingDataFile Weapon = new ThingDataFile();

        public ApparelComponent[] Apparel  = new ApparelComponent[0];

        public ItemComponent[] Items = new ItemComponent[0];

        public TransformComponent Transform = new TransformComponent();
    }
}