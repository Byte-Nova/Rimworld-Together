namespace TCPNetwork.Packets
{
    public class InformationData
    {
        public enum InfoStepMode { Connection, Wealth }

        public InfoStepMode _stepMode { get; set; } = InfoStepMode.Connection;

        public bool _isPlayerOnline { get; set; } = false;

        public int _settlementTile { get; set; } = -1;

        public byte[] _settlementRawData { get; set; } = null;
    }
}