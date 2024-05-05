using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class FileTransferData
    {
        public double fileSize;
        public double fileParts;
        public byte[] fileBytes;

        public bool isLastPart;
        public SaveMode additionalInstructions;
    }
}
