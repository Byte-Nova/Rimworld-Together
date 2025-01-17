using GameServer.Managers;
using Shared;

namespace GameServer.Files
{
    [Serializable]
    public class UserFile
    {
        public string Uid;

        public string Label;

        public bool IsAdmin;

        public bool IsBanned;

        public string SavedIP;

        public double ActivityProtectionTime;

        public double EventProtectionTime;

        public double AidProtectionTime;

        public string GuildName;

        public string[] RunningMods;

        public List<string> AllyPlayers = new List<string>();

        public List<string> EnemyPlayers = new List<string>();

        public SiteConfigFile[] SiteConfigs = Array.Empty<SiteConfigFile>();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);

        public void SetLoginDetails(LoginData data)
        {
            // No need to save these details

            Uid = data._uid;
            Label = data._username;
        }

        public void UpdateFaction(GuildFile toUpdateWith)
        {
            if (toUpdateWith == null) GuildName = null;
            else GuildName = toUpdateWith.Name;

            UserManagerH.SaveUserFile(this);
        }

        public void UpdateEventTime()
        {
            EventProtectionTime = TimeConverter.GetCurrentTimeToEpoch();
            UserManagerH.SaveUserFile(this);
        }

        public void UpdateAidTime()
        {
            AidProtectionTime = TimeConverter.GetCurrentTimeToEpoch();
            UserManagerH.SaveUserFile(this);
        }

        public void UpdateActivityTime()
        {
            ActivityProtectionTime = TimeConverter.GetCurrentTimeToEpoch();
            UserManagerH.SaveUserFile(this);
        }

        public void UpdateAdmin(bool mode)
        {
            IsAdmin = mode;
            UserManagerH.SaveUserFile(this);
        }

        public void UpdateBan(bool mode)
        {
            IsBanned = mode;
            UserManagerH.SaveUserFile(this);
        }

        public void UpdateMods(string[] mods)
        {
            RunningMods = mods;
            UserManagerH.SaveUserFile(this);
        }
    }
}
