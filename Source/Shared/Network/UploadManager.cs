using System;
using System.IO;

namespace Shared
{
    public class UploadManager
    {
        public FileStream fileStream;

        private FileInfo fileInfo;

        public string filePath;

        private double partSize = 262144;
        
        public bool isLastPart;

        public Action onFinish;

        public UploadManager(string filePath) { this.filePath = filePath; }

        public void PrepareUpload()
        {
            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            fileInfo = new FileInfo(filePath);
        }

        public byte[] ReadFilePart()
        {
            double bytesToRead;
            if (fileStream.Position + partSize <= fileInfo.Length) bytesToRead = partSize;
            else
            {
                bytesToRead = fileInfo.Length - fileStream.Position;
                isLastPart = true;
            }

            byte[] toReturn = new byte[(int)bytesToRead];
            fileStream.Read(toReturn, 0, (int)bytesToRead);

            if (isLastPart) FinishFileWrite();
            return toReturn;
        }

        public void FinishFileWrite()
        {
            fileStream.Close();
            fileStream.Dispose();
        }
    }
}
