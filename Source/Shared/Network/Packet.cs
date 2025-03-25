using System;
using System.IO;

namespace Shared
{
    public class Packet
    {
        public static int DefaultPacketSizeInBytes { get; private set; } = 4;
        public static int CurrentPacketSizeInBytes { get; private set; }
        public string Header = string.Empty;
        public byte[] Contents = Array.Empty<byte>();

        public Packet(string header, byte[] contents)
        {
            Header = header;
            Contents = contents;
        }

        public static Packet CreateFromObject(string header, object objectToUse)
        {
            byte[] contents = Serializer.ConvertObjectToBytes(objectToUse);
            return new Packet(header, contents);
        }

        public static void SetPacketSize(int newSize)
        {
            CurrentPacketSizeInBytes = newSize;
        }

        public static byte[] CompressPacket(Packet packet)
        {
            return Serializer.ConvertObjectToBytes(packet, true);
        }

        public static Packet DecompressPacket(byte[] contents)
        {
            try
            {
                // If the first two bytes indicate GZip (0x1F, 0x8B) then decompress.
                if (contents.Length >= 2 && contents[0] == 0x1F && contents[1] == 0x8B)
                {
                    return Serializer.ConvertBytesToObject<Packet>(contents, true);
                }
                else
                {
                    return Serializer.ConvertBytesToObject<Packet>(contents, false);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Failed to decompress or deserialize packet. Data may be corrupted.", ex);
            }
        }
    }
}