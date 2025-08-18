using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    public class ModConfigFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public string[] UnsortedMods { get; set; } = new string[0];

        public ulong[] AllModIds { get; set; } = new ulong[0];

        public string[] RequiredMods { get; set; } = new string[0];

        public string[] OptionalMods { get; set; } = new string[0];

        public string[] ForbiddenMods { get; set; } = new string[0];

        public bool EnforcedConfigs { get; set; } = false;

        public string[] ModFileNames { get; set; } = new string[0];

        public string[] ModConfigs { get; set; } = new string[0];

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
                ModConfigFile file = new ModConfigFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}