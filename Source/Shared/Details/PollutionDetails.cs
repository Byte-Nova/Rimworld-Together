using System;

namespace Shared
{
    [Serializable]
    public class PollutionDetails
    {
        public int Tile { get; set; }

        public float Quantity { get; set; }
    }
}