using System;

namespace Shared
{
    [Serializable]

    public class ThingDataFile
    {
        public string DefName;

        public string MaterialDefName;

        public int Quantity;

        public int Quality;

        public int Hitpoints;

        public bool IsMinified;

        public float[] Position;

        public int Rotation;

        public float RotProgress;

        public float[] Color = new float[4];

        //Complex data

        public GenepackData GenepackData = new GenepackData();

        public BookData BookData = new BookData();

        public XenoGermData XenoGermData = new XenoGermData();

        public PlantData PlantData = new PlantData();

        public BladelinkWeaponData BladelinkWeaponData = new BladelinkWeaponData();
        
        public EggData EggData = new EggData();
    }
}