using System;
using System.Collections.Generic;
using System.IO;
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

        public static void SetValuesForWorld(string seedString, float planetCoverage, OverallRainfall rainfall, OverallTemperature temperature, OverallPopulation population, List<FactionDef> factions, float pollution)
        {
            WorldGeneratorManager.seedString = seedString;
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
                    if (firstGeneration)
                    {
                        SendWorldToServer();
                        PostWorldGeneration();
                        ClientValues.ToggleGenerateWorld(false);
                    }

                    else
                    {
                        PostWorldGeneration();
                        ClientValues.ToggleRequireSaveManipulation(true);
                    }
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

        private static void SendWorldToServer()
        {
            SaveManager.ForceSave();

            WorldDetailsJSON worldDetailsJSON = new WorldDetailsJSON();
            worldDetailsJSON.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Required).ToString();

            worldDetailsJSON.seedString = seedString;
            worldDetailsJSON.planetCoverage = planetCoverage.ToString();
            worldDetailsJSON.rainfall = ((int)rainfall).ToString();
            worldDetailsJSON.temperature = ((int)temperature).ToString(); ;
            worldDetailsJSON.population = ((int)population).ToString();
            worldDetailsJSON.pollution = pollution.ToString();
           
            foreach(FactionDef faction in factions)
            {
                worldDetailsJSON.factions.Add(faction.defName);
            }

            string filePath = Path.Combine(new string[] { Master.savesPath, SaveManager.customSaveName + ".rws" });
            worldDetailsJSON.tileBiomeDeflate = XMLParser.GetDataFromXML(filePath, "tileBiomeDeflate");
            worldDetailsJSON.tileElevationDeflate = XMLParser.GetDataFromXML(filePath, "tileElevationDeflate");
            worldDetailsJSON.tileHillinessDeflate = XMLParser.GetDataFromXML(filePath, "tileHillinessDeflate");
            worldDetailsJSON.tileTemperatureDeflate = XMLParser.GetDataFromXML(filePath, "tileTemperatureDeflate");
            worldDetailsJSON.tileRainfallDeflate = XMLParser.GetDataFromXML(filePath, "tileRainfallDeflate");
            worldDetailsJSON.tileSwampinessDeflate = XMLParser.GetDataFromXML(filePath, "tileSwampinessDeflate");
            worldDetailsJSON.tileFeatureDeflate = XMLParser.GetDataFromXML(filePath, "tileFeatureDeflate");
            worldDetailsJSON.tilePollutionDeflate = XMLParser.GetDataFromXML(filePath, "tilePollutionDeflate");
            worldDetailsJSON.tileRoadOriginsDeflate = XMLParser.GetDataFromXML(filePath, "tileRoadOriginsDeflate");
            worldDetailsJSON.tileRoadAdjacencyDeflate = XMLParser.GetDataFromXML(filePath, "tileRoadAdjacencyDeflate");
            worldDetailsJSON.tileRoadDefDeflate = XMLParser.GetDataFromXML(filePath, "tileRoadDefDeflate");
            worldDetailsJSON.tileRiverOriginsDeflate = XMLParser.GetDataFromXML(filePath, "tileRiverOriginsDeflate");
            worldDetailsJSON.tileRiverAdjacencyDeflate = XMLParser.GetDataFromXML(filePath, "tileRiverAdjacencyDeflate");
            worldDetailsJSON.tileRiverDefDeflate = XMLParser.GetDataFromXML(filePath, "tileRiverDefDeflate");

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldDetailsJSON);
            Network.listener.dataQueue.Enqueue(packet);
        }

        public static void GetWorldFromServer()
        {
            WorldDetailsJSON worldDetailsJSON = cachedWorldDetails;
            SaveManager.ForceSave();

            string filePath = Path.Combine(new string[] { Master.savesPath, SaveManager.customSaveName + ".rws" });
            XMLParser.SetDataIntoXML(filePath, "tileBiomeDeflate", worldDetailsJSON.tileBiomeDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileElevationDeflate", worldDetailsJSON.tileElevationDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileHillinessDeflate", worldDetailsJSON.tileHillinessDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileTemperatureDeflate", worldDetailsJSON.tileTemperatureDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileRainfallDeflate", worldDetailsJSON.tileRainfallDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileSwampinessDeflate", worldDetailsJSON.tileSwampinessDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileFeatureDeflate", worldDetailsJSON.tileFeatureDeflate);
            XMLParser.SetDataIntoXML(filePath, "tilePollutionDeflate", worldDetailsJSON.tilePollutionDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileRoadOriginsDeflate", worldDetailsJSON.tileRoadOriginsDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileRoadAdjacencyDeflate", worldDetailsJSON.tileRoadAdjacencyDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileRoadDefDeflate", worldDetailsJSON.tileRoadDefDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileRiverOriginsDeflate", worldDetailsJSON.tileRiverOriginsDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileRiverAdjacencyDeflate", worldDetailsJSON.tileRiverAdjacencyDeflate);
            XMLParser.SetDataIntoXML(filePath, "tileRiverDefDeflate", worldDetailsJSON.tileRiverDefDeflate);

            ClientValues.ToggleRequireSaveManipulation(false);
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
