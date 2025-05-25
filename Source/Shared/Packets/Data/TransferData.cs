using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{

    public class TransferData
    {
        public TransferStepMode _stepMode { get; set; } = TransferStepMode.TradeRequest;

        public TransferMode _transferMode { get; set; } = TransferMode.Gift;

        public int _fromTile { get; set; } = -1;

        public int _toTile { get; set; } = -1;

        public List<HumanFile> _humans { get; set; } = new List<HumanFile>();

        public List<AnimalFile> _animals { get; set; } = new List<AnimalFile>();

        public List<ThingFile> _things { get; set; } = new List<ThingFile>();

        public override string ToString()
        {
            return $"TransferData:|{_stepMode}|{_transferMode}|{_fromTile}|{_toTile}|{_humans?.Count ?? 0}|{_animals?.Count ?? 0}|{_things?.Count ?? 0}";
        }
    }
}
