using Shared.Files;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class TransferData
    {
        public enum TransferMode { Gift, Trade, Rebound, Pod }

        public enum TransferLocation { Caravan, Settlement, Pod }

        public enum TransferStepMode { TradeRequest, TradeAccept, TradeReject, TradeReRequest, TradeReAccept, TradeReReject, Recover, Pod }

        public TransferStepMode _stepMode { get; set; } = TransferStepMode.TradeRequest;

        public TransferMode _transferMode { get; set; } = TransferMode.Gift;

        public int _fromTile { get; set; } = -1;

        public int _toTile { get; set; } = -1;

        public List<HumanFile> _humans { get; set; } = new List<HumanFile>();

        public List<string> _animals { get; set; } = new List<string>();

        public List<string> _things { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"TransferData:|{_stepMode}|{_transferMode}|{_fromTile}|{_toTile}|{_humans?.Count ?? 0}|{_animals?.Count ?? 0}|{_things?.Count ?? 0}";
        }
    }
}
