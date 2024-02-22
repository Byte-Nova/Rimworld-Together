using System.IO;
using System.IO.Compression;

namespace Shared
{
    //Class in charge of managing compression/decompression of bytes

    public static class GZip
    {
        //Compresses a given byte array into a smaller version

        public static byte[] Compress(byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }

            return memoryStream.ToArray();
        }

        //Decompresses a given byte array into the original version

        public static byte[] Decompress(byte[] bytes)
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
