namespace Shared.Files.Maps
{
    public class MapTile
    {
        public byte TileByte { get; set; } = byte.MaxValue;

        public byte RoofByte { get; set; } = byte.MaxValue;

        public bool IsPolluted { get; set; } = false;
    }
}