using Shared;

namespace GameServer
{
    [Serializable]
    public class UserFile
    {
        public string Username = "Unknown";

        public string Password;

        public string Uid;
        
        public bool IsAdmin;

        public bool IsBanned;

        public string SavedIP;

        public double ActivityProtectionTime;

        public double EventProtectionTime;

        public double AidProtectionTime;

        public string[] RunningMods;

        public UserRelationshipsFile Relationships = new UserRelationshipsFile();

        public FactionFile FactionFile;

        public SiteConfigFile[] SiteConfigs = new SiteConfigFile[0];

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);

        public void SetLoginDetails(LoginData data)
        {
            //Don't force save in this function because it wouldn't server any purpose

            Username = data._username;
            Password = data._password;
            Uid = Hasher.GetHashFromString(Username);
        }

        public void UpdateFaction(FactionFile toUpdateWith)
        {
            FactionFile = toUpdateWith;

            UserManagerHelper.SaveUserFile(this);
        }

        public void UpdateEventTime()
        {
            EventProtectionTime = TimeConverter.CurrentTimeToEpoch();
            UserManagerHelper.SaveUserFile(this);
        }

        public void UpdateAidTime()
        {
            AidProtectionTime = TimeConverter.CurrentTimeToEpoch();
            UserManagerHelper.SaveUserFile(this);
        }

        public void UpdateActivityTime()
        {
            ActivityProtectionTime = TimeConverter.CurrentTimeToEpoch();
            UserManagerHelper.SaveUserFile(this);
        }

        public void UpdateAdmin(bool mode)
        {
            IsAdmin = mode;
            UserManagerHelper.SaveUserFile(this);
        }

        public void UpdateBan(bool mode)
        {
            IsBanned = mode;
            UserManagerHelper.SaveUserFile(this);
        }

        public void UpdateMods(string[] mods)
        {
            RunningMods = mods;
            UserManagerHelper.SaveUserFile(this);
        }
    }
}
