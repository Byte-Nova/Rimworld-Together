using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronous.Objects
{
    public class PlayerMentalState
    {
        public enum MentalMode { Add, Remove }

        public MentalMode Mode { get; set; } = MentalMode.Add;

        public int MapTile { get; set; } = -1;

        public string PawnID { get; set; } = string.Empty;

        public byte MentalStateByte { get; set; } = byte.MaxValue;
    }
}
