using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.JSON
{
    [Serializable]
    public class FileTransferJSON
    {
        public string fileName;
        public double fileSize;
        public double fileParts;
        public byte[] fileBytes;

        public bool isLastPart;
        public string additionalInstructions;
    }
}
