using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.Shared.JSON;
using Verse;
using Verse.Profile;

namespace RimworldTogether.GameClient.Managers
{
    public static class WorldGeneratorManager
    {
        public static string seedString;
        public static float planetCoverage;
        public static OverallRainfall rainfall;
        public static OverallTemperature temperature;
        public static OverallPopulation population;
        public static float pollution;
        public static List<FactionDef> factions = new List<FactionDef>();

        private static List<WorldGenStepDef> tmpGenSteps = new List<WorldGenStepDef>();

        public static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      where x.defName != "Roads"
                                                                      orderby x.order, x.index
                                                                      select x;

        public static void SetValuesFromServer(WorldDetailsJSON worldDetailsJSON)
        {
            seedString = worldDetailsJSON.SeedString;
            planetCoverage = worldDetailsJSON.PlanetCoverage;
            rainfall = (OverallRainfall)worldDetailsJSON.Rainfall;
            temperature = (OverallTemperature)worldDetailsJSON.Temperature;
            population = (OverallPopulation)worldDetailsJSON.Population;
            pollution = worldDetailsJSON.Pollution;

            foreach (string factionDefName in worldDetailsJSON.Factions)
            {
                try
                {
                    foreach (FactionDef def in DefDatabase<FactionDef>.AllDefs)
                    {
                        if (factionDefName == def.defName)
                        {
                            factions.Add(def);
                            continue;
                        }
                    }
                }

                catch (Exception e) 
                {
                    Log.Warning($"Error when trying to add faction into world, perhaps missing?" +
                        $" Exception: {e}");
                }
            }
        }

        public static void GeneratePatchedWorld()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Find.GameInitData.ResetWorldRelatedMapInitData();
                Current.Game.World = GenerateWorld();
                LongEventHandler.ExecuteWhenFinished(delegate
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

                    MemoryUtility.UnloadUnusedUnityAssets();
                    Find.World.renderer.RegenerateAllLayersNow();
                });
            }, "GeneratingWorld", doAsynchronously: true, null);
        }

        private static World GenerateWorld()
        {
            Rand.PushState();
            Rand.Seed = GenText.StableStringHash(seedString);

            Current.CreatingWorld = new World();
            Current.CreatingWorld.info.seedString = seedString;
            Current.CreatingWorld.info.planetCoverage = planetCoverage;
            Current.CreatingWorld.info.overallRainfall = rainfall;
            Current.CreatingWorld.info.overallTemperature = temperature;
            Current.CreatingWorld.info.overallPopulation = population;
            Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld);
            Current.CreatingWorld.info.factions = factions;
            Current.CreatingWorld.info.pollution = pollution;

            tmpGenSteps.Clear();
            tmpGenSteps.AddRange(GenStepsInOrder);

            for (int i = 0; i < tmpGenSteps.Count; i++)
            {
                tmpGenSteps[i].worldGenStep.GenerateFresh(seedString);
            }
        
            Current.CreatingWorld.grid.StandardizeTileData();
            Current.CreatingWorld.FinalizeInit();
            Find.Scenario.PostWorldGenerate();

            if (!ModsConfig.IdeologyActive) Find.Scenario.PostIdeoChosen();
            return Current.CreatingWorld;
        }
    }
}
