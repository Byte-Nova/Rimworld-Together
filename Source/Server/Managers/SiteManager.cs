using GameServer.Core;
using GameServer.Files;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class SiteManager
    {
        //Variables

        private static readonly double taskDelayMS = 1800000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionConfigs.EnableSites)
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

        public static void ConfirmNewSite(ServerClient client, SiteFile siteFile)
        {
            SiteManagerHelper.SaveSite(siteFile);

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._file = siteFile;

            foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
            {
                siteData._file.Goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile);
                Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);

                cClient.listener.EnqueuePacket(packet);
            }

            siteData._stepMode = SiteStepMode.Accept;
            Packet rPacket = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
            client.listener.EnqueuePacket(rPacket);

            InformationDisplayer.DisplayAddSite(siteFile.Tile.ToString());
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (PlayerSettlementManager.CheckIfTileIsInUse(siteData._file.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._file.Tile}, but that tile already has a settlement");
            else if (SiteManagerHelper.CheckIfTileIsInUse(siteData._file.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._file.Tile}, but that tile already has a site");
            else
            {
                SiteFile siteFile = new SiteFile();

                siteFile.Tile = siteData._file.Tile;
                siteFile.UID = client.userFile.Uid;
                siteFile.Type = SiteManagerHelper.GetTypeFromDef(siteData._file.Type.DefName);
                if (!string.IsNullOrEmpty(client.userFile.GuildName)) siteFile.GuildName = client.userFile.GuildName;
                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._file.Tile);

            if (siteFile.UID == client.userFile.Uid) DestroySiteFromFile(siteFile);
            else
            {
                ResponseShortcutManager.SendIllegalPacket(client,
                    $"The site at tile {siteData._file.Tile} was attempted to be destroyed by {client.userFile.Uid}, but {siteFile.UID} owns it");
            }
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Destroy;
            siteData._file = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
            NetworkHelper.SendPacketToAllClients(packet);

            File.Delete(Path.Combine(Master.sitesPath, siteFile.Tile + SiteManagerHelper.fileExtension));

            InformationDisplayer.DisplayRemoveSite(siteFile.Tile.ToString());
        }

        public static async Task StartSiteTicker()
        {
            while (true)
            {
                try { SiteRewardTick(); }
                catch (Exception e) { Printer.Error($"Site tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMinutes(Master.siteValues.TimeIntervalMinutes));
            }
        }

        public static void SiteRewardTick()
        {
            SiteFile[] sites = SiteManagerHelper.GetAllSites();

            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                List<SiteRewardFile> rewards = new List<SiteRewardFile>();

                // Get player specific sites
                List<SiteFile> sitesToAdd = new List<SiteFile>();
                if (string.IsNullOrEmpty(client.userFile.GuildName)) sitesToAdd = sites.ToList().FindAll(fetch => fetch.UID == client.userFile.Uid);
                else sitesToAdd.AddRange(sites.ToList().FindAll(fetch => fetch.GuildName == client.userFile.GuildName));
                foreach (SiteFile site in sitesToAdd)
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

                    rewards.Add(rewardFile);
                }

                if (rewards.Count == 0) return;
                else
                {
                    SiteData siteData = new SiteData();
                    siteData._stepMode = SiteStepMode.Rewards;
                    siteData._rewardFiles = rewards.ToArray();

                    Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
                    client.listener.EnqueuePacket(packet);
                }
            }

            InformationDisplayer.DisplaySiteTick();
        }

        public static void UpdateAllSiteInfo()
        {
            foreach (SiteFile site in SiteManagerHelper.GetAllSites())
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

            foreach (UserFile file in UserManagerH.GetAllUserFiles())
            {
                foreach (SiteConfigFile config in file.SiteConfigs)
                {
                    if (!Master.siteValues.SiteInfoFiles.Any(site => site.Rewards.Any(reward => reward.RewardDef == config.RewardDefName)))
                    {
                        Printer.Warning($"{file.Uid}'s config was outdated for site {config.DefName}. Updating to new default config.", LogImportanceMode.Verbose);
                        config.RewardDefName = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == config.DefName).First().Rewards.First().RewardDef;
                        UserManagerH.SaveUserFile(file);
                    }
                }
            }
        }

        public static void ChangeUserSiteConfig(ServerClient client, SiteData data)
        {
            SiteRewardConfigData config = data._rewardConfig;
            SiteConfigFile toModify = client.userFile.SiteConfigs.First(fetch => fetch.DefName == config._siteDef);
            toModify.RewardDefName = config._rewardDef;

            UserManagerH.SaveUserFile(client.userFile);
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

                UserManagerH.SaveUserFile(client.userFile);
            }
        }
    }

    public static class SiteManagerHelper
    {
        public readonly static string fileExtension = ".mpsite";

        public static void SaveSite(SiteFile siteFile)
        {
            siteFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.sitesPath, siteFile.Tile + fileExtension), siteFile); }
            catch (Exception e) { Printer.Error(e.ToString()); }

            siteFile.SavingSemaphore.Release();
        }

        public static void UpdateFaction(SiteFile siteFile, GuildFile toUpdateWith)
        {
            if (toUpdateWith == null) siteFile.GuildName = null;
            else siteFile.GuildName = toUpdateWith.Name;
            SaveSite(siteFile);
        }

        public static SiteFile[] GetAllSitesFromUID(string uid)
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.UID == uid) sitesList.Add(siteFile);
            }

            return sitesList.ToArray();
        }

        public static SiteFile GetSiteFileFromTile(int tileToGet)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Tile == tileToGet) return siteFile;
            }

            return null;
        }

        public static void GetSiteInfo(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData._file.Tile);
            siteData._file = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
            client.listener.EnqueuePacket(packet);
        }

        public static SiteFile[] GetAllSites()
        {
            List<SiteFile> sitesList = new List<SiteFile>();
            try
            {
                string[] sites = Directory.GetFiles(Master.sitesPath);
                foreach (string site in sites)
                {
                    if (!site.EndsWith(fileExtension)) continue;
                    sitesList.Add(Serializer.SerializeFromFile<SiteFile>(site));
                }
            }
            catch (Exception ex) { Printer.Error($"Sites could not be loaded, either your formatting is wrong in the file 'SiteConfig.json' or you have not updated your sites to the newest version ('Update' command).\n\n{ex.ToString()}"); }
            return sitesList.ToArray();
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Tile == tileToCheck) return true;
            }

            return false;
        }

        public static SiteInfoFile GetTypeFromDef(string defName)
        {
            SiteInfoFile site = Master.siteValues.SiteInfoFiles.Where(S => S.DefName == defName).FirstOrDefault();
            if (site != null) return site;
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
