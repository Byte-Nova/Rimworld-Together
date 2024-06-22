using Shared;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace GameServer
{
    //Class object for the client connecting into the server. Contains all important data about it

    public class ServerClient
    {
        //Reference to the listener instance of this client
        [NonSerialized] public Listener listener;

        [NonSerialized] public UserFile userFile;

        [NonSerialized] public ServerClient InVisitWith;

        public string Uid;

        public string Username = "Unknown";

        public string Password;

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

        public ServerClient(TcpClient tcp)
        {
            if (tcp == null) return;
            else SavedIP = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        }

        public void UpdateEventTime()
        { 
            EventProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveToUserFile();
        }

        public void UpdateAidTime() 
        { 
            AidProtectionTime = TimeConverter.CurrentTimeToEpoch();
            SaveToUserFile();
        }

        public void UpdateAdmin(bool mode)
        {
            IsAdmin = mode;
            SaveToUserFile();
        }

        public void UpdateBan(bool mode)
        {
            IsBanned = mode;
            SaveToUserFile();
        }

        public void UpdateMods(List<string> mods)
        {
            RunningMods = mods;
            SaveToUserFile();
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

            SaveToUserFile();
        }

        //TODO
        //MAKE SURE THIS WORKS AS EXPECTED

        public void SaveToUserFile()
        {
            UserFile userFile = UserManager.GetUserFileFromName(Username);

            foreach (FieldInfo clientField in typeof(ServerClient).GetFields())
            {
                foreach (FieldInfo fileField in typeof(UserFile).GetFields())
                {
                    if (fileField.Name == clientField.Name)
                    {
                        fileField.SetValue(userFile, clientField.GetValue(this));
                        Logger.Warning($"Saving > {fileField.GetValue(userFile)}");
                    }
                }
            }

            userFile.SaveUserFile();
        }

        //TODO
        //MAKE SURE THIS WORKS AS EXPECTED

        public void LoadFromUserFile()
        {
            UserFile file = UserManager.GetUserFile(this);

            foreach (FieldInfo fileField in typeof(UserFile).GetFields())
            {
                foreach (FieldInfo clientField in typeof(ServerClient).GetFields())
                {
                    if (clientField.Name == fileField.Name)
                    {
                        clientField.SetValue(this, fileField.GetValue(file));
                        Logger.Warning($"Loading > {clientField.GetValue(this)}");
                    }
                }
            }
        }
    }
}
