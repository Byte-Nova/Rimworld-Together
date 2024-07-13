using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles all the planet functions for the mod

    public static class PlanetManager
    {
        public static List<Settlement> playerSettlements = new List<Settlement>();

        public static List<Site> playerSites = new List<Site>();

        //Parses the required packet into a useful order

        public static void ParseSettlementPacket(Packet packet)
        {
            SettlementData settlementData = Serializer.ConvertBytesToObject<SettlementData>(packet.contents);

            switch (settlementData.settlementStepMode)
            {
                case SettlementStepMode.Add:
                    SpawnSingleSettlement(settlementData);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSingleSettlement(settlementData);
                    break;
            }
        }

        //Regenerates the planet of player objects

        public static void BuildPlanet()
        {
            FactionValues.FindPlayerFactionsInWorld();
            PlanetManagerHelper.GetMapGenerators();

            //This step gets skiped if it's the first time building the planet
            if (ClientValues.isGeneratingFreshWorld) return;
            else
            {
                RemoveNPCSettlements();
                SpawnNPCSettlements();

                RemoveOldSettlements();
                SpawnPlayerSettlements();

                RemoveOldSites();
                SpawnPlayerSites();

                RoadManager.ClearAllRoads();
                RoadManager.AddRoads(RoadManagerHelper.tempRoadDetails, false);

                CaravanManager.ClearAllCaravans();
                CaravanManager.AddCaravans(CaravanManagerHelper.tempCaravanDetails);
            }
        }

        //Spawns player settlements

        private static void SpawnNPCSettlements()
        {
            if (PlanetManagerHelper.tempNPCSettlements == null) return;

            for (int i = 0; i < PlanetManagerHelper.tempNPCSettlements.Count(); i++)
            {
                PlanetNPCSettlement PlanetNPCSettlement = PlanetManagerHelper.tempNPCSettlements[i];

                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = PlanetNPCSettlement.tile;
                    settlement.Name = PlanetNPCSettlement.name;

                    //TODO
                    //THIS FUNCTION WILL ALWAYS ASSIGN ALL SETTLEMENTS TO THE FIRST INSTANCE OF A FACTION IF THERE'S MORE OF ONE OF THE SAME TIME
                    //HAVING MULTIPLE GENTLE TRIBES WILL SYNC ALL THE SETTLEMENTS OF THE GENTLE TRIBES TO THE FIRST ONE. FIX!!
                    settlement.SetFaction(PlanetManagerHelper.GetNPCFactionFromDefName(PlanetNPCSettlement.factionDefName));

                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to build NPC settlement at {PlanetNPCSettlement.tile}. Reason: {e}"); }
            }
        }

        //Removes player settlements

        private static void RemoveNPCSettlements()
        {
            DestroyedSettlement[] destroyedSettlements = Find.WorldObjects.DestroyedSettlements.Where(fetch => !FactionValues.playerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (DestroyedSettlement destroyedSettlement in destroyedSettlements) Find.WorldObjects.Remove(destroyedSettlement);

            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => !FactionValues.playerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (Settlement settlement in settlements) Find.WorldObjects.Remove(settlement);
        }

        //Spawns player settlements

        private static void SpawnPlayerSettlements()
        {
            if (PlanetManagerHelper.tempSettlements == null) return;

            for (int i = 0; i < PlanetManagerHelper.tempSettlements.Count(); i++)
            {
                OnlineSettlementFile settlementFile = PlanetManagerHelper.tempSettlements[i];

                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = settlementFile.tile;
                    settlement.Name = $"{settlementFile.owner}'s settlement";
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(settlementFile.goodwill));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to build settlement at {settlementFile.tile}. Reason: {e}"); }
            }
        }

        //Removes old player settlements

        private static void RemoveOldSettlements()
        {
            playerSettlements.Clear();

            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction)).ToArray();
            foreach (Settlement settlement in settlements) Find.WorldObjects.Remove(settlement);
        }

        //Spawns player sites

        private static void SpawnPlayerSites()
        {
            if (PlanetManagerHelper.tempSites == null) return;

            for (int i = 0; i < PlanetManagerHelper.tempSites.Count(); i++)
            {
                OnlineSiteFile siteFile = PlanetManagerHelper.tempSites[i];

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

        //Removes old player sites

        private static void RemoveOldSites()
        {
            playerSites.Clear();

            Site[] sites = Find.WorldObjects.Sites.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction)).ToArray();
            foreach (Site site in sites) Find.WorldObjects.Remove(site);

            sites = Find.WorldObjects.Sites.Where(fetch => fetch.Faction == Faction.OfPlayer).ToArray();
            foreach (Site site in sites) Find.WorldObjects.Remove(site);
        }

        //Spawns a player settlement from a request

        public static void SpawnSingleSettlement(SettlementData newSettlementJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = newSettlementJSON.tile;
                    settlement.Name = $"{newSettlementJSON.owner}'s settlement";
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(newSettlementJSON.goodwill));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn settlement at {newSettlementJSON.tile}. Reason: {e}"); }
            }
        }

        //Removes a player settlement from a request

        public static void RemoveSingleSettlement(SettlementData newSettlementJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement toGet = playerSettlements.Find(x => x.Tile == newSettlementJSON.tile);

                    playerSettlements.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Logger.Error($"Failed to remove settlement at {newSettlementJSON.tile}. Reason: {e}"); }
            }
        }

        //Spawns a player site from a request

        public static void SpawnSingleSite(SiteData siteData)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    SitePartDef siteDef = SiteManager.GetDefForNewSite(siteData.type,siteData.isFromFaction);
                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: siteData.tile,
                        threatPoints: 1000,
                        faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(siteData.goodwill));

                    playerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn site at {siteData.tile}. Reason: {e}"); }
            }
        }

        //Removes a player site from a request

        public static void RemoveSingleSite(SiteData siteData)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Site toGet = playerSites.Find(x => x.Tile == siteData.tile);

                    playerSites.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Logger.Message($"Failed to remove site at {siteData.tile}. Reason: {e}"); }
            }
        }
    }

    //Helper class for the PlanetManager class

    public static class PlanetManagerHelper
    {
        public static PlanetNPCSettlement[] tempNPCSettlements;
        public static OnlineSettlementFile[] tempSettlements;
        public static OnlineSiteFile[] tempSites;

        public static MapGeneratorDef emptyGenerator;
        public static MapGeneratorDef defaultSettlementGenerator;
        public static MapGeneratorDef defaultSiteGenerator;

        public static void SetWorldFeatures(ServerGlobalData serverGlobalData)
        {
            tempNPCSettlements = serverGlobalData.npcSettlements;
            tempSettlements = serverGlobalData.playerSettlements;
            tempSites = serverGlobalData.playerSites;
        }

        //Returns an online faction depending on the value

        public static Faction GetPlayerFactionFromGoodwill(Goodwill goodwill)
        {
            Faction factionToUse = null;

            switch (goodwill)
            {
                case Goodwill.Enemy:
                    factionToUse = FactionValues.enemyPlayer;
                    break;

                case Goodwill.Neutral:
                    factionToUse = FactionValues.neutralPlayer;
                    break;

                case Goodwill.Ally:
                    factionToUse = FactionValues.allyPlayer;
                    break;

                case Goodwill.Faction:
                    factionToUse = FactionValues.yourOnlineFaction;
                    break;

                case Goodwill.Personal:
                    factionToUse = Faction.OfPlayer;
                    break;
            }

            return factionToUse;
        }

        //Returns an npc faction depending on the value

        public static Faction GetNPCFactionFromDefName(string defName)
        {
            return Find.World.factionManager.AllFactions.First(fetch => fetch.def.defName == defName);
        }

        //Gets the default generator for the map builder

        public static void GetMapGenerators()
        {
            emptyGenerator = DefDatabase<MapGeneratorDef>.AllDefs.First(fetch => fetch.defName == "Empty");

            WorldObjectDef settlement = WorldObjectDefOf.Settlement;
            defaultSettlementGenerator = settlement.mapGenerator;

            WorldObjectDef site = WorldObjectDefOf.Site;
            defaultSiteGenerator = site.mapGenerator;
        }

        //Sets the default generator for the map builder

        public static void SetDefaultGenerators()
        {
            WorldObjectDef settlement = WorldObjectDefOf.Settlement;
            settlement.mapGenerator = defaultSettlementGenerator;

            WorldObjectDef site = WorldObjectDefOf.Site;
            site.mapGenerator = defaultSiteGenerator;
        }

        public static void SetOverrideGenerators()
        {
            WorldObjectDef settlement = WorldObjectDefOf.Settlement;
            settlement.mapGenerator = emptyGenerator;

            WorldObjectDef site = WorldObjectDefOf.Site;
            site.mapGenerator = emptyGenerator;
        }
    }
}
