using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.Profile;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class WorldGeneratorManager
    {
        public static string seedString;
        public static int persistentRandomValue;
        public static float planetCoverage;
        public static OverallRainfall rainfall;
        public static OverallTemperature temperature;
        public static OverallPopulation population;
        public static float pollution;
        public static FactionDef[] factions;
        public static WorldAISettlement[] npcSettlements;
        public static WorldData cachedWorldData;

        public static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      orderby x.order, x.index
                                                                      select x;

        public static void SetValuesFromGame(string seedString, float planetCoverage, OverallRainfall rainfall, OverallTemperature temperature, OverallPopulation population, List<FactionDef> factions, float pollution)
        {
            WorldGeneratorManager.seedString = seedString;
            persistentRandomValue = GenText.StableStringHash(seedString);
            WorldGeneratorManager.planetCoverage = planetCoverage;
            WorldGeneratorManager.rainfall = rainfall;
            WorldGeneratorManager.temperature = temperature;
            WorldGeneratorManager.population = population;
            WorldGeneratorManager.pollution = pollution;
            WorldGeneratorManager.factions = GetWorldFactions(null, factions);
        }

        public static void SetValuesFromServer(WorldData worldData)
        {
            seedString = worldData.seedString;
            persistentRandomValue = worldData.persistentRandomValue;
            planetCoverage = float.Parse(worldData.planetCoverage);
            rainfall = (OverallRainfall)int.Parse(worldData.rainfall);
            temperature = (OverallTemperature)int.Parse(worldData.temperature);
            population = (OverallPopulation)int.Parse(worldData.population);
            pollution = float.Parse(worldData.pollution);
            factions = GetWorldFactions(worldData, null);
            cachedWorldData = worldData;
        }

        public static void GeneratePatchedWorld()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Find.GameInitData.ResetWorldRelatedMapInitData();
                Current.Game.World = GenerateWorld();
                LongEventHandler.ExecuteWhenFinished(delegate 
                {
                    Find.World.renderer.RegenerateAllLayersNow();
                    MemoryUtility.UnloadUnusedUnityAssets();
                    Current.CreatingWorld = null;
                    PostWorldGeneration();
                });
            }, "GeneratingWorld", doAsynchronously: true, null);
        }

        private static World GenerateWorld()
        {
            Rand.PushState(persistentRandomValue);
            Current.CreatingWorld = new World();
            Current.CreatingWorld.info.seedString = seedString;
            Current.CreatingWorld.info.persistentRandomValue = persistentRandomValue;
            Current.CreatingWorld.info.planetCoverage = planetCoverage;
            Current.CreatingWorld.info.overallRainfall = rainfall;
            Current.CreatingWorld.info.overallTemperature = temperature;
            Current.CreatingWorld.info.overallPopulation = population;
            Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld);
            Current.CreatingWorld.info.factions = factions.ToList();
            Current.CreatingWorld.info.pollution = pollution;

            WorldGenStepDef[] worldGenSteps = GenStepsInOrder.ToArray();
            for (int i = 0; i < worldGenSteps.Count(); i++) worldGenSteps[i].worldGenStep.GenerateFresh(seedString);

            Current.CreatingWorld.grid.StandardizeTileData();
            Current.CreatingWorld.FinalizeInit();
            Find.Scenario.PostWorldGenerate();

            if (!ModsConfig.IdeologyActive) Find.Scenario.PostIdeoChosen();
            return Current.CreatingWorld;
        }

        public static void SendWorldToServer()
        {
            WorldData worldData = new WorldData();
            worldData.worldStepMode = WorldStepMode.Required;

            worldData.seedString = seedString;
            worldData.persistentRandomValue = persistentRandomValue;
            worldData.planetCoverage = planetCoverage.ToString();
            worldData.rainfall = ((int)rainfall).ToString();
            worldData.temperature = ((int)temperature).ToString();
            worldData.population = ((int)population).ToString();
            worldData.pollution = pollution.ToString();
            worldData.factions = GetWorldFactionsDefNames();
            worldData.npcSettlements = GetWorldSettlements();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void PostWorldGeneration()
        {
            Page_SelectStartingSite newSelectStartingSite = new Page_SelectStartingSite();
            Page_ConfigureStartingPawns newConfigureStartingPawns = new Page_ConfigureStartingPawns();
            newConfigureStartingPawns.nextAct = PageUtility.InitGameStart;

            if (ModsConfig.IdeologyActive)
            {
                Page_ChooseIdeoPreset newChooseIdeoPreset = new Page_ChooseIdeoPreset();
                newChooseIdeoPreset.prev = newSelectStartingSite;
                newChooseIdeoPreset.next = newConfigureStartingPawns;

                newSelectStartingSite.next = newChooseIdeoPreset;
            }

            else
            {
                newSelectStartingSite.next = newConfigureStartingPawns;
                newConfigureStartingPawns.prev = newSelectStartingSite;
            }

            Find.WindowStack.Add(newSelectStartingSite);
            DialogShortcuts.ShowWorldGenerationDialogs();
        }

        private static FactionDef[] GetWorldFactions(WorldData worldData, List<FactionDef> factionData)
        {
            if (ClientValues.needsToGenerateWorld)
            {
                factionData.Add(FactionValues.neutralPlayerDef);
                factionData.Add(FactionValues.allyPlayerDef);
                factionData.Add(FactionValues.enemyPlayerDef);
                factionData.Add(FactionValues.yourOnlineFactionDef);
                return factionData.ToArray();
            }

            else
            {
                List<FactionDef> factionsToUse = new List<FactionDef>();
                foreach (string str in worldData.factions)
                {
                    FactionDef faction = DefDatabase<FactionDef>.AllDefs.ToList().Find(fetch => fetch.defName == str);
                    if (faction != null) factionsToUse.Add(faction);
                    else Logger.Warning($"Faction '{str}' wasn't found in the client's game, ignoring");
                }
                return factionsToUse.ToArray();
            }
        }

        private static string[] GetWorldFactionsDefNames()
        {
            List<string> factionDefNames = new List<string>();
            foreach (FactionDef faction in factions) factionDefNames.Add(faction.defName);
            return factionDefNames.ToArray();
        }

        private static WorldAISettlement[] GetWorldSettlements()
        {
            List<WorldAISettlement> npcSettlements = new List<WorldAISettlement>();
            foreach (Settlement settlement in Find.World.worldObjects.Settlements.Where(fetch => factions.Contains(fetch.Faction.def)))
            {
                WorldAISettlement worldAISettlement = new WorldAISettlement();
                worldAISettlement.tile = settlement.Tile;
                worldAISettlement.factionDefName = settlement.Faction.def.defName;
                worldAISettlement.name = settlement.Name;

                npcSettlements.Add(worldAISettlement);
            }
            return npcSettlements.ToArray();
        }
    }
}
