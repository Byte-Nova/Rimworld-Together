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

        //Complex data

        public GenepackData GenepackData = new GenepackData();

        public BookData BookData = new BookData();

        public PlantData PlantData = new PlantData();
    }
}