using System;
using System.Reflection;

namespace Shared
{
    [Serializable]
    public class Packet
    {
        public string header;

        public byte[] contents;

        public bool isModded;

        public Packet(string header, byte[] contents, bool isModded)
        {
            this.header = header;
            this.contents = contents;
            this.isModded = isModded;
        }

        public static Packet CreatePacketFromObject(string header, object objectToUse = null)
        {
            if (objectToUse == null) return new Packet(header, null, false);
            else
            {
                byte[] contents = Serializer.ConvertObjectToBytes(objectToUse);
                return new Packet(header, contents, false);
            }
        }

        public static Packet CreateModdedPacketFromObject(string header, object objectToUse = null)
        {
            if (objectToUse == null) return new Packet(header, null, true);
            else
            {
                byte[] contents = Serializer.ConvertObjectToBytes(objectToUse);
                return new Packet(header, contents, true);
            }
        }
    }
}
