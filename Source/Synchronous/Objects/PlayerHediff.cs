using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronous.Objects
{
    public class PlayerHediff
    {
        public enum HediffMode { Add, Remove, Tend }

        public HediffMode Mode = HediffMode.Add;

        public int MapTile { get; set; } = -1;

        public string PawnID { get; set; } = string.Empty;

        public string HediffDefname { get; set; } = string.Empty;

        public string PartDefname { get; set; } = string.Empty;

        public float Severity { get; set; } = -1;

        public bool IsPermanent { get; set; } = false;

        public float TendQuality { get; set; } = -1;
    }
}
