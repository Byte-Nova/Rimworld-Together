using GameServer.Core;
using GameServer.Files;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Server;

namespace GameServer.Managers
{
    public static class SiteManager
    {
        [HandlesPacket(PacketHeader.SiteManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            if (!Master.ActionConfigs.EnableSites)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            SiteData data = Serializer.ConvertBytesToObject<SiteData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case SiteStepMode.Build:
                    AddNewSite(client, data);
                    break;

                case SiteStepMode.Destroy:
                    DestroySite(client, data);
                    break;

                case SiteStepMode.Info:
                    SiteManagerHelper.GetSiteInfo(client, data);
                    break;

                case SiteStepMode.Config:
                    ChangeUserSiteConfig(client, data);
                    break;

            }
        }

        public static void ConfirmNewSite(ServerClient client, SiteFile siteFile)
        {
            SiteManagerHelper.SaveSite(siteFile);

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._file = siteFile;

            foreach (ServerClient cClient in ServerNetwork.Instance.GetConnectedClientsSafe())
            {
                siteData._file.Goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile);
                cClient.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);
            }

            siteData._stepMode = SiteStepMode.Accept;
            client.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);

            InformationDisplayer.DisplayAddSite(siteFile.Tile.ToString());
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteData._file.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._file.Tile}, but that tile already has a settlement");
            else if (SiteManagerHelper.CheckIfTileIsInUse(siteData._file.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._file.Tile}, but that tile already has a site");
            else
            {
                SiteFile siteFile = new SiteFile();

                siteFile.Tile = siteData._file.Tile;
                siteFile.UID = client.UserFile.Uid;
                siteFile.Type = SiteManagerHelper.GetTypeFromDef(siteData._file.Type.DefName);
                if (!string.IsNullOrEmpty(client.UserFile.GuildName)) siteFile.GuildName = client.UserFile.GuildName;
                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._file.Tile);
            if (siteFile.UID == client.UserFile.Uid) DestroySiteFromFile(siteFile);
            else return;
        }

        private static void VisitSite(ServerClient client, SiteData siteData)
        {
            if (MapManager.CheckIfMapExists(siteData._file.Tile)) siteData._siteMap = MapManager.GetMapFromTile(siteData._file.Tile);
            else siteData._siteMap = null;

            client.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);
        }

        private static void RaidSite(ServerClient client, SiteData siteData)
        {
            if (MapManager.CheckIfMapExists(siteData._file.Tile)) siteData._siteMap = MapManager.GetMapFromTile(siteData._file.Tile);
            else siteData._siteMap = null;

            client.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Destroy;
            siteData._file = siteFile;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.SiteManager, siteData);

            File.Delete(Path.Combine(Master.SitesPath, siteFile.Tile + SiteManagerHelper.fileExtension));

            InformationDisplayer.DisplayRemoveSite(siteFile.Tile.ToString());
        }

        public static void StartSiteTicker()
        {
            while (true)
            {
                try { SiteRewardTick(); }
                catch (Exception e) { Printer.Error($"Site tick failed, this should never happen. Exception > {e}"); }

                Thread.Sleep(TimeSpan.FromMinutes(Master.SiteValues.TimeIntervalMinutes));
            }
        }

        public static void SiteRewardTick()
        {
            SiteFile[] sites = SiteManagerHelper.GetAllSites();

            foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe())
            {
                List<SiteRewardFile> rewards = new List<SiteRewardFile>();

                // Get player specific sites
                List<SiteFile> sitesToAdd = new List<SiteFile>();
                if (string.IsNullOrEmpty(client.UserFile.GuildName)) sitesToAdd = sites.ToList().FindAll(fetch => fetch.UID == client.UserFile.Uid);
                else sitesToAdd.AddRange(sites.ToList().FindAll(fetch => fetch.GuildName == client.UserFile.GuildName));

                foreach (SiteFile site in sitesToAdd)
                {
                    SiteRewardFile rewardFile = new SiteRewardFile();
                    foreach (SiteRewardFile reward in site.Type.Rewards)
                    {
                        if (client.UserFile.SiteConfigs.Any(S => S.RewardDefName == reward.RewardDef))
                        {
                            rewardFile.RewardDef = reward.RewardDef;
                            rewardFile.RewardAmount = reward.RewardAmount;
                        }
                    }

                    if (rewardFile.RewardDef == "") rewardFile = site.Type.Rewards.First();

                    rewards.Add(rewardFile);
                }

                if (rewards.Count == 0) continue;
                else
                {
                    SiteData siteData = new SiteData();
                    siteData._stepMode = SiteStepMode.Rewards;
                    siteData._rewardFiles = rewards.ToArray();

                    client.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);
                }
            }

            InformationDisplayer.DisplaySiteTick();
        }

        public static void UpdateAllSiteInfo()
        {
            foreach (SiteFile site in SiteManagerHelper.GetAllSites())
            {
                foreach (SiteInfoFile config in Master.SiteValues.SiteInfoFiles)
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
                    if (!Master.SiteValues.SiteInfoFiles.Any(site => site.Rewards.Any(reward => reward.RewardDef == config.RewardDefName)))
                    {
                        Printer.Warning($"{file.Uid}'s config was outdated for site {config.DefName}. Updating to new default config.", LogImportanceMode.Verbose);
                        config.RewardDefName = Master.SiteValues.SiteInfoFiles.Where(S => S.DefName == config.DefName).First().Rewards.First().RewardDef;
                        UserManagerH.SaveUserFile(file);
                    }
                }
            }
        }

        public static void ChangeUserSiteConfig(ServerClient client, SiteData data)
        {
            SiteRewardConfigData config = data._rewardConfig;
            SiteConfigFile toModify = client.UserFile.SiteConfigs.First(fetch => fetch.DefName == config._siteDef);
            toModify.RewardDefName = config._rewardDef;

            UserManagerH.SaveUserFile(client.UserFile);
        }

        public static void SetSiteInfoForClient(ServerClient client)
        {
            if (client.UserFile.SiteConfigs.Length > 0) return;
            else
            {
                List<SiteConfigFile> configFiles = new List<SiteConfigFile>();
                for (int i = 0; i < Master.SiteValues.SiteInfoFiles.Length; i++)
                {
                    SiteConfigFile toAdd = new SiteConfigFile();
                    toAdd.DefName = Master.SiteValues.SiteInfoFiles[i].DefName;
                    toAdd.RewardDefName = Master.SiteValues.SiteInfoFiles[i].Rewards.First().RewardDef;

                    configFiles.Add(toAdd);
                }

                client.UserFile.SiteConfigs = configFiles.ToArray();

                UserManagerH.SaveUserFile(client.UserFile);
            }
        }
    }

    public static class SiteManagerHelper
    {
        public readonly static string fileExtension = ".mpsite";

        public static void SaveSite(SiteFile siteFile)
        {
            siteFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.SitesPath, siteFile.Tile + fileExtension), siteFile); }
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

            string[] sites = Directory.GetFiles(Master.SitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.UID == uid) sitesList.Add(siteFile);
            }

            return sitesList.ToArray();
        }

        public static SiteFile GetSiteFileFromTile(int tileToGet)
        {
            string[] sites = Directory.GetFiles(Master.SitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Tile == tileToGet) return siteFile;
            }

            return null;
        }

        public static void GetSiteInfo(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData._file.Tile);
            siteData._file = siteFile;

            client.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);
        }

        public static SiteFile[] GetAllSites()
        {
            List<SiteFile> sitesList = new List<SiteFile>();
            try
            {
                string[] sites = Directory.GetFiles(Master.SitesPath);
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
            string[] sites = Directory.GetFiles(Master.SitesPath);
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
            SiteInfoFile site = Master.SiteValues.SiteInfoFiles.Where(S => S.DefName == defName).FirstOrDefault();
            if (site != null) return site;
            return null;
        }

        public static void SetSitePresets(SiteValuesFile file)
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

            file.SiteInfoFiles = siteInfoFiles.ToArray();
        }
    }
}
