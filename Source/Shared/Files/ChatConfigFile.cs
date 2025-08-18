using Shared;
using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    public class ChatConfigFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public bool EnableMoTD { get; set; } = false;

        public string MessageOfTheDay { get; set; } = "Remember to drink water";

        public bool LoginNotifications { get; set; } = false;

        public bool DisconnectNotifications { get; set; } = false;

        public override void Save()
        {
            try { Serializer.SerializeToFile(Path, this); }
            catch (Exception e) { throw new Exception(e.ToString()); }
        }

        public static object Load<T>()
        {
            if (File.Exists(Path)) return Serializer.SerializeFromFile<T>(Path);
            else
            {
                ChatConfigFile file = new ChatConfigFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}