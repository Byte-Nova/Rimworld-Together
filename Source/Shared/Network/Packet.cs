using System;

namespace RimworldTogether
{
    [Serializable]
    public class Packet
    {
        public string header;

        public string[] contents;

        public Packet(string header, string[] contents = null)
        {
            this.header = header;

            this.contents = contents;
        }
    }
}
