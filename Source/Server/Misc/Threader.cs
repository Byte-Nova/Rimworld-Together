using GameServer.Managers;

namespace GameServer.Misc
{
    public static class Threader
    {
        public enum ServerMode { Sites, Backup }

        public static Task GenerateServerThread(ServerMode mode)
        {
            return mode switch
            {
                ServerMode.Sites => Task.Run(SiteManager.StartSiteTicker),
                ServerMode.Backup => Task.Run(BackupManager.AutoBackup),
                _ => throw new NotImplementedException(),
            };
        }
    }
}