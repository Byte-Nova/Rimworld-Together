using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SiteManager
    {
        //Variables

        private static readonly double taskDelayMS = 1800000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableSites)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            SiteData siteData = Serializer.ConvertBytesToObject<SiteData>(packet.contents);
            switch (siteData._stepMode)
            {
                case SiteStepMode.Build:
                    AddNewSite(client, siteData);
                    break;

                case SiteStepMode.Destroy:
                    DestroySite(client, siteData);
                    break;

                case SiteStepMode.Info:
                    SiteManagerHelper.GetSiteInfo(client, siteData);
                    break;
                    
                case SiteStepMode.Config:
                    ChangeUserSiteConfig(client, siteData);
                    break;

            }
        }

        public static void ConfirmNewSite(ServerClient client, SiteIdendityFile siteFile)
        {
            SiteManagerHelper.SaveSite(siteFile);

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._siteFile = siteFile;

            foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
            {
                siteData._siteFile.Goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile);
                Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);

                cClient.listener.EnqueuePacket(packet);
            }

            siteData._stepMode = SiteStepMode.Accept;
            Packet rPacket = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
            client.listener.EnqueuePacket(rPacket);

            Logger.Warning($"[Created site] > {client.userFile.Username}");
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (PlayerSettlementManager.CheckIfTileIsInUse(siteData._siteFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._siteFile.Tile}, but that tile already has a settlement");
            else if (SiteManagerHelper.CheckIfTileIsInUse(siteData._siteFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._siteFile.Tile}, but that tile already has a site");
            else
            {
                SiteIdendityFile siteFile = new SiteIdendityFile();

                siteFile.Tile = siteData._siteFile.Tile;
                siteFile.Owner = client.userFile.Username;
                siteFile.Type = SiteManagerHelper.GetTypeFromDef(siteData._siteFile.Type.DefName);
                if (client.userFile.FactionFile != null) siteFile.FactionFile = client.userFile.FactionFile;
                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteIdendityFile siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._siteFile.Tile);

            if (siteFile.Owner == client.userFile.Username) DestroySiteFromFile(siteFile);
            else
            {
                ResponseShortcutManager.SendIllegalPacket(client, 
                    $"The site at tile {siteData._siteFile.Tile} was attempted to be destroyed by {client.userFile.Username}, but {siteFile.Owner} owns it");
            }
        }

        public static void DestroySiteFromFile(SiteIdendityFile siteFile)
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Destroy;
            siteData._siteFile = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
            NetworkHelper.SendPacketToAllClients(packet);

            File.Delete(Path.Combine(Master.sitesPath, siteFile.Tile + SiteManagerHelper.fileExtension));
            Logger.Warning($"[Remove site] > {siteFile.Tile}");
        }

        public static async Task StartSiteTicker()
        {
            while (true)
            {
                try { SiteRewardTick(); }
                catch (Exception e) { Logger.Error($"Site tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMinutes(Master.siteValues.TimeIntervalMinutes));
            }
        }

        public static void SiteRewardTick()
        {
            SiteIdendityFile[] sites = SiteManagerHelper.GetAllSites();

            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                List<SiteRewardFile> data = new List<SiteRewardFile>();

                //Get player specific sites
                List<SiteIdendityFile> sitesToAdd = new List<SiteIdendityFile>();
                if (client.userFile.FactionFile == null) sitesToAdd = sites.ToList().FindAll(fetch => fetch.Owner == client.userFile.Username);
                else sitesToAdd.AddRange(sites.ToList().FindAll(fetch => fetch.FactionFile != null && fetch.FactionFile.Name == client.userFile.FactionFile.Name));
                foreach (SiteIdendityFile site in sitesToAdd)
                {
                    SiteRewardFile rewardFile = new SiteRewardFile();
                    foreach (SiteRewardFile reward in site.Type.Rewards)
                    {
                        if (client.userFile.SiteConfigs.Any(S => S.RewardDefName == reward.RewardDef))
                        {
                            rewardFile.RewardDef = reward.RewardDef;
                            rewardFile.RewardAmount = reward.RewardAmount;
                        }
                    }

                    if (rewardFile.RewardDef == "") rewardFile = site.Type.Rewards.First();

                    data.Add(rewardFile);
                }

                Packet packet = Packet.CreatePacketFromObject(nameof(SiteRewardManager), new RewardData() { _rewardData = data.ToArray() });
                client.listener.EnqueuePacket(packet);
            }

            Logger.Warning($"[Site tick]");
        }

        public static void UpdateAllSiteInfo()
        {
            foreach (SiteIdendityFile site in SiteManagerHelper.GetAllSites())
            {
                foreach (SiteInfoFile config in Master.siteValues.SiteInfoFiles)
                {
                    if (config.DefName == site.Type.DefName)
                    {
                        site.Type = config.Clone();
                        SiteManagerHelper.SaveSite(site);
                    }
                }
            }

            foreach (UserFile file in UserManagerHelper.GetAllUserFiles())
            {
                foreach (SiteConfigFile config in file.SiteConfigs)
                {
                    if (!Master.siteValues.SiteInfoFiles.Any(site => site.Rewards.Any(reward => reward.RewardDef == config.RewardDefName)))
                    {
                        Logger.Warning($"{file.Username}'s config was outdated for site {config.DefName}. Updating to new default config.", LogImportanceMode.Verbose);
                        config.RewardDefName = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == config.DefName).First().Rewards.First().RewardDef;
                        UserManagerHelper.SaveUserFile(file);
                    }
                }
            }
        }

        public static void ChangeUserSiteConfig(ServerClient client, SiteData data)
        {
            SiteRewardConfigData config = data._siteConfigFile;
            SiteConfigFile toModify = client.userFile.SiteConfigs.First(fetch => fetch.DefName == config._siteDef);
            toModify.RewardDefName = config._rewardDef;

            UserManagerHelper.SaveUserFile(client.userFile);
        }

        public static void SetSiteInfoForClient(ServerClient client)
        {
            if (client.userFile.SiteConfigs.Length > 0) return;
            else
            {
                List<SiteConfigFile> configFiles = new List<SiteConfigFile>();
                for (int i = 0; i < Master.siteValues.SiteInfoFiles.Length; i++)
                {
                    SiteConfigFile toAdd = new SiteConfigFile();
                    toAdd.DefName = Master.siteValues.SiteInfoFiles[i].DefName;
                    toAdd.RewardDefName = Master.siteValues.SiteInfoFiles[i].Rewards.First().RewardDef;

                    configFiles.Add(toAdd);
                }

                client.userFile.SiteConfigs = configFiles.ToArray();

                UserManagerHelper.SaveUserFile(client.userFile);
            }
        }
    }

    public static class SiteManagerHelper
    {
        public readonly static string fileExtension = ".mpsite";

        public static void SaveSite(SiteIdendityFile siteFile)
        {
            siteFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.sitesPath, siteFile.Tile + fileExtension), siteFile); }
            catch (Exception e) { Logger.Error(e.ToString()); }
            
            siteFile.SavingSemaphore.Release();
        }

        public static void UpdateFaction(SiteIdendityFile siteFile, FactionFile toUpdateWith)
        {
            siteFile.FactionFile = toUpdateWith;
            SaveSite(siteFile);
        }

        public static SiteIdendityFile[] GetAllSitesFromUsername(string username)
        {
            List<SiteIdendityFile> sitesList = new List<SiteIdendityFile>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteIdendityFile siteFile = Serializer.SerializeFromFile<SiteIdendityFile>(site);
                if (siteFile.Owner == username) sitesList.Add(siteFile);
            }

            return sitesList.ToArray();
        }

        public static SiteIdendityFile GetSiteFileFromTile(int tileToGet)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteIdendityFile siteFile = Serializer.SerializeFromFile<SiteIdendityFile>(site);
                if (siteFile.Tile == tileToGet) return siteFile;
            }

            return null;
        }

        public static void GetSiteInfo(ServerClient client, SiteData siteData)
        {
            SiteIdendityFile siteFile = GetSiteFileFromTile(siteData._siteFile.Tile);
            siteData._siteFile = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
            client.listener.EnqueuePacket(packet);
        }

        public static SiteIdendityFile[] GetAllSites()
        {
            List<SiteIdendityFile> sitesList = new List<SiteIdendityFile>();
            try
            {
                string[] sites = Directory.GetFiles(Master.sitesPath);
                foreach (string site in sites)
                {
                    if (!site.EndsWith(fileExtension)) continue;
                    sitesList.Add(Serializer.SerializeFromFile<SiteIdendityFile>(site));
                }
            } catch(Exception ex) { Logger.Error($"Sites could not be loaded, either your formatting is wrong in the file 'SiteValues.json' or you have not updated your sites to the newest version ('Update' command).\n\n{ex.ToString()}"); }
            return sitesList.ToArray();
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteIdendityFile siteFile = Serializer.SerializeFromFile<SiteIdendityFile>(site);
                if (siteFile.Tile == tileToCheck) return true;
            }

            return false;
        }

        public static SiteInfoFile GetTypeFromDef(string defName) 
        {
            SiteInfoFile site = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == defName).FirstOrDefault();
            if(site != null) return site;
            return null;
        }

        public static void SetSitePresets()
        {
            List<SiteInfoFile> siteInfoFiles = new List<SiteInfoFile>();

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTFarmland",
                DefNameCost = ["Silver"],
                Cost = [500],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "RawRice",
                        RewardAmount = 50
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "RawCorn",
                        RewardAmount = 50
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "SmokeleafLeaves",
                        RewardAmount = 25
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "PsychoidLeaves",
                        RewardAmount = 25
                    }
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTHunterCamp",
                DefNameCost = ["Silver"],
                Cost = [500],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "Meat_Muffalo",
                        RewardAmount = 125
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "Meat_Human",
                        RewardAmount = 125
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "Leather_Chinchilla",
                        RewardAmount = 60
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "Leather_Bear",
                        RewardAmount = 60
                    },
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTQuarry",
                DefNameCost = ["Silver"],
                Cost = [500],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "BlocksGranite",
                        RewardAmount = 50
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "BlocksMarble",
                        RewardAmount = 50
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "Steel",
                        RewardAmount = 30
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "Plasteel",
                        RewardAmount = 10
                    }
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTSawmill",
                DefNameCost = ["Silver"],
                Cost = [300],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "WoodLog",
                        RewardAmount = 100
                    }
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTBank",
                DefNameCost = ["Silver"],
                Cost = [750],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "Silver",
                        RewardAmount = 50
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "Gold",
                        RewardAmount = 15
                    }
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTLaboratory",
                DefNameCost = ["Silver"],
                Cost = [750],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "ComponentIndustrial",
                        RewardAmount = 10
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "ComponentSpacer",
                        RewardAmount = 2
                    },
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTRefinery",
                DefNameCost = ["Silver"],
                Cost = [750],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "Chemfuel",
                        RewardAmount = 50
                    }
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTHerbalWorkshop",
                DefNameCost = ["Silver"],
                Cost = [750],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "MedicineHerbal",
                        RewardAmount = 10
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "MedicineIndustrial",
                        RewardAmount = 2
                    }
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTTextileFactory",
                DefNameCost = ["Silver"],
                Cost = [750],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "Cloth",
                        RewardAmount = 50
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "DevilstrandCloth",
                        RewardAmount = 30
                    }
                ]
            });

            siteInfoFiles.Add(new SiteInfoFile()
            {
                DefName = "RTFoodProcessor",
                DefNameCost = ["Silver"],
                Cost = [750],
                Rewards =
                [
                    new SiteRewardFile()
                    {
                        RewardDef = "MealSurvivalPack",
                        RewardAmount = 10
                    },
                    new SiteRewardFile()
                    {
                        RewardDef = "MealNutrientPaste",
                        RewardAmount = 30
                    }
                ]
            });

            Master.siteValues.SiteInfoFiles = siteInfoFiles.ToArray();
        }
    }
}
