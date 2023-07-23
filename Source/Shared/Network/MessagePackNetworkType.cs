using MessagePack;

namespace RimworldTogether.Shared.Network
{
    [MessagePackObject]
    public struct MessagePackNetworkType
    {
        [Key(0)] public int type;
        [Key(1)] public byte[] data;

        public MessagePackNetworkType(int type, byte[] data)
        {
            this.type = type;
            this.data = data;
        }
    }
}