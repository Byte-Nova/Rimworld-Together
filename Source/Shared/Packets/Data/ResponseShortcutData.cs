using static Shared.CommonEnumerators;

namespace Shared
{

    public class ResponseShortcutData
    {
        public ResponseStepMode _stepMode { get; set; } = ResponseStepMode.IllegalAction;

        public override string ToString()
        {
            return $"ResponseShortcutData:|{_stepMode}";
        }
    }
}