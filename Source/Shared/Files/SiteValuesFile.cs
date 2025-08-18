using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    public class SiteValuesFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public int TimeIntervalMinutes { get; set; } = 30;
        
        public SiteInfoFile[] SiteInfoFiles { get; set; } = new SiteInfoFile[0];

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
                SiteValuesFile file = new SiteValuesFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}
