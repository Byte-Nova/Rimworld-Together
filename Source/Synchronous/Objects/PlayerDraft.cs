using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronous.Objects
{
    public class PlayerDraft
    {
        public int MapTile { get; set; } = -1;

        public string PawnID { get; set; } = string.Empty;

        public bool DraftValue { get; set; } = false;
    }
}
