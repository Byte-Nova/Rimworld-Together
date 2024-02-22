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

        public static Packet CreatePacketFromJSON(string header, object jsonToUse = null, bool requiresMainThread = true)
        {
            if (jsonToUse == null) return new Packet(header, null, requiresMainThread);
            else
            {
                byte[] contents = Serializer.ConvertObjectToBytes(jsonToUse);
                return new Packet(header, contents, requiresMainThread);
            }
        }
    }
}
