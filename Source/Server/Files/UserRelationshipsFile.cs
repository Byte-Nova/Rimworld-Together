namespace GameServer
{
    [Serializable]
    public class UserRelationshipsFile
    {
        public List<string> AllyPlayers = new List<string>();

        public List<string> EnemyPlayers = new List<string>();
    }
}