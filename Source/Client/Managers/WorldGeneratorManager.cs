using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.Profile;

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
        public static List<FactionDef> factions = new List<FactionDef>();
        public static WorldData cachedWorldDetails;

        public static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      orderby x.order, x.index
                                                                      select x;

        public static void SetValuesFromGame(string seedString, float planetCoverage, OverallRainfall rainfall, OverallTemperature temperature, OverallPopulation population, List<FactionDef> factions, float pollution)
        {
            WorldGeneratorManager.seedString = seedString;
            WorldGeneratorManager.persistentRandomValue = GenText.StableStringHash(seedString);
            WorldGeneratorManager.planetCoverage = planetCoverage;
            WorldGeneratorManager.rainfall = rainfall;
            WorldGeneratorManager.temperature = temperature;
            WorldGeneratorManager.population = population;
            WorldGeneratorManager.pollution = pollution;
            WorldGeneratorManager.factions = factions;

            WorldGeneratorManager.factions.Add(FactionValues.neutralPlayerDef);
            WorldGeneratorManager.factions.Add(FactionValues.allyPlayerDef);
            WorldGeneratorManager.factions.Add(FactionValues.enemyPlayerDef);
            WorldGeneratorManager.factions.Add(FactionValues.yourOnlineFactionDef);
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

            factions = new List<FactionDef>();
            foreach(string str in worldData.factions)
            {
                factions.Add(DefDatabase<FactionDef>.AllDefs.First(fetch => fetch.defName == str));
            }

            cachedWorldDetails = worldData;
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
            Current.CreatingWorld.info.factions = factions;
            Current.CreatingWorld.info.pollution = pollution;

            WorldGenStepDef[] worldGenSteps = GenStepsInOrder.ToArray();
            for (int i = 0; i < worldGenSteps.Count(); i++)
            {
                worldGenSteps[i].worldGenStep.GenerateFresh(seedString);
            }

            Current.CreatingWorld.grid.StandardizeTileData();
            Current.CreatingWorld.FinalizeInit();
            Find.Scenario.PostWorldGenerate();

            if (!ModsConfig.IdeologyActive) Find.Scenario.PostIdeoChosen();
            return Current.CreatingWorld;
        }

        public static void SendWorldToServer()
        {
            WorldData worldData = new WorldData();
            worldData.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Required).ToString();

            worldData.seedString = seedString;
            worldData.persistentRandomValue = persistentRandomValue;
            worldData.planetCoverage = planetCoverage.ToString();
            worldData.rainfall = ((int)rainfall).ToString();
            worldData.temperature = ((int)temperature).ToString();
            worldData.population = ((int)population).ToString();
            worldData.pollution = pollution.ToString();
           
            foreach(FactionDef faction in factions)
            {
                worldData.factions.Add(faction.defName);
            }

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
    }
}
