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
        public static WorldDetailsJSON cachedWorldDetails;

        public static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      orderby x.order, x.index
                                                                      select x;

        public static Dictionary<string, byte[]> tileData = new()
        {
             {"tileBiome"         , new byte[]{} }  ,
             {"tileElevation"     , new byte[]{} }  ,
             {"tileHilliness"     , new byte[]{} }  ,
             {"tileTemperature"   , new byte[]{} }  ,
             {"tileRainfall"      , new byte[]{} }  ,
             {"tileSwampiness"    , new byte[]{} }  ,
             {"tileFeature"       , new byte[]{} }  ,
             {"tileRoadOrigins"   , new byte[]{} }  ,
             {"tileRoadAdjacency" , new byte[]{} }  ,
             {"tileRoadDef"       , new byte[]{} }  ,
             {"tileRiverOrigins"  , new byte[]{} }  ,
             {"tileRiverAdjacency", new byte[]{} }  ,
             {"tileRiverDef"      , new byte[]{} }
          };

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

            //TODO
            //We might want to add a message for the players to let them know factions are missing
            //For now, we output into the console for debugging purposes

            factions = new List<FactionDef>();
            foreach(string factionName in worldDetailsJSON.factions.Keys)
            {
                FactionDef faction = DefDatabase<FactionDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                if (faction != null) factions.Add(faction);
                else Log.Warning($"[Rimworld Together] > Faction '{str}' wasn't found in the client's game, ignoring");
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
                    if (!firstGeneration) ClientValues.requiresSaveManipulation = true;
                    Find.World.renderer.RegenerateAllLayersNow();
                    MemoryUtility.UnloadUnusedUnityAssets();
                    Current.CreatingWorld = null;
                    PostWorldGeneration();
                });
            }, "GeneratingWorld", doAsynchronously: true, null);
        }

        private static World GenerateWorld()
        {
            Rand.PushState();
            int seed = (Rand.Seed = GenText.StableStringHash(seedString));

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

            List<WorldGenStepDef> worldGenSteps = GenStepsInOrder.ToList();
            for (int i = 0; i < worldGenSteps.Count(); i++)
            {
                Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(worldGenSteps, i));
                worldGenSteps[i].worldGenStep.GenerateFresh(seedString);
            }

            if (!ClientValues.needsToGenerateWorld)
            {
                XmlParser.parseGrid(cachedWorldDetails, tileData);

                RawDataToTiles(Current.CreatingWorld.grid, tileData);
            }

            Current.CreatingWorld.grid.StandardizeTileData();
            Current.CreatingWorld.FinalizeInit();
            Find.Scenario.PostWorldGenerate();

            if (!ModsConfig.IdeologyActive) Find.Scenario.PostIdeoChosen();
            return Current.CreatingWorld;
        }

        private static int GetSeedPart(List<WorldGenStepDef> genSteps, int index)
        {
            int seedPart = genSteps[index].worldGenStep.SeedPart;
            int num = 0;
            for (int i = 0; i < index; i++)
            {
                if (genSteps[i].worldGenStep.SeedPart == seedPart)
                {
                    num++;
                }
            }
            return seedPart + num;
        }

        public static void RawDataToTiles(WorldGrid grid, Dictionary<string, byte[]> tileData)
        {


            if (grid.tiles.Count != grid.TilesCount)
            {
                grid.tiles.Clear();
                for (int m = 0; m < grid.TilesCount; m++)
                {
                    grid.tiles.Add(new Tile());
                }
            }
            else
            {
                for (int j = 0; j < grid.TilesCount; j++)
                {
                    grid.tiles[j].potentialRoads = null;
                    grid.tiles[j].potentialRivers = null;
                }
            }
            
            DataSerializeUtility.LoadUshort(tileData["tileBiome"], grid.TilesCount, delegate (int i, ushort data)
            {
                grid.tiles[i].biome = (DefDatabase<BiomeDef>.GetByShortHash(data) ?? BiomeDefOf.TemperateForest);
            });
            DataSerializeUtility.LoadUshort(tileData["tileElevation"], grid.TilesCount, delegate (int i, ushort data)
            {
                grid.tiles[i].elevation = (float)(data - 8192);
            });
            DataSerializeUtility.LoadByte(tileData["tileHilliness"], grid.TilesCount, delegate (int i, byte data)
            {
                grid.tiles[i].hilliness = (Hilliness)data;
            });
            DataSerializeUtility.LoadUshort(tileData["tileTemperature"], grid.TilesCount, delegate (int i, ushort data)
            {
                grid.tiles[i].temperature = (float)data / 10f - 300f;
            });
            DataSerializeUtility.LoadUshort(tileData["tileRainfall"], grid.TilesCount, delegate (int i, ushort data)
            {
                grid.tiles[i].rainfall = (float)data;
            });
            DataSerializeUtility.LoadByte(tileData["tileSwampiness"], grid.TilesCount, delegate (int i, byte data)
            {
                grid.tiles[i].swampiness = (float)data / 255f;
            });

            //Road Info
            int[] array = DataSerializeUtility.DeserializeInt(tileData["tileRoadOrigins"]);
            byte[] array2 = DataSerializeUtility.DeserializeByte(tileData["tileRoadAdjacency"]);
            ushort[] array3 = DataSerializeUtility.DeserializeUshort(tileData["tileRoadDef"]);
            for (int k = 0; k < array.Length; k++)
            {
                int num = array[k];
                int tileNeighbor = grid.GetTileNeighbor(num, (int)array2[k]);
                RoadDef byShortHash = DefDatabase<RoadDef>.GetByShortHash(array3[k]);
                if (byShortHash != null)
                {
                    if (grid.tiles[num].potentialRoads == null)
                    {
                        grid.tiles[num].potentialRoads = new List<Tile.RoadLink>();
                    }
                    if (grid.tiles[tileNeighbor].potentialRoads == null)
                    {
                        grid.tiles[tileNeighbor].potentialRoads = new List<Tile.RoadLink>();
                    }
                    grid.tiles[num].potentialRoads.Add(new Tile.RoadLink
                    {
                        neighbor = tileNeighbor,
                        road = byShortHash
                    });
                    grid.tiles[tileNeighbor].potentialRoads.Add(new Tile.RoadLink
                    {
                        neighbor = num,
                        road = byShortHash
                    });
                }
            }
            int[] array4 = DataSerializeUtility.DeserializeInt(tileData["tileRiverOrigins"]);
            byte[] array5 = DataSerializeUtility.DeserializeByte(tileData["tileRiverAdjacency"]);
            ushort[] array6 = DataSerializeUtility.DeserializeUshort(tileData["tileRiverDef"]);
            for (int l = 0; l < array4.Length; l++)
            {
                int num2 = array4[l];
                int tileNeighbor2 = grid.GetTileNeighbor(num2, (int)array5[l]);
                RiverDef byShortHash2 = DefDatabase<RiverDef>.GetByShortHash(array6[l]);
                if (byShortHash2 != null)
                {
                    if (grid.tiles[num2].potentialRivers == null)
                    {
                        grid.tiles[num2].potentialRivers = new List<Tile.RiverLink>();
                    }
                    if (grid.tiles[tileNeighbor2].potentialRivers == null)
                    {
                        grid.tiles[tileNeighbor2].potentialRivers = new List<Tile.RiverLink>();
                    }
                    grid.tiles[num2].potentialRivers.Add(new Tile.RiverLink
                    {
                        neighbor = tileNeighbor2,
                        river = byShortHash2
                    });
                    grid.tiles[tileNeighbor2].potentialRivers.Add(new Tile.RiverLink
                    {
                        neighbor = num2,
                        river = byShortHash2
                    });
                }
            }
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

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldDetailsJSON);
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
