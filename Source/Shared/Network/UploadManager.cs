using System.IO;

namespace Shared.Network
{
    public class UploadManager
    {
        private FileStream fileStream;
        private FileInfo fileInfo;

        public string filePath;
        public string fileName;
        public double fileSize;
        public double fileParts;

        private double partSize = 262144;
        public bool isLastPart;

        public void PrepareUpload(string filePath)
        {
            this.filePath = filePath;

            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            fileInfo = new FileInfo(filePath);

            fileName = Path.GetFileName(filePath);
            fileSize = fileInfo.Length;
            fileParts = fileInfo.Length / partSize;
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
