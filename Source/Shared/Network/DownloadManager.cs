using System.IO;

namespace Shared
{
    public class DownloadManager
    {
        public FileStream fileStream;

        public string filePath;

        public bool isLastPart;

        public DownloadManager(string filePath) { this.filePath = filePath; }

        public void PrepareDownload() { fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite); }

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
