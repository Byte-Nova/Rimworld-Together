using static Shared.CommonEnumerators;

namespace Shared
{

    public class ModConfigData
    {
        public ModConfigStepMode _stepMode { get; set; } = ModConfigStepMode.Send;

        public ModConfigFile _configFile { get; set; } = null;
    }
}