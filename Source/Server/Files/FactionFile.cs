namespace RimworldTogether.GameServer.Files
{
    [Serializable]
    public class FactionFile
    {
        public string factionName;

        public List<string> factionMembers = new List<string>();

        public List<string> factionMemberRanks = new List<string>();
    }
}
