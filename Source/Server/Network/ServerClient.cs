using Shared;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    //Class object for the client connecting into the server. Contains all important data about it

    public class ServerClient
    {
        //Reference to the listener instance of this client
        [NonSerialized] public Listener listener;

        public string username = "Unknown";

        public string uid;

        public string password;

        public string factionName;

        public bool hasFaction;

        public bool isAdmin;

        public bool isBanned;

        public double activityProtectionTime;

        public double eventProtectionTime;

        public double aidProtectionTime;

        [NonSerialized] public ServerClient inVisitWith;

        [NonSerialized] public List<string> runningMods = new List<string>();

        [NonSerialized] public List<string> allyPlayers = new List<string>();

        [NonSerialized] public List<string> enemyPlayers = new List<string>();

        public string SavedIP { get; set; }

        public ServerClient(TcpClient tcp)
        {
            if (tcp == null) return;
            else SavedIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        }

        public void UpdateEventTime()
        { 
            eventProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveToUserFile();
        }

        public void UpdateAidTime() 
        { 
            aidProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveToUserFile();
        }

        public void UpdateAdmin(bool mode)
        {
            isAdmin = mode;
            SaveToUserFile();
        }

        public void UpdateBan(bool mode)
        {
            isBanned = mode;
            SaveToUserFile();
        }

        public void UpdateFaction(string updatedFactionName)
        {
            if (string.IsNullOrWhiteSpace(updatedFactionName))
            {
                hasFaction = false;
                factionName = null;
            }

            else
            {
                hasFaction = true;
                factionName = updatedFactionName;
            }

            SaveToUserFile();
        }

        public void SaveToUserFile()
        {
            UserFile userFile = UserManager.GetUserFileFromName(username);
            userFile.Uid = uid;
            userFile.Username = username;
            userFile.Password = password;
            userFile.FactionName = factionName;
            userFile.HasFaction = hasFaction;
            userFile.IsAdmin = isAdmin;
            userFile.IsBanned = isBanned;
            userFile.EnemyPlayers = enemyPlayers;
            userFile.AllyPlayers = allyPlayers;
            userFile.SavedIP = SavedIP;
            userFile.ActivityProtectionTime = activityProtectionTime;
            userFile.EventProtectionTime = eventProtectionTime;
            userFile.AidProtectionTime = aidProtectionTime;

            userFile.SaveUserFile();
        }

        public void LoadFromUserFile()
        {
            UserFile file = UserManager.GetUserFile(this);
            uid = file.Uid;
            username = file.Username;
            password = file.Password;
            factionName = file.FactionName;
            hasFaction = file.HasFaction;
            isAdmin = file.IsAdmin;
            isBanned = file.IsBanned;
            enemyPlayers = file.EnemyPlayers;
            allyPlayers = file.AllyPlayers;
            activityProtectionTime = file.ActivityProtectionTime;
            eventProtectionTime = file.EventProtectionTime;
            aidProtectionTime = file.AidProtectionTime;

            Logger.Warning(uid);
            Logger.Warning(username);
            Logger.Warning(password);
            Logger.Warning(factionName);
            Logger.Warning(hasFaction.ToString());
            Logger.Warning(isAdmin.ToString());
            Logger.Warning(isBanned.ToString());
            Logger.Warning(enemyPlayers.ToString());
            Logger.Warning(allyPlayers.ToString());
            Logger.Warning(activityProtectionTime.ToString());
            Logger.Warning(eventProtectionTime.ToString());
            Logger.Warning(aidProtectionTime.ToString());
        }
    }
}
