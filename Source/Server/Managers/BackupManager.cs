using GameServer.Core;
using GameServer.Misc;
using Shared;
using Shared.Files;
using System.IO.Compression;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class BackupManager
    {
        public static readonly string fileExtension = ".zip";

        private static readonly Semaphore savingSemaphore = new Semaphore(1, 1);

        public static void BackupServer()
        {
            savingSemaphore.WaitOne();

            try
            {
                string backupName = $"Server_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
                string backupPath = $"{Master.BackupServerPath + Path.DirectorySeparatorChar}{backupName}{fileExtension}";

                List<string> toArchive = new List<string>();
                toArchive.AddRange(Directory.GetFiles(Master.AssetsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.ConfigsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.LogsPath, "*.*", SearchOption.AllDirectories));

                CreateArchive(toArchive, backupPath);

                if (Directory.GetFiles(Master.BackupServerPath).Count() > Master.BackupConfig.Amount && Master.BackupConfig.AutomaticDeletion == true)
                {
                    DeleteOldestArchive();
                }

                InformationDisplayer.DisplayServerBackup(backupPath);
            }
            catch (Exception ex) { Printer.Error(ex.ToString()); }

            savingSemaphore.Release();
        }

        public static void BackupUser(string uid, bool persistent = false)
        {
            savingSemaphore.WaitOne();

            try
            {
                string playerArchivedSavePath = Path.Combine(Master.BackupUsersPath, uid);
                if (persistent) playerArchivedSavePath += " - persistent";
                playerArchivedSavePath += fileExtension;

                if (File.Exists(playerArchivedSavePath))
                {
                    if (persistent == true)
                    {
                        Printer.Error($"Could not backup user {uid} because the file {playerArchivedSavePath} already exist. Consider running a non-persistent backup if you want to overwrite it.");
                        savingSemaphore.Release();
                        return;
                    }

                    else
                    {
                        File.Delete(playerArchivedSavePath);
                        Printer.Warning($"Deleting backup of {uid} because he already had one.", LogImportanceMode.Verbose);
                    }
                }

                List<string> toArchive = new List<string>();

                string userFilePath = Path.Combine(Master.UsersPath, uid + UserManagerH.fileExtension);
                if (File.Exists(userFilePath)) toArchive.Add(userFilePath);

                string userSavePath = Path.Combine(Master.SavesPath, uid + SaveManager.fileExtension);
                if (File.Exists(userSavePath)) toArchive.Add(userSavePath);

                SiteFile[] playerSites = SiteManagerHelper.GetAllSitesFromUID(uid);
                foreach (SiteFile site in playerSites) toArchive.Add(Path.Combine(Master.SitesPath, site.Tile + SiteManagerHelper.fileExtension));

                SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(uid);
                foreach (SettlementFile settlementFile in playerSettlements) toArchive.Add(Path.Combine(Master.SettlementsPath, settlementFile.Tile + SettlementManager.fileExtension));

                CreateArchive(toArchive, playerArchivedSavePath);

                InformationDisplayer.DisplayUserBackup(playerArchivedSavePath);
            }
            catch (Exception ex) { Printer.Error(ex.ToString()); }

            savingSemaphore.Release();
        }

        private static void CreateArchive(List<string> files, string toPath)
        {
            using FileStream zip = new FileStream(toPath, FileMode.CreateNew);
            using ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Create);

            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    string relativePath = Path.GetRelativePath(Master.MainPath, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }
        }

        private static void DeleteOldestArchive()
        {
            while (Directory.GetFiles(Master.BackupServerPath).Length > Master.BackupConfig.Amount)
            {
                FileSystemInfo fileInfo = new DirectoryInfo(Master.BackupServerPath).GetFileSystemInfos().OrderBy(file => file.CreationTime).First();
                Printer.Warning($"Deleting backup {fileInfo.Name} because we've reached the limit of {Master.BackupConfig.Amount}", LogImportanceMode.Verbose);
                fileInfo.Delete();
            }
        }

        public static void AutoBackup()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromHours(Master.BackupConfig.IntervalHours));

                try { BackupServer(); }
                catch (Exception e) { Printer.Error($"Backup tick failed, this should never happen. Exception > {e}"); }
            }
        }
    }
}
