namespace GameServer
{
    [Serializable]
    public class SiteFile
    {
        public string tile;

        public string owner;

        public string type;

        public byte[] workerData;

        public bool isFromFaction;

        public string factionName;
    }
}
