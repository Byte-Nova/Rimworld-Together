using GameClient.Files;
using Shared;
using System.IO;

namespace GameClient.Core.Preferences
{
    public static class ConnectionDataManager
    {
        public static void SaveConnectionData(string ip, string port)
        {
            ConnectionDataFile newConnectionData;
            if (File.Exists(Master.connectionDataPath)) newConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
            else newConnectionData = new ConnectionDataFile();

            newConnectionData.IP = ip;
            newConnectionData.Port = port;

            Serializer.SerializeToFile(Master.connectionDataPath, newConnectionData);
        }

        public static ConnectionDataFile LoadConnectionData()
        {
            if (File.Exists(Master.connectionDataPath)) return Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
            else return new ConnectionDataFile();
        }
    }
}