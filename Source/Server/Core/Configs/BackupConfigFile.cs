namespace GameServer.Core.Configs
{
    public class BackupConfigFile
    {
        public bool AutomaticBackups = true;

        public float IntervalHours = 24f;

        public bool AutomaticDeletion = true;

        public int Amount = 3;
    }
}
