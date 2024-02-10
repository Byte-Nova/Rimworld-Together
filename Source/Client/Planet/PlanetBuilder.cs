using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using Shared.Misc;
using Verse;

namespace RimworldTogether.GameClient.Planet
{
    public static class PlanetBuilder
    {
        public static List<Settlement> playerSettlements = new List<Settlement>();

        public static List<Site> playerSites = new List<Site>();

        public static void BuildPlanet()
        {
            SpawnPlayerSettlements();
            SpawnPlayerSites();
        }

        private static void RemoveOldSettlements()
        {
            playerSettlements.Clear();

            DestroyedSettlement[] destroyedSettlements = Find.WorldObjects.DestroyedSettlements.ToArray();
            foreach (DestroyedSettlement destroyedSettlement in destroyedSettlements)
            {
                Find.WorldObjects.Remove(destroyedSettlement);
            }

            Settlement[] settlements = Find.WorldObjects.Settlements.ToArray();
            foreach (Settlement settlement in settlements)
            {
                if (settlement.Faction == FactionValues.allyPlayer)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }

                if (settlement.Faction == FactionValues.neutralPlayer)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }

                if (settlement.Faction == FactionValues.enemyPlayer)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }

                if (settlement.Faction == FactionValues.yourOnlineFaction)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }
            }
        }

        private static void SpawnPlayerSettlements()
        {
            FactionValues.FindPlayerFactionsInWorld();

            RemoveOldSettlements();

            for (int i = 0; i < PlanetBuilderHelper.tempSettlementTiles.Count(); i++)
            {
                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = int.Parse(PlanetBuilderHelper.tempSettlementTiles[i]);
                    settlement.Name = $"{PlanetBuilderHelper.tempSettlementOwners[i]}'s settlement";
                    settlement.SetFaction(GetPlayerFaction(int.Parse(PlanetBuilderHelper.tempSettlementLikelihoods[i])));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }

                catch (Exception e)
                {
                    Log.Error($"Failed to build settlement at {PlanetBuilderHelper.tempSettlementTiles[i]}. " +
                        $"Reason: {e}");
                }
            }
        }

        private static void RemoveOldSites()
        {
            playerSites.Clear();

            Site[] sites = Find.WorldObjects.Sites.ToArray();
            foreach (Site site in sites)
            {
                if (site.Faction == FactionValues.enemyPlayer)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }

                if (site.Faction == FactionValues.neutralPlayer)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }

                if (site.Faction == FactionValues.allyPlayer)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }

                if (site.Faction == FactionValues.yourOnlineFaction)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }

                if (site.Faction == Faction.OfPlayer)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }
            }
        }

        private static void SpawnPlayerSites()
        {
            RemoveOldSites();

            for (int i = 0; i < PlanetBuilderHelper.tempSiteTiles.Count(); i++)
            {
                try
                {
                    SitePartDef siteDef = SiteManager.GetDefForNewSite(int.Parse(PlanetBuilderHelper.tempSiteTypes[i]),
                        PlanetBuilderHelper.tempSiteIsFromFactions[i]);

                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: int.Parse(PlanetBuilderHelper.tempSiteTiles[i]),
                        threatPoints: 1000,
                        faction: GetPlayerFaction(int.Parse(PlanetBuilderHelper.tempSiteLikelihoods[i])));

                    playerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }

                catch (Exception e)
                {
                    Log.Error($"Failed to spawn site at {PlanetBuilderHelper.tempSiteTiles[i]}. Reason: {e}");
                };
            }
        }

        public static void SpawnSingleSettlement(SettlementDetailsJSON newSettlementJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement newSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    newSettlement.Tile = int.Parse(newSettlementJSON.tile);
                    newSettlement.Name = $"{newSettlementJSON.owner}'s settlement";
                    newSettlement.SetFaction(GetPlayerFaction(int.Parse(newSettlementJSON.value)));

                    playerSettlements.Add(newSettlement);
                    Find.WorldObjects.Add(newSettlement);
                }
                catch (Exception e) { Log.Error($"Failed to spawn settlement at {newSettlementJSON.tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSettlement(SettlementDetailsJSON newSettlementJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement toGet = playerSettlements.Find(x => x.Tile.ToString() == newSettlementJSON.tile);

                    playerSettlements.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Log.Error($"Failed to remove settlement at {newSettlementJSON.tile}. Reason: {e}"); }
            }
        }

        public static void SpawnSingleSite(SiteDetailsJSON siteDetailsJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    SitePartDef siteDef = SiteManager.GetDefForNewSite(int.Parse(siteDetailsJSON.type),
                        siteDetailsJSON.isFromFaction);

                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: int.Parse(siteDetailsJSON.tile),
                        threatPoints: 1000,
                        faction: GetPlayerFaction(int.Parse(siteDetailsJSON.likelihood)));

                    playerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }

                catch (Exception e)
                {
                    Log.Error($"Failed to spawn site at {siteDetailsJSON.tile}. Reason: {e}");
                };
            }
        }

        public static void RemoveSingleSite(SiteDetailsJSON siteDetailsJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Site toGet = playerSites.Find(x => x.Tile.ToString() == siteDetailsJSON.tile);

                    playerSites.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Log.Message($"Failed to remove site at {siteDetailsJSON.tile}. Reason: {e}"); }
            }
        }

        public static Faction GetPlayerFaction(int value)
        {
            Faction factionToUse = null;
            switch (value)
            {
                case (int)CommonEnumerators.Likelihoods.Enemy:
                    factionToUse = FactionValues.enemyPlayer;
                    break;

                case (int)CommonEnumerators.Likelihoods.Neutral:
                    factionToUse = FactionValues.neutralPlayer;
                    break;

                case (int)CommonEnumerators.Likelihoods.Ally:
                    factionToUse = FactionValues.allyPlayer;
                    break;

                case (int)CommonEnumerators.Likelihoods.Faction:
                    factionToUse = FactionValues.yourOnlineFaction;
                    break;

                case (int)CommonEnumerators.Likelihoods.Personal:
                    factionToUse = Faction.OfPlayer;
                    break;
            }

            return factionToUse;
        }
    }
}
