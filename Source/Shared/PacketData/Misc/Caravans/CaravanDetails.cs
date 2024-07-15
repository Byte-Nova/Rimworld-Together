using System;

namespace Shared
{
    [Serializable]
    public class CaravanDetails
    {
        public int ID;
        public int tile;
        public string owner;
        public double timeSinceRefresh;
    }
}