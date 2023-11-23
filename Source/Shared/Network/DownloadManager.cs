using System.IO;

namespace Shared.Network
{
    public class DownloadManager
    {
        private FileStream fileStream;

        public string filePath;
        public double fileSize;
        public double fileParts;

        public bool isLastPart;

        public void PrepareDownload(string filePath, double fileParts)
        {
            this.fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            this.fileParts = fileParts;
        }

        public void WriteFilePart(byte[] partBytes)
        {
            fileStream.Write(partBytes, 0, partBytes.Length);
            fileStream.Flush();
        }

        public void FinishFileWrite()
        {
            fileStream.Close();
            fileStream.Dispose();
        }
    }
}
