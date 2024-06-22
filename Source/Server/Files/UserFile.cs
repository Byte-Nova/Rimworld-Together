using Shared;

namespace GameServer
{
    [Serializable]
    public class UserFile
    {
        public string Uid;

        public string Username;

        public string Password;

        public string FactionName;

        public bool HasFaction;

        public bool IsAdmin;

        public bool IsBanned;

        public string SavedIP;

        public double ActivityProtectionTime;

        public double EventProtectionTime;

        public double AidProtectionTime;

        public List<string> AllyPlayers = new List<string>();

        public List<string> EnemyPlayers = new List<string>();

        public void UpdateFaction(string updatedFactionName)
        {
            if (string.IsNullOrWhiteSpace(updatedFactionName))
            {
                HasFaction = false;
                FactionName = null;
            }

            else
            {
                HasFaction = true;
                FactionName = updatedFactionName;
            }

            SaveUserFile();
        }

        public void UpdateActivity()
        {
            ActivityProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveUserFile();
        }

        public void SaveUserFile()
        {
            string savePath = Path.Combine(Master.usersPath, Username + UserManager.fileExtension);
            Serializer.SerializeToFile(savePath, this);
        }
    }
}
