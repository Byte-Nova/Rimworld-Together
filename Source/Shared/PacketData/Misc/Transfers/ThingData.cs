using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class ThingData
    {
        public string defName;
        public string materialDefName;
        public int quantity;
        public string quality;

        //Complex items
        public List<string> genepackContent = new List<string>();
        public BookData book;

        public bool isMinified;
        public int hitpoints;

        public string[] position;
        public int rotation;

        public float growthTicks;
    }
}