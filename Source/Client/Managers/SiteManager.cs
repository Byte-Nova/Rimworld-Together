using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.TCP;
using GameClient.Values;
using GameClient.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;


namespace GameClient.Managers
{
    [RTManager]
    public static class SiteManager
    {
        public static SitePartDef[] siteDefs;

        public static SiteValuesFile siteValues;

        public static List<Site> playerSites = new List<Site>();

        public static void ParsePacket(Packet packet)
        {
            SiteData siteData = Serializer.ConvertBytesToObject<SiteData>(packet.contents);

            switch (siteData._stepMode)
            {
                case SiteStepMode.Accept:
                    OnSiteAccept();
                    break;

                case SiteStepMode.Build:
                    SpawnSingleSite(siteData._file);
                    break;

                case SiteStepMode.Destroy:
                    RemoveSingleSite(siteData._file);
                    break;

                case SiteStepMode.Rewards:
                    ReceiveSiteRewards(siteData._rewardFiles);
                    break;
            }
        }

        public static void SetSiteDefs()
        {
            siteDefs = new SitePartDef[]
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

        private static void OnSiteAccept()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK("The desired site has been built!"));

            SaveManager.ForceSave();
        }

        public static void RequestDestroySite()
        {
            Action r1 = delegate
            {
                SiteData siteData = new SiteData();
                siteData._file.Tile = SessionValues.chosenSite.Tile;
                siteData._stepMode = SiteStepMode.Destroy;

                Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to destroy this site?", r1, null);
            DialogManager.PushNewDialog(d1);
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
            Site[] sites = Find.WorldObjects.Sites.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction) ||
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
                    SitePartDef siteDef = siteDefs.First(fetch => fetch.defName == toAdd.Type.DefName);
                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: toAdd.Tile,
                        threatPoints: 1000,
                        faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.Goodwill));

                    playerSites.Add(site);
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
                    if (playerSites.Contains(toGet)) playerSites.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                else Printer.Warning($"Ignored removal of site at {toGet.Tile} because player was inside");
            }
            catch (Exception e) { Printer.Error($"Failed to remove site at {toRemove.Tile}. Reason: {e}"); }
        }

        public static void RequestSiteBuild(SiteInfoFile configFile)
        {
            for (int i = 0; i < configFile.DefNameCost.Length; i++)
            {
                if (!RimworldManager.CheckIfHasEnoughItemInCaravan(SessionValues.chosenCaravan, configFile.DefNameCost[i], configFile.Cost[i]))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
                    return;
                }
            }

            for (int i = 0; i < configFile.DefNameCost.Length; i++)
            {
                RimworldManager.RemoveThingFromCaravan(SessionValues.chosenCaravan,
                    DefDatabase<ThingDef>.GetNamed(configFile.DefNameCost[i]), configFile.Cost[i]);
            }

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._file.Tile = SessionValues.chosenCaravan.Tile;
            siteData._file.Type.DefName = configFile.DefName;

            Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), siteData);
            Network.listener.EnqueuePacket(packet);

            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for building"));
        }

        public static void RequestSiteChangeConfig(SiteInfoFile config, string reward)
        {
            SiteRewardConfigData rewardConfig = new SiteRewardConfigData();
            rewardConfig._siteDef = config.DefName;
            rewardConfig._rewardDef = reward;

            SiteData packetData = new SiteData();
            packetData._stepMode = SiteStepMode.Config;
            packetData._rewardConfig = rewardConfig;

            Packet packet = Packet.CreatePacketFromObject(nameof(SiteManager), packetData);
            Network.listener.EnqueuePacket(packet);
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
    }
}

public static class SiteManagerHelper
{
    public static SiteFile[] tempSites;

    public static void SetValues(ServerGlobalData serverGlobalData)
    {
        SiteManager.siteValues = serverGlobalData._siteValues;
        tempSites = serverGlobalData._playerSites;
    }
}


