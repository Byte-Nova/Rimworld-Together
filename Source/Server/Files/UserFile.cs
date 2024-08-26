using Shared;

namespace GameServer
{
    [Serializable]
    public class UserFile
    {
        public string Username = "Unknown";

        public string Password;

        public string Uid;

        public string FactionName;

        public bool HasFaction;

        public bool IsAdmin;

        public bool IsBanned;

        public string SavedIP;

        public double ActivityProtectionTime;

        public double EventProtectionTime;

        public double AidProtectionTime;

        public List<string> RunningMods = new List<string>();

        public List<string> AllyPlayers = new List<string>();

        public List<string> EnemyPlayers = new List<string>();

        [NonSerialized] public Semaphore savingSemaphore = new Semaphore(1, 1);

        public void SetLoginDetails(LoginData data)
        {
            //Don't force save in this function because it wouldn't server any purpose

            Username = data.username;
            Password = data.password;
            Uid = Hasher.GetHashFromString(Username);
        }

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

        public void UpdateEventTime()
        {
            EventProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveUserFile();
        }

        public void UpdateAidTime()
        {
            AidProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveUserFile();
        }

        public void UpdateActivityTime()
        {
            ActivityProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveUserFile();
        }

        public void UpdateAdmin(bool mode)
        {
            IsAdmin = mode;
            SaveUserFile();
        }

        public void UpdateBan(bool mode)
        {
            IsBanned = mode;
            SaveUserFile();
        }

        public void UpdateMods(List<string> mods)
        {
            RunningMods = mods;
            SaveUserFile();
        }

        public void SaveUserFile()
        {
            savingSemaphore.WaitOne();

            string savePath = Path.Combine(Master.usersPath, Username + UserManagerHelper.fileExtension);
            Serializer.SerializeToFile(savePath, this);

            savingSemaphore.Release();
        }
    }
}
