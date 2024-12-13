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

        public string targetPatchName;

        public Packet(string header, byte[] contents, bool isModded, string targetPatchName = "")
        {
            this.header = header;
            this.contents = contents;
            this.isModded = isModded;
            this.targetPatchName = targetPatchName;
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

        public static Packet CreateModdedPacketFromObject(string header, string targetPatchName, object objectToUse = null)
        {
            if (objectToUse == null) return new Packet(header, null, true);
            else
            {
                byte[] contents = Serializer.ConvertObjectToBytes(objectToUse, true);
                return new Packet(header, contents, true, targetPatchName);
            }
        }
    }
}
