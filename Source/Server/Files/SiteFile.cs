namespace GameServer
{
    [Serializable]
    public class SiteFile
    {
        public int tile;

        public string owner;

        public int type;

        public byte[] workerData;

        public bool isFromFaction;

        public string factionName;
    }
}
