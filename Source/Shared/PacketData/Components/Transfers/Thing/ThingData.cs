using System;

namespace Shared
{
    [Serializable]

    public class ThingData
    {
        public string defName;

        public string materialDefName;

        public int quantity;

        public int quality;

        public int hitpoints;

        public bool isMinified;

        public float[] position;

        public int rotation;

        //Complex data

        public GenepackData genepackData = new GenepackData();

        public BookData bookData = new BookData();

        public PlantData plantData = new PlantData();
    }
}