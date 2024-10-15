using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SaveData
    {
        public SaveStepMode _stepMode;

        public double _fileSize;

        public double _fileParts;

        public byte[] _fileBytes;

        public bool _isLastPart;
        
        public int _instructions = -1;
    }
}
