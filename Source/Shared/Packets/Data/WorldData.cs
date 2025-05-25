using static Shared.CommonEnumerators;

namespace Shared
{

    public class WorldData
    {
        public WorldStepMode _stepMode { get; set; } = WorldStepMode.AskFor;

        public byte[] _fileBytes { get; set; } = null;

        public override string ToString()
        {
            return $"WorldData:|{_stepMode}|{_fileBytes?.Length ?? 0}b";
        }
    }
}
