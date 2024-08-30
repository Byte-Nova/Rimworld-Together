namespace GameServer
{
    [Serializable]
    public class SettlementFile
    {
        public int tile;

        public string owner;

        public bool isShip = false;
    }
    public class SpaceSettlementFile : SettlementFile 
    {
        public float radius;
        public float phi;
        public float theta;
    }
}
