using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Updater
{
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
    }
}
