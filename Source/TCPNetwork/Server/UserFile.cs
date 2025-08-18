using TCPNetwork.Packets;
using Shared;
using Shared.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace TCPNetwork.Server
{
    public class UserFile
    {
        public string Uid;

        public string Label;

        public bool IsAdmin;

        public bool IsBanned;

        public string SavedIP;

        public double EventProtectionTime;

        public double AidProtectionTime;

        public string GuildName;

        public string[] RunningMods;

        public List<string> AllyPlayers = new List<string>();

        public List<string> EnemyPlayers = new List<string>();

        public SiteConfigFile[] SiteConfigs = Array.Empty<SiteConfigFile>();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);

        public string UsersPath { get; set; } = string.Empty;

        public static string fileExtension { get; set; } = ".mpuser";

        public UserFile(string usersPath) { this.UsersPath = usersPath; }

        public void SetLoginDetails(LoginData data)
        {
            // No need to save these details

            Uid = data._uid;
            Label = data._username;
        }

        public void SaveUserFile()
        {
            SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(UsersPath, Uid + fileExtension), this); }
            catch (Exception e) { throw new Exception(e.ToString()); }

            SavingSemaphore.Release();
        }

        public void UpdateFaction(GuildFile toUpdateWith)
        {
            if (toUpdateWith == null) GuildName = null;
            else GuildName = toUpdateWith.Name;

            SaveUserFile();
        }

        public void UpdateEventTime()
        {
            EventProtectionTime = TimeConverter.GetCurrentTimeToEpoch();
            SaveUserFile();
        }

        public void UpdateAidTime()
        {
            AidProtectionTime = TimeConverter.GetCurrentTimeToEpoch();
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

        public void UpdateMods(string[] mods)
        {
            RunningMods = mods;
            SaveUserFile();
        }
    }
}
