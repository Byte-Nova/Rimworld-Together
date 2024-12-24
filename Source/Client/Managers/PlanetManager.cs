using System.Collections.Generic;
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
                PlayerSettlementManager.AddSettlements(PlayerSettlementManagerHelper.tempSettlements);

                SiteManager.ClearAllSites();
                SiteManager.AddSites(SiteManagerHelper.tempSites);

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

        public static List<Faction> GetNPCFactionFromDefName(string defName)
        {
            List<Faction> factions = new List<Faction>();
            foreach (Faction faction in Find.World.factionManager.AllFactions)
            {
                if (faction.def.defName == defName)
                {
                    factions.Add(faction);
                }
            }

            if(factions.Count >= 1) return factions;
            else
            {
                switch (defName) // If missing factions from missing dlcs.
                {
                    case "OutlanderRoughPig":
                        factions.AddRange(GetNPCFactionFromDefName(FactionDefOf.OutlanderRough.defName));
                        break;

                    case "PirateYttakin":
                        factions.AddRange(GetNPCFactionFromDefName(FactionDefOf.Pirate.defName));
                        break;

                    case "PirateWaster":
                        factions.AddRange(GetNPCFactionFromDefName(FactionDefOf.Pirate.defName));
                        break;

                    case "TribeRoughNeanderthal":
                        factions.AddRange(GetNPCFactionFromDefName(FactionDefOf.TribeRough.defName));
                        break;

                    case "TribeSavageImpid":
                        factions.AddRange(GetNPCFactionFromDefName(FactionDefOf.TribeRough.defName));
                        break;

                    case "TribeCannibal":
                        factions.AddRange(GetNPCFactionFromDefName(FactionDefOf.TribeRough.defName));
                        break;

                    case "Empire":
                        factions.AddRange(GetNPCFactionFromDefName(FactionDefOf.OutlanderCivil.defName));
                        break;
                        
                    default:
                        break;
                }

                return factions;
            }
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
