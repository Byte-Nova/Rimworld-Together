using GameServer.Core;
using GameServer.Misc;
using Shared;
using System.IO.Compression;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
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
                string backupPath = $"{Master.backupServerPath + Path.DirectorySeparatorChar}{backupName}{fileExtension}";

                List<string> toArchive = new List<string>();
                toArchive.AddRange(Directory.GetFiles(Master.configsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.factionsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.mapsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.savesPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.settlementsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.sitesPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.usersPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.caravansPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.eventsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.logsPath, "*.*", SearchOption.AllDirectories));
                CreateArchive(toArchive, backupPath);

                if (Directory.GetFiles(Master.backupServerPath).Count() > Master.backupConfig.Amount && Master.backupConfig.AutomaticDeletion == true)
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
                string playerArchivedSavePath = Path.Combine(Master.backupUsersPath, uid);
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

                string userFilePath = Path.Combine(Master.usersPath, uid + UserManagerH.fileExtension);
                if (File.Exists(userFilePath)) toArchive.Add(userFilePath);

                string userSavePath = Path.Combine(Master.savesPath, uid + SaveManager.fileExtension);
                if (File.Exists(userSavePath)) toArchive.Add(userSavePath);

                SiteFile[] playerSites = SiteManagerHelper.GetAllSitesFromUID(uid);
                foreach (SiteFile site in playerSites) toArchive.Add(Path.Combine(Master.sitesPath, site.Tile + SiteManagerHelper.fileExtension));

                SettlementFile[] playerSettlements = PlayerSettlementManager.GetAllSettlementsFromUsername(uid);
                foreach (SettlementFile settlementFile in playerSettlements) toArchive.Add(Path.Combine(Master.settlementsPath, settlementFile.Tile + PlayerSettlementManager.fileExtension));

                CaravanFile[] playerCaravans = CaravanManagerHelper.GetCaravansFromUID(uid);
                foreach (CaravanFile caravanFile in playerCaravans) toArchive.Add(Path.Combine(Master.caravansPath, caravanFile.ID + CaravanManager.fileExtension));

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
                    string relativePath = Path.GetRelativePath(Master.mainPath, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }
        }

        private static void DeleteOldestArchive()
        {
            while (Directory.GetFiles(Master.backupServerPath).Length > Master.backupConfig.Amount)
            {
                FileSystemInfo fileInfo = new DirectoryInfo(Master.backupServerPath).GetFileSystemInfos().OrderBy(file => file.CreationTime).First();
                Printer.Warning($"Deleting backup {fileInfo.Name} because we've reached the limit of {Master.backupConfig.Amount}", LogImportanceMode.Verbose);
                fileInfo.Delete();
            }
        }

        public static async Task AutoBackup()
        {
            while (true)
            {
                try { BackupServer(); }
                catch (Exception e) { Printer.Error($"Backup tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromHours(Master.backupConfig.IntervalHours));
            }
        }
    }
}
