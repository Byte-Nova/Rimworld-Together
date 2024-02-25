using System;

namespace Shared
{
    [Serializable]

    public class ItemDetailsJSON
    {
        public string defName;
        public string materialDefName;
        public string quantity;
        public string quality;

        public bool isMinified;
        public string hitpoints;

        public string[] position;
        public string rotation;
    }
}