using System;

namespace Shared
{
    [Serializable]
    public class Packet
    {
        public string header;

        public byte[] contents;
        
        public bool requiresMainThread;

        public Packet(string header, byte[] contents, bool requiresMainThread)
        {
            this.header = header;
            this.contents = contents;
            this.requiresMainThread = requiresMainThread;
        }

        public static Packet CreatePacketFromObject(string header, object objectToUse = null, bool requiresMainThread = true)
        {
            if (objectToUse == null) return new Packet(header, null, requiresMainThread);
            else
            {
                byte[] contents = Serializer.ConvertObjectToBytes(objectToUse);
                return new Packet(header, contents, requiresMainThread);
            }
        }
    }
}
