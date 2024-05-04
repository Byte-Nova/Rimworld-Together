using static Shared.CommonEnumerators;

namespace GameServer
{
    [Serializable]
    public class FactionFile
    {
        public string factionName;

        public List<string> factionMembers = new List<string>();

        public FactionRanks[] factionMemberRanks = new FactionRanks[0];
    }
}
