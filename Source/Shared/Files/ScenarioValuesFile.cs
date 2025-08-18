using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    public class ScenarioValuesFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public bool EnforceScenario { get; set; } = false;

        public string ScenarioName { get; set; } = string.Empty;

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
                ScenarioValuesFile file = new ScenarioValuesFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}