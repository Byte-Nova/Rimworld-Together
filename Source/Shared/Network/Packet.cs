using RimworldTogether.Shared.Serializers;
using System;

namespace RimworldTogether.Shared.Network
{
    [Serializable]
    public class Packet
    {
        public string header;

        public byte[] contents;

        public Packet(string header, byte[] contents = null)
        {
            this.header = header;

            this.contents = contents;
        }

        public static Packet CreatePacketFromJSON(string pointer, object jsonToUse = null)
        {
            if (jsonToUse == null) return new Packet(pointer, null);
            else
            {
                byte[] contents = ObjectConverter.ConvertObjectToBytes(jsonToUse);
                return new Packet(pointer, contents);
            }
        }
    }
}
