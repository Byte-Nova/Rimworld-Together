using RimWorld.Planet;
using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GameClient
{
    public static class PlayerSiteManager
    {
        public static List<Site> playerSites = new List<Site>();

        public static void AddSites(OnlineSiteFile[] sites)
        {
            if (sites == null) return;

            for (int i = 0; i < PlayerSiteManagerHelper.tempSites.Count(); i++)
            {
                OnlineSiteFile siteFile = PlayerSiteManagerHelper.tempSites[i];

                try
                {
                    SitePartDef siteDef = SiteManager.GetDefForNewSite(siteFile.type, siteFile.fromFaction);
                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: siteFile.tile,
                        threatPoints: 1000,
                        faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(siteFile.goodwill));

                    playerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn site at {siteFile.tile}. Reason: {e}"); }
            }
        }

        public static void ClearAllSites()
        {
            playerSites.Clear();

            Site[] sites = Find.WorldObjects.Sites.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction)).ToArray();
            foreach (Site site in sites) Find.WorldObjects.Remove(site);

            sites = Find.WorldObjects.Sites.Where(fetch => fetch.Faction == Faction.OfPlayer).ToArray();
            foreach (Site site in sites) Find.WorldObjects.Remove(site);
        }

        public static void SpawnSingleSite(SiteData siteData)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    SitePartDef siteDef = SiteManager.GetDefForNewSite(siteData.siteFile.type, siteData.siteFile.factionFile != null);
                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: siteData.siteFile.tile,
                        threatPoints: 1000,
                        faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(siteData.goodwill));

                    playerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn site at {siteData.siteFile.tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSite(SiteData siteData)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Site toGet = playerSites.Find(x => x.Tile == siteData.siteFile.tile);

                    playerSites.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Logger.Message($"Failed to remove site at {siteData.siteFile.tile}. Reason: {e}"); }
            }
        }
    }

    public static class PlayerSiteManagerHelper
    {
        public static OnlineSiteFile[] tempSites;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempSites = serverGlobalData.playerSites;
        }
    }
}
