using System;

namespace Shared
{
    [Serializable]

    public class ThingDataFile
    {
        public string Hash;

        public string DefName;

        public string MaterialDefName;

        public int Quantity;

        public int Quality;

        public int Hitpoints;

        public bool IsMinified;

        //Complex data

        public TransformComponent TransformComponent = new TransformComponent();

        public GenepackComponent GenepackComponent = new GenepackComponent();

        public BookComponent BookComponent = new BookComponent();

        public XenogermComponent XenogermComponent = new XenogermComponent();

        public PlantComponent PlantComponent = new PlantComponent();
    }
}