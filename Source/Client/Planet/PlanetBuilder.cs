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

        public static Faction GetPlayerFaction(int value)
        {
            Faction factionToUse = null;
            switch (value)
            {
                case (int)CommonEnumerators.Likelihoods.Enemy:
                    factionToUse = PlanetFactions.enemyPlayer;
                    break;

                case (int)CommonEnumerators.Likelihoods.Neutral:
                    factionToUse = PlanetFactions.neutralPlayer;
                    break;

                case (int)CommonEnumerators.Likelihoods.Ally:
                    factionToUse = PlanetFactions.allyPlayer;
                    break;

                case (int)CommonEnumerators.Likelihoods.Faction:
                    factionToUse = PlanetFactions.yourOnlineFaction;
                    break;

                case (int)CommonEnumerators.Likelihoods.Personal:
                    factionToUse = Faction.OfPlayer;
                    break;
            }

            return factionToUse;
        }

        public static void BuildPlanet()
        {
            SpawnPlayerSettlements();
            SpawnPlayerSites();
        }

        private static void RemoveOldSettlements()
        {
            playerSettlements.Clear();

            DestroyedSettlement[] destroyedSettlements = Find.WorldObjects.DestroyedSettlements.ToArray();
            foreach(DestroyedSettlement destroyedSettlement in destroyedSettlements)
            {
                Find.WorldObjects.Remove(destroyedSettlement);
            }

            Settlement[] settlements = Find.WorldObjects.Settlements.ToArray();
            foreach (Settlement settlement in settlements)
            {
                if (settlement.Faction == PlanetFactions.allyPlayer)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }

                if (settlement.Faction == PlanetFactions.neutralPlayer)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }

                if (settlement.Faction == PlanetFactions.enemyPlayer)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }

                if (settlement.Faction == PlanetFactions.yourOnlineFaction)
                {
                    Find.WorldObjects.Remove(settlement);
                    continue;
                }
            }
        }

        private static void SpawnPlayerSettlements()
        {
            Action toDo = delegate
            {
                PlanetFactions.FindPlayerFactionsInWorld();

                RemoveOldSettlements();

                for (int i = 0; i < PlanetBuilder_Temp.tempSettlementTiles.Count(); i++)
                {
                    try
                    {
                        Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                        settlement.Tile = int.Parse(PlanetBuilder_Temp.tempSettlementTiles[i]);
                        settlement.Name = $"{PlanetBuilder_Temp.tempSettlementOwners[i]}'s settlement";
                        settlement.SetFaction(GetPlayerFaction(int.Parse(PlanetBuilder_Temp.tempSettlementLikelihoods[i])));

                        playerSettlements.Add(settlement);
                        Find.WorldObjects.Add(settlement);
                    }

                    catch (Exception e) 
                    {
                        Log.Error($"Failed to build settlement at {PlanetBuilder_Temp.tempSettlementTiles[i]}. " +
                            $"Reason: {e}");
                    }
                }
            };
            toDo.Invoke();
        }

        private static void RemoveOldSites()
        {
            playerSites.Clear();

            Site[] sites = Find.WorldObjects.Sites.ToArray();
            foreach (Site site in sites)
            {
                if (site.Faction == PlanetFactions.enemyPlayer)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }

                if (site.Faction == PlanetFactions.neutralPlayer)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }

                if (site.Faction == PlanetFactions.allyPlayer)
                {
                    Find.WorldObjects.Remove(site);
                    continue;
                }

                if (site.Faction == PlanetFactions.yourOnlineFaction)
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
            Action toDo = delegate
            {
                RemoveOldSites();

                for (int i = 0; i < PlanetBuilder_Temp.tempSiteTiles.Count(); i++)
                {
                    try
                    {
                        SitePartDef siteDef = SiteManager.GetDefForNewSite(int.Parse(PlanetBuilder_Temp.tempSiteTypes[i]),
                            PlanetBuilder_Temp.tempSiteIsFromFactions[i]);

                        Site site = SiteMaker.MakeSite(sitePart: siteDef,
                            tile: int.Parse(PlanetBuilder_Temp.tempSiteTiles[i]),
                            threatPoints: 1000,
                            faction: GetPlayerFaction(int.Parse(PlanetBuilder_Temp.tempSiteLikelihoods[i])));

                        playerSites.Add(site);
                        Find.WorldObjects.Add(site);
                    }

                    catch (Exception e) 
                    {
                        Log.Error($"Failed to spawn site at {PlanetBuilder_Temp.tempSiteTiles[i]}. Reason: {e}");
                    };
                }
            };
            toDo.Invoke();
        }

        public static void SpawnSingleSettlement(SettlementDetailsJSON newSettlementJSON)
        {
            Action toDo = delegate
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
            };
            toDo.Invoke();
        }

        public static void RemoveSingleSettlement(SettlementDetailsJSON newSettlementJSON)
        {
            Action toDo = delegate
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
            };
            toDo.Invoke();
        }

        public static void SpawnSingleSite(SiteDetailsJSON siteDetailsJSON)
        {
            Action toDo = delegate
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
            };
            toDo.Invoke();
        }

        public static void RemoveSingleSite(SiteDetailsJSON siteDetailsJSON)
        {
            Action toDo = delegate
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
            };
            toDo.Invoke();
        }
    }

    public static class PlanetBuilder_Temp
    {
        public static string[] tempSettlementTiles;

        public static string[] tempSettlementOwners;

        public static string[] tempSettlementLikelihoods;

        public static string[] tempSiteTiles;

        public static string[] tempSiteOwners;

        public static string[] tempSiteLikelihoods;

        public static string[] tempSiteTypes;

        public static bool[] tempSiteIsFromFactions;

        public static void SetWorldFeatures(ServerOverallJSON serverOverallJSON)
        {
            tempSettlementTiles = serverOverallJSON.settlementTiles.ToArray();
            tempSettlementOwners = serverOverallJSON.settlementOwners.ToArray();
            tempSettlementLikelihoods = serverOverallJSON.settlementLikelihoods.ToArray();

            tempSiteTiles = serverOverallJSON.siteTiles.ToArray();
            tempSiteOwners = serverOverallJSON.siteOwners.ToArray();
            tempSiteLikelihoods = serverOverallJSON.siteLikelihoods.ToArray();
            tempSiteTypes = serverOverallJSON.siteTypes.ToArray();
            tempSiteIsFromFactions = serverOverallJSON.isFromFactions.ToArray();
        }
    }
}
