using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
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