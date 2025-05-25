namespace Shared
{
    public class VersionData
    {
        public enum VersionStep { Ask, Pass }

        public VersionStep _step { get; set; } = VersionStep.Ask;

        public string _version { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"VersionData:|{_step}|{_version}";
        }
    }
}