using System;

namespace RimworldTogether.Shared.JSON.Things
{
    [Serializable]
    public class ItemDetailsJSON
    {
        public string defName;

        public string materialDefName;

        public string quantity;

        public string quality;

        public string hitpoints;

        public bool isMinified;

        public string position;

        public string rotation;
    }
}