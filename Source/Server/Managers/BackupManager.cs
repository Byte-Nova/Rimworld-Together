using Discord;
using Shared;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class BackupManager
    {
        public static readonly string fileExtension = ".zip";

        private static readonly Semaphore savingSemaphore = new Semaphore(1,1);

        public static void BackupServer()
        {
            savingSemaphore.WaitOne();

            try
            {
                string backupName = $"Server_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
                string backupPath = $"{Master.backupServerPath + Path.DirectorySeparatorChar}{backupName}{fileExtension}";

                List<string> toArchive = new List<string>();
                toArchive.AddRange(Directory.GetFiles(Master.corePath, "*.*", SearchOption.AllDirectories));
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

                Logger.Warning($"Successfully backed up server under {backupName}{fileExtension}");
            }
            catch (Exception ex) { Logger.Error(ex.ToString()); }

            savingSemaphore.Release();
        }

        public static void BackupUser(string username, bool persistent = false) 
        {
            savingSemaphore.WaitOne();

            try
            {
                string playerArchivedSavePath = Path.Combine(Master.backupUsersPath, username);
                if (persistent) playerArchivedSavePath += " - persistent";
                playerArchivedSavePath += fileExtension;

                if (File.Exists(playerArchivedSavePath))
                {
                    if (persistent == true)
                    {
                        Logger.Error($"Could not backup user {username} because the file {playerArchivedSavePath} already exist. Consider running a non-persistent backup if you want to overwrite it.");
                        savingSemaphore.Release();
                        return;
                    }

                    else
                    {
                        File.Delete(playerArchivedSavePath);
                        Logger.Warning($"Deleting backup of {username} because he already had one.", LogImportanceMode.Verbose);
                    }
                }

                List<string> toArchive = new List<string>();

                string userFilePath = Path.Combine(Master.usersPath, username + UserManagerHelper.fileExtension);
                if (File.Exists(userFilePath)) toArchive.Add(userFilePath);

                string userSavePath = Path.Combine(Master.savesPath, username + SaveManager.fileExtension);
                if (File.Exists(userSavePath)) toArchive.Add(userSavePath);

                SiteIdendityFile[] playerSites = SiteManagerHelper.GetAllSitesFromUsername(username);
                foreach (SiteIdendityFile site in playerSites) toArchive.Add(Path.Combine(Master.sitesPath, site.Tile + SiteManagerHelper.fileExtension));

                SettlementFile[] playerSettlements = PlayerSettlementManager.GetAllSettlementsFromUsername(username);
                foreach (SettlementFile settlementFile in playerSettlements) toArchive.Add(Path.Combine(Master.settlementsPath, settlementFile.Tile + PlayerSettlementManager.fileExtension));

                CaravanFile[] playerCaravans = CaravanManagerHelper.GetCaravansFromOwner(username);
                foreach (CaravanFile caravanFile in playerCaravans) toArchive.Add(Path.Combine(Master.caravansPath, caravanFile.ID + CaravanManager.fileExtension));

                CreateArchive(toArchive, playerArchivedSavePath);
                Logger.Warning($"Successfully backed up user data for {username} under the name {playerArchivedSavePath}.");
            }
            catch (Exception ex) { Logger.Error(ex.ToString()); }

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
                Logger.Warning($"Deleting backup {fileInfo.Name} because we've reached the limit of {Master.backupConfig.Amount}", LogImportanceMode.Verbose);
                fileInfo.Delete();
            }
        }

        public static async Task AutoBackup()
        {
            while (true)
            {
                try { BackupServer(); }
                catch (Exception e) { Logger.Error($"Backup tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromHours(Master.backupConfig.IntervalHours));
            }
        }
    }
}
