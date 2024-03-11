using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;

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
        public static WorldDetailsJSON cachedWorldDetails;

        public static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      orderby x.order, x.index
                                                                      select x;

        public static void SetValuesFromGame(string seedString, float planetCoverage, OverallRainfall rainfall, OverallTemperature temperature, OverallPopulation population, List<FactionDef> factions, float pollution)
        {
            WorldGeneratorManager.seedString = seedString;
            WorldGeneratorManager.persistentRandomValue = 0;
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

        public static void SetValuesFromServer(WorldDetailsJSON worldDetailsJSON)
        {
            seedString = worldDetailsJSON.seedString;
            persistentRandomValue = worldDetailsJSON.persistentRandomValue;
            planetCoverage = float.Parse(worldDetailsJSON.planetCoverage);
            rainfall = (OverallRainfall)int.Parse(worldDetailsJSON.rainfall);
            temperature = (OverallTemperature)int.Parse(worldDetailsJSON.temperature);
            population = (OverallPopulation)int.Parse(worldDetailsJSON.population);
            pollution = float.Parse(worldDetailsJSON.pollution);

            factions = new List<FactionDef>();
            foreach(string str in worldDetailsJSON.factions)
            {
                factions.Add(DefDatabase<FactionDef>.AllDefs.First(fetch => fetch.defName == str));
            }

            cachedWorldDetails = worldDetailsJSON;
        }

        public static void GeneratePatchedWorld(bool firstGeneration)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Find.GameInitData.ResetWorldRelatedMapInitData();
                Current.Game.World = GenerateWorld();
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    PostWorldGeneration();

                    if (!firstGeneration) ClientValues.ToggleRequireSaveManipulation(true);
                });
            }, "GeneratingWorld", doAsynchronously: true, null);
        }

        private static World GenerateWorld()
        {
            Rand.PushState(0);
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
            WorldDetailsJSON worldDetailsJSON = new WorldDetailsJSON();
            worldDetailsJSON.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Required).ToString();

            worldDetailsJSON.seedString = seedString;
            worldDetailsJSON.persistentRandomValue = persistentRandomValue;
            worldDetailsJSON.planetCoverage = planetCoverage.ToString();
            worldDetailsJSON.rainfall = ((int)rainfall).ToString();
            worldDetailsJSON.temperature = ((int)temperature).ToString();
            worldDetailsJSON.population = ((int)population).ToString();
            worldDetailsJSON.pollution = pollution.ToString();
           
            foreach(FactionDef faction in factions)
            {
                worldDetailsJSON.factions.Add(faction.defName);
            }

            worldDetailsJSON = XmlParser.GetWorldXmlData(worldDetailsJSON);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        public static void GetWorldFromServer()
        {
            XmlParser.ModifyWorldXml(cachedWorldDetails);
            GameDataSaveLoader.LoadGame(SaveManager.customSaveName);
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
