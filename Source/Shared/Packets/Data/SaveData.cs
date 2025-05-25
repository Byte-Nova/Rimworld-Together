using static Shared.CommonEnumerators;

namespace Shared
{

    public class SaveData
    {
        public SaveStepMode _stepMode { get; set; } = SaveStepMode.Send;

        public SaveMode _instructions { get; set; } = SaveMode.Disconnect;

        public byte[] _fileBytes { get; set; } = null;

        public override string ToString()
        {
            return $"SaveData:|{_stepMode}|{_instructions}|{_fileBytes?.Length ?? 0}b";
        }
    }
}
