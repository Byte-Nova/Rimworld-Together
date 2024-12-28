using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Updater
{
    public static class UpdateManager
    {
        public static void UpdateServer() 
        {
            BackupManager.BackupServer();
            UpdateSites();
        }

        private static void UpdateSites() 
        {
            string pathToDelete = Path.Combine(Master.corePath, "SiteValues.json");
            if (File.Exists(pathToDelete)) File.Delete(pathToDelete);

            foreach (string file in Directory.GetFiles(Master.sitesPath))
            {
                if (File.Exists(file))
                {
                    SiteFile site = Serializer.SerializeFromFile<SiteFile>(file);
                    SiteIdendityFile newSite = new SiteIdendityFile();
                    newSite.Goodwill = site.Goodwill;
                    newSite.FactionFile = UserManagerHelper.GetUserFileFromName(site.Owner).FactionFile;
                    newSite.Owner = site.Owner;
                    newSite.Tile = site.Tile;

                    switch (site.Type)
                    {
                        case 0:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTFarmland").First();
                            break;

                        case 1:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTQuarry").First();
                            break;

                        case 2:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTSawmill").First();
                            break;

                        case 3:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTBank").First();
                            break;

                        case 4:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTLaboratory").First();
                            break;

                        case 5:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTRefinery").First();
                            break;

                        case 6:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTHerbalWorkshop").First();
                            break;

                        case 7:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTTextileFactory").First();
                            break;

                        case 8:
                            newSite.Type = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == "RTFoodProcessor").First();
                            break;
                    }

                    Serializer.SerializeToFile(file, newSite);
                }
            }

            Logger.Title("Please restart the server to finalize the update.");
        }
    }
}
