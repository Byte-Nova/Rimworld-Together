using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class ModConfigData
    {
        public ModConfigStepMode _stepMode;

        public ModConfigFile _configFile;
    }
}