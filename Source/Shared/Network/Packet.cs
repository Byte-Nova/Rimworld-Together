using System;

namespace Shared
{
    [Serializable]
    public class Packet
    {
        public string header;

        public byte[] contents;

        public Packet(string header, byte[] contents, bool isModded, string targetPatchName = "")
        {
            this.header = header;
            this.contents = contents;
        }

        public static Packet CreatePacketFromObject(string header, object objectToUse = null)
        {
            if (objectToUse == null) return new Packet(header, null, false);
            else
            {
                byte[] contents = Serializer.ConvertObjectToBytes(objectToUse, true);
                return new Packet(header, contents, false);
            }
        }
    }
}
