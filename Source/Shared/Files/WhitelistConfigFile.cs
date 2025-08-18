using Shared;
using Shared.Files;
using System;
using System.Collections.Generic;
using System.IO;

namespace Shared.Files
{
    public class WhitelistConfigFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public bool UseWhitelist { get; set; } = false;

        public List<string> WhitelistedUsers { get; set; } = new List<string>() { };

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
                WhitelistConfigFile file = new WhitelistConfigFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}
