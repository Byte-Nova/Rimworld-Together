using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    public class StorytellerValuesFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public bool EnforceStoryteller { get; set; } = false;

        public string StorytellerDefname { get; set; } = string.Empty;

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
                StorytellerValuesFile file = new StorytellerValuesFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}