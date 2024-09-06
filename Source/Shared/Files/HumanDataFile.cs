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

        public string FavoriteColor;
        
        public float GrowthPoints;

        public HediffComponent[] hediffs = new HediffComponent[0];

        public XenotypeComponent xenotype = new XenotypeComponent();

        public XenogeneComponent[] xenogenes  = new XenogeneComponent[0];

        public EndogeneComponent[] endogenes  = new EndogeneComponent[0];

        public StoryComponent stories = new StoryComponent();

        public SkillComponent[] skills  = new SkillComponent[0];

        public TraitComponent[] traits  = new TraitComponent[0];

        public ApparelComponent[] equipedApparel  = new ApparelComponent[0];

        public ThingDataFile EquippedWeapon = new ThingDataFile();

        public ItemComponent[] items = new ItemComponent[0];

        public TransformComponent transform = new TransformComponent();
    }
}