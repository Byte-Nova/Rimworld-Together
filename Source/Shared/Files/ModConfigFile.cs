namespace Shared
{
    public class ModConfigFile
    {
        public string[] UnsortedMods = new string[0];

        public ulong[] AllModIds = new ulong[0];

        public string[] RequiredMods = new string[0];

        public string[] OptionalMods = new string[0];

        public string[] ForbiddenMods = new string[0];

        public bool EnforcedConfigs = false;

        public string[] ModFileNames = new string[0];

        public string[] ModConfigs = new string[0];

        public override string ToString()
        {
            return $"ModConfigFile:|Total Mods : {UnsortedMods?.Length ?? 0}";
        }
    }
}