using System;

namespace Shared
{
    [Serializable]

    public class ThingDataFile
    {
        public string ID;

        public string DefName;

        public string MaterialDefName;

        public int Quantity;

        public int Quality;

        public int Hitpoints;

        public bool IsMinified;

        public float RotProgress;

        public float[] Color = new float[4];

        //Complex data

        public TransformComponent TransformComponent = new TransformComponent();

        public GenepackComponent GenepackComponent = new GenepackComponent();

        public BookComponent BookComponent = new BookComponent();

        public XenogermComponent XenogermComponent = new XenogermComponent();

        public PlantComponent PlantComponent = new PlantComponent();

        public BladelinkWeaponData BladelinkWeaponData = new BladelinkWeaponData();
        
        public EggData EggData = new EggData();
    }
}