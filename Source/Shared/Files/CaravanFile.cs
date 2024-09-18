using System;

namespace Shared
{
    [Serializable]
    public class CaravanFile
    {
        public int ID;

        public int Tile;

        public string Owner;

        public double TimeSinceRefresh;
    }
}