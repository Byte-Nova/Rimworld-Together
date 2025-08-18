namespace TCPNetwork.Packets
{
    public class InformationData
    {
        public enum InfoStepMode { Connection, Wealth }

        public InfoStepMode _stepMode { get; set; } = InfoStepMode.Connection;

        public bool _isPlayerOnline { get; set; } = false;

        public int _settlementWealth { get; set; } = -1;

        public int _settlementTile { get; set; } = -1;
    }
}