using System;

namespace Shared
{
    [Serializable]
    public class FileTransferData
    {
        public double _fileSize;

        public double _fileParts;

        public byte[] _fileBytes;

        public bool _isLastPart;
        
        public int _instructions = -1;
    }
}
