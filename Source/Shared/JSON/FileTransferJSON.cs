using System;

namespace Shared.JSON
{
    [Serializable]
    public class FileTransferJSON
    {
        public double fileSize;
        public double fileParts;
        public byte[] fileBytes;

        public bool isLastPart;
        public string additionalInstructions;
    }
}
