using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Shared
{
    public static class GZip
    {
        public static byte[] CompressBytes(byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }

            return memoryStream.ToArray();
        }

        public static byte[] DecompressBytes(byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            using MemoryStream outputStream = new MemoryStream();
            using (GZipStream decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                decompressStream.CopyTo(outputStream);
            }

            return outputStream.ToArray();
        }
    }
}
