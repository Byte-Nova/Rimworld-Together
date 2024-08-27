using System;

namespace Shared
{
    [Serializable]
    public class FileTransferData
    {
        public double fileSize;

        public double fileParts;

        public byte[] fileBytes;

        public bool isLastPart;
        
        public int instructions = -1;
    }
}
