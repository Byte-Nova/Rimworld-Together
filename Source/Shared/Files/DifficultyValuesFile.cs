using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    [Serializable]
    public class DifficultyValuesFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public bool EnforceDifficulty { get; set; } = false;

        public string ScribeData { get; set; } = string.Empty;

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
                DifficultyValuesFile file = new DifficultyValuesFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}
