using static Shared.CommonEnumerators;

namespace Shared
{

    public class CaravanData
    {
        public CaravanStepMode _stepMode { get; set; } = CaravanStepMode.Add;

        public CaravanFile _caravanFile { get; set; } = null;

        public override string ToString()
        {
            return $"CaravanData:|{_stepMode}|{_caravanFile}";
        }
    }
}