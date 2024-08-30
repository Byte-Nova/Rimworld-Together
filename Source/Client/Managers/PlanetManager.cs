using System.Linq;
using RimWorld;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles all the planet functions for the mod

    public static class PlanetManager
    {
        //Regenerates the planet of player objects

        public static void BuildPlanet()
        {
            FactionValues.FindPlayerFactionsInWorld();
            PlanetManagerHelper.GetMapGenerators();

            //This step gets skiped if it's the first time building the planet
            if (ClientValues.isGeneratingFreshWorld) return;
            else
            {
                PlayerSettlementManager.ClearAllSettlements();
                SOS2SendData.ClearAllShips();
                PlayerSettlementManager.AddSettlements(PlayerSettlementManagerHelper.tempSettlements);

                PlayerSiteManager.ClearAllSites();
                PlayerSiteManager.AddSites(PlayerSiteManagerHelper.tempSites);

                NPCSettlementManager.ClearAllSettlements();
                NPCSettlementManager.AddSettlements(NPCSettlementManagerHelper.tempNPCSettlements);

                RoadManager.ClearAllRoads();
                RoadManager.AddRoads(RoadManagerHelper.tempRoadDetails, false);

                PollutionManager.ClearAllPollution();
                PollutionManager.AddPollutedTiles(PollutionManagerHelper.tempPollutionDetails, false);

                CaravanManager.ClearAllCaravans();
                CaravanManager.AddCaravans(CaravanManagerHelper.tempCaravanDetails);
            }
        }
    }

    //Helper class for the PlanetManager class

    public static class PlanetManagerHelper
    {
        public static MapGeneratorDef emptyGenerator;
        public static MapGeneratorDef defaultSettlementGenerator;
        public static MapGeneratorDef defaultSiteGenerator;

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
