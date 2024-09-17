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
                    SitePartDef siteDef = SiteManager.GetDefForNewSite(toAdd.Type, toAdd.FactionFile != null);
                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: toAdd.Tile,
                        threatPoints: 1000,
                        faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.Goodwill));

                    playerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn site at {toAdd.Tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSite(SiteFile toRemove)
        {
            try
            {
                Site toGet = Find.WorldObjects.Sites.Find(fetch => fetch.Tile == toRemove.Tile && FactionValues.playerFactions.Contains(fetch.Faction));
                if (!RimworldManager.CheckIfMapHasPlayerPawns(toGet.Map))
                {
                    if (playerSites.Contains(toGet)) playerSites.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                else Logger.Warning($"Ignored removal of site at {toGet.Tile} because player was inside");
            }
            catch (Exception e) { Logger.Error($"Failed to remove site at {toRemove.Tile}. Reason: {e}"); }
        }
    }

    public static class PlayerSiteManagerHelper
    {
        public static SiteFile[] tempSites;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempSites = serverGlobalData._playerSites;
        }
    }
}
