using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using GameClient.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;


namespace GameClient.Managers
{
    public static class SiteManager
    {
        public static SitePartDef[] SiteDefs { get; set; }

        public static SiteValuesFile SiteValues { get; set; }

        public static List<Site> PlayerSites { get; set; } = new List<Site>();

        [HandlesPacket(PacketHeader.SiteManager)]
        private static void ParsePacket(byte[] bytes)
        {
            SiteData data = Serializer.ConvertBytesToObject<SiteData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case SiteStepMode.Accept:
                    OnSiteAccept();
                    break;

                case SiteStepMode.Deny:
                    OnSiteDeny();
                    break;

                case SiteStepMode.Build:
                    SpawnSingleSite(data._file);
                    break;

                case SiteStepMode.Destroy:
                    RemoveSingleSite(data._file);
                    break;

                case SiteStepMode.Rewards:
                    ReceiveSiteRewards(data._rewardFiles);
                    break;
            }
        }

        public static void RequestDestroySite()
        {
            Action r1 = delegate
            {
                SiteData siteData = new SiteData();
                siteData._file.Tile = SessionValues.ChosenSite.Tile;
                siteData._stepMode = SiteStepMode.Destroy;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SiteManager, siteData);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to destroy this site?", r1, null);
            RT_Dialog_Base.PushNewDialog(d1);
        }

        public static void RequestSiteBuild(SiteInfoFile configFile)
        {
            for (int i = 0; i < configFile.DefNameCost.Length; i++)
            {
                if (!RimworldManager.CheckIfHasEnoughItemInCaravan(SessionValues.ChosenCaravan, configFile.DefNameCost[i], configFile.Cost[i]))
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You do not have enough silver!" }));
                    return;
                }
            }

            for (int i = 0; i < configFile.DefNameCost.Length; i++)
            {
                RimworldManager.RemoveThingFromCaravan(SessionValues.ChosenCaravan,
                    DefDatabase<ThingDef>.GetNamed(configFile.DefNameCost[i]), configFile.Cost[i]);
            }

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._file.Tile = SessionValues.ChosenCaravan.Tile;
            siteData._file.Type.DefName = configFile.DefName;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SiteManager, siteData);

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for building"));
        }

        public static void RequestSiteChangeConfig(SiteInfoFile config, string reward)
        {
            SiteRewardConfigData rewardConfig = new SiteRewardConfigData();
            rewardConfig._siteDef = config.DefName;
            rewardConfig._rewardDef = reward;

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Config;
            siteData._rewardConfig = rewardConfig;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SiteManager, siteData);
        }

        private static void ReceiveSiteRewards(SiteRewardFile[] files)
        {
            List<Thing> rewards = new List<Thing>();
            foreach (SiteRewardFile reward in files)
            {
                try
                {
                    ThingDef def = DefDatabase<ThingDef>.AllDefs.First(fetch => fetch.defName == reward.RewardDef);
                    Thing toMake = ThingMaker.MakeThing(def);
                    toMake.stackCount = reward.RewardAmount;
                    toMake.HitPoints = def.BaseMaxHitPoints;
                    rewards.Add(toMake);

                    Printer.Message($"Received {reward.RewardAmount} of {reward.RewardDef}", LogImportanceMode.Verbose);
                }
                catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
            }

            if (rewards.Count > 0)
            {
                TransferManager.GetTransferedItemsToSettlement(rewards.ToArray(), true, false, false);
                RimworldManager.GenerateLetter("Site rewards", $"You've received your site rewards", LetterDefOf.PositiveEvent);
                Printer.Message("Rewards delivered", LogImportanceMode.Verbose);
            }
        }

        public static void AddSites(SiteFile[] sites)
        {
            foreach (SiteFile toAdd in sites)
            {
                SpawnSingleSite(toAdd);
            }
        }

        public static void ClearAllSites()
        {
            PlayerSites.Clear();

            Site[] sites = Find.WorldObjects.Sites.Where(fetch => ClientValues.PlayerFactions.Contains(fetch.Faction) ||
                fetch.Faction == Faction.OfPlayer).ToArray();

            foreach (Site toRemove in sites)
            {
                SiteFile siteFile = new SiteFile();
                siteFile.Tile = toRemove.Tile;
                RemoveSingleSite(siteFile);
            }
        }

        public static void SpawnSingleSite(SiteFile toAdd)
        {
            if (Find.WorldObjects.Sites.FirstOrDefault(fetch => fetch.Tile == toAdd.Tile) != null) return;
            else
            {
                try
                {
                    SitePartDef siteDef = SiteDefs.First(fetch => fetch.defName == toAdd.Type.DefName);
                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: toAdd.Tile,
                        threatPoints: 1000,
                        faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.Goodwill));

                    PlayerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }
                catch (Exception e) { Printer.Error($"Failed to spawn site at {toAdd.Tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSite(SiteFile toRemove)
        {
            try
            {
                Site toGet = Find.WorldObjects.Sites.Find(fetch => fetch.Tile == toRemove.Tile);
                if (!RimworldManager.CheckIfMapHasPlayerPawns(toGet.Map))
                {
                    if (PlayerSites.Contains(toGet)) PlayerSites.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                else Printer.Warning($"Ignored removal of site at {toGet.Tile} because player was inside");
            }
            catch (Exception e) { Printer.Error($"Failed to remove site at {toRemove.Tile}. Reason: {e}"); }
        }

        private static void OnSiteAccept()
        {
            RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "The desired site has been built!" }));

            SaveManager.ForceSave();
        }

        private static void OnSiteDeny()
        {
            RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "The current action is not available!" }));
        }
    }
}

public static class SiteManagerH
{
    public static SiteFile[] tempSites;

    public static void SetValues(ServerGlobalData serverGlobalData)
    {
        SiteManager.SiteValues = serverGlobalData._siteValues;
        tempSites = serverGlobalData._playerSites;
    }

    public static void SetSiteDefs()
    {
        SiteManager.SiteDefs = new SitePartDef[]
        {
            RTSitePartDefOf.RTFarmland,
            RTSitePartDefOf.RTHunterCamp,
            RTSitePartDefOf.RTQuarry,
            RTSitePartDefOf.RTSawmill,
            RTSitePartDefOf.RTBank,
            RTSitePartDefOf.RTLaboratory,
            RTSitePartDefOf.RTRefinery,
            RTSitePartDefOf.RTHerbalWorkshop,
            RTSitePartDefOf.RTTextileFactory,
            RTSitePartDefOf.RTFoodProcessor
        };
    }
}


