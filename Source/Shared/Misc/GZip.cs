using System;
using System.IO;
using System.IO.Compression;

namespace RimworldTogether.Shared.Misc
{
    public static class GZip
    {
        public static string Compress(byte[] input)
        {
            byte[] compressed = DoCompress(input);
            return Convert.ToBase64String(compressed);
        }

        public static byte[] Decompress(string input)
        {
            byte[] compressed = Convert.FromBase64String(input);
            return DoDecompress(compressed);
        }

        private static byte[] DoCompress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }

        private static byte[] DoDecompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source,
                    CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    decompressionStream.Read(result, 0, length);
                    return result;
                }
            }
        }
    }
}
