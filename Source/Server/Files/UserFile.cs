namespace RimworldTogether.GameServer.Files
{
    [Serializable]
    public class UserFile
    {
        public string uid;

        public string username;

        public string password;

        public string factionName;

        public bool hasFaction;

        public bool isAdmin;

        public bool isBanned;

        public string SavedIP;

        public List<string> allyPlayers = new List<string>();

        public List<string> enemyPlayers = new List<string>();
    }
}
