using Shared.Files.Maps;
using Shared.Files.Synchronous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPNetwork.Packets
{
    public class SynchronousData
    {
        public enum StepMode { Ask, Accept, Reject, Start }

        public StepMode _stepMode { get; set; } = StepMode.Ask;

        public int _fromTile { get; set; } = -1;

        public int _toTile { get; set; } = -1;

        public byte[] _contents { get; set; } = null;

        public PartyFile _party { get; set; } = null;
    }
}
