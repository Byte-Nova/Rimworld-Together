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
            userFile.uid = uid;
            userFile.username = username;
            userFile.password = password;
            userFile.factionName = factionName;
            userFile.hasFaction = hasFaction;
            userFile.isAdmin = isAdmin;
            userFile.isBanned = isBanned;
            userFile.enemyPlayers = enemyPlayers;
            userFile.allyPlayers = allyPlayers;
            userFile.savedIP = SavedIP;
            userFile.eventProtectionTime = eventProtectionTime;
            userFile.aidProtectionTime = aidProtectionTime;

            string savePath = Path.Combine(Master.usersPath, username + UserManager.fileExtension);
            Serializer.SerializeToFile(savePath, userFile);
        }

        public void LoadFromUserFile()
        {
            UserFile file = UserManager.GetUserFile(this);
            uid = file.uid;
            username = file.username;
            password = file.password;
            factionName = file.factionName;
            hasFaction = file.hasFaction;
            isAdmin = file.isAdmin;
            isBanned = file.isBanned;
            enemyPlayers = file.enemyPlayers;
            allyPlayers = file.allyPlayers;
            eventProtectionTime = file.eventProtectionTime;
            aidProtectionTime = file.aidProtectionTime;
        }
    }
}
