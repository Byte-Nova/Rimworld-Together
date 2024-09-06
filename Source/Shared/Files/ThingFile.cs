using System;

namespace Shared
{
    [Serializable]

    public class ThingFile
    {
        public string DefName;

        public string MaterialDefName;

        public int Quantity;

        public int Quality;

        public int Hitpoints;

        public bool IsMinified;

        public TransformComponent Transform = new TransformComponent();

        public GenepackComponent GenepackComponent = new GenepackComponent();

        public BookComponent BookComponent = new BookComponent();

        public PlantComponent PlantComponent = new PlantComponent();
    }
}