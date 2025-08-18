using Shared;
using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    public class BackupConfigFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public bool AutomaticBackups { get; set; } = true;

        public float IntervalHours { get; set; } = 24f;

        public bool AutomaticDeletion { get; set; } = true;

        public int Amount { get; set; } = 3;

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
                BackupConfigFile file = new BackupConfigFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}
