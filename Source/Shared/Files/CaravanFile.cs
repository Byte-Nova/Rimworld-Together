using System;

namespace Shared
{
    [Serializable]
    public class CaravanFile
    {
        public int Tile;

        public string UID;

        public int ID;

        public override string ToString()
        {
            return $"CaravanFile:|{Tile}|{UID}|{ID}";
        }
    }
}