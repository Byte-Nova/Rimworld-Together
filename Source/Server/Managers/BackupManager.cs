using Discord;
using Shared;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class BackupManager
    {
        public static readonly string fileExtension = ".zip";
        private static readonly Semaphore inUse = new Semaphore(1,1);
        public static void BackupServer(string backupName = "")
        {
            inUse.WaitOne();
            try
            {
                backupName = $"World-{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
                string backupPath = $"{Master.backupWorldPath + Path.DirectorySeparatorChar}{backupName}{fileExtension}";
                List<string> files = new List<string>();
                files.AddRange(Directory.GetFiles(Master.corePath, "*.*", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(Master.factionsPath, "*.*", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(Master.mapsPath, "*.*", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(Master.savesPath, "*.*", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(Master.settlementsPath, "*.*", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(Master.sitesPath, "*.*", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(Master.usersPath, "*.*", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(Master.caravansPath, "*.*", SearchOption.AllDirectories));
                CreateArchive(files, backupPath, Master.backupWorldPath);
                if (Directory.GetFiles(Master.backupWorldPath).Count() > Master.backupConfig.Amount && Master.backupConfig.AutomaticDeletion == true)
                {
                    DeleteOldestArchive();
                }
                Logger.Warning($"Successfully backed up server under {backupName}{fileExtension}");
                Main_.SetPaths();
            }
            catch (Exception ex) { Logger.Error(ex.ToString()); }
            inUse.Release();
        }

        public static void BackupUser(string username, bool persistent = false) 
        {
            inUse.WaitOne();
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
                        inUse.Release();
                        return;
                    }
                    else
                    {
                        File.Delete(playerArchivedSavePath);
                        if (Master.serverConfig.VerboseLogs) Logger.Warning($"Deleting backup of {username} because he already had one.");
                    }
                }


                List<string> files = new List<string>();

                if(File.Exists(Path.Combine(Master.savesPath, username + SaveManager.fileExtension))) files.Add(Path.Combine(Master.savesPath, username + SaveManager.fileExtension));

                MapData[] userMaps = MapManager.GetAllMapsFromUsername(username);
                foreach (MapData map in userMaps)
                {
                    if(map != null) files.Add(Path.Combine(Master.mapsPath, map._mapTile + MapManager.fileExtension));
                }

                SiteFile[] playerSites = SiteManagerHelper.GetAllSitesFromUsername(username);
                foreach (SiteFile site in playerSites)
                {
                    if(site != null) files.Add(Path.Combine(Master.sitesPath, site.Tile + SiteManagerHelper.fileExtension));
                }
                SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
                foreach (SettlementFile settlementFile in playerSettlements)
                {
                    if(settlementFile != null) files.Add(Path.Combine(Master.settlementsPath, settlementFile.Tile + SettlementManager.fileExtension));
                }

                CreateArchive(files, playerArchivedSavePath, Master.usersPath);
                Logger.Warning($"Successfully backed up user data for {username} under the name {playerArchivedSavePath}.");
            }catch (Exception ex) { Logger.Error(ex.ToString()); }
            inUse.Release();
        }

        private static void CreateArchive(List<string> files, string toPath, string fromPath) 
        {
            using (FileStream zip = new FileStream(toPath, FileMode.CreateNew))
            {
                using (ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Create))
                {
                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            string relativePath = Path.GetRelativePath(Master.mainPath, file);

                            archive.CreateEntryFromFile(file, relativePath);
                        }
                    }
                }
            }
        }

        private static void DeleteOldestArchive() 
        {
            while (Directory.GetFiles(Master.backupWorldPath).Count() > Master.backupConfig.Amount)
            {
                FileSystemInfo fileInfo = new DirectoryInfo(Master.backupWorldPath).GetFileSystemInfos().OrderBy(file => file.CreationTime).First();
                if (Master.serverConfig.VerboseLogs) Logger.Warning($"Deleting backup {fileInfo.Name} because we've reached the limit of {Master.backupConfig.Amount}");
                fileInfo.Delete();
            }
        }

        public static async Task AutoBackup()
        {
            while (true)
            {
                BackupServer();

                await Task.Delay(TimeSpan.FromHours(Master.backupConfig.IntervalHours));
            }
        }
    }
}
