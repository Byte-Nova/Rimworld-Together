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
        public static WorldData cachedWorldData;

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
            FactionDef factionToAdd;
            Dictionary<string, FactionData> factionDictionary = new Dictionary<string, FactionData>();
            Dictionary<string, byte[]> cacheDetailsFactionDict = new Dictionary<string, byte[]>();

            //Convert the string-byte[] dictionary into a string-FactionData dictionary
            foreach (string str in worldData.factions.Keys)
            {
                factionDictionary[str] = (FactionData)Serializer.ConvertBytesToObject(worldData.factions[str]);
            }

            //TODO
            //We might want to add a message for the players to let them know factions are missing
            //For now, we output into the console for debugging purposes

            //for each faction in worldDetails, try to add it to the client's world
            FactionData factionData = new FactionData();
            foreach (string factionName in factionDictionary.Keys)
            {
                factionToAdd = DefDatabase<FactionDef>.AllDefs.FirstOrCreate(factionDictionary[factionName] ,fetch => fetch.defName == factionName);

                factionToAdd.fixedName = factionDictionary[factionName].fixedName;
                factions.Add(factionToAdd);
                factionData = FactionScribeManager.factionToFactionDetails(factionToAdd);
                cacheDetailsFactionDict[factionName] = Serializer.ConvertObjectToBytes(factionData);
            }

            worldData.factions = cacheDetailsFactionDict;
            cachedWorldData = worldData;
        }

        public static FactionDef FirstOrCreate(this IEnumerable<FactionDef> factionDefs,FactionData currentFaction, Func<FactionDef,bool> predicate)
        {
            //Try to find the exact faction saved on the server
            FactionDef factionToReturn = factionDefs.FirstOrDefault(predicate);


            if (factionToReturn == null)
            {
                //try to find a similar faction that is currently in the world
                Faction factionFound = Current.Game.World.factionManager.AllFactions.FirstOrDefault(
                                fetch => (fetch.def.permanentEnemy == currentFaction.permanentEnemy) &&
                                (fetch.def.naturalEnemy == currentFaction.naturalEnemy) &&
                                ((byte)fetch.def.techLevel == currentFaction.techLevel) &&
                                (fetch.def.hidden == currentFaction.hidden));

                if (factionFound != null)
                    factionToReturn = factionFound.def;

                //if try to find a similar faction in all factionDefs
                if (factionToReturn == null)
                {
                    factionToReturn = factionDefs.FirstOrDefault(
                        fetch => (fetch.permanentEnemy == currentFaction.permanentEnemy) &&
                                (fetch.naturalEnemy == currentFaction.naturalEnemy) &&
                                ((byte)fetch.techLevel == currentFaction.techLevel) &&
                                (fetch.hidden == currentFaction.hidden));

                    //if a faction cannot be found with similar details, then make a new faction using the sent faction's details
                    if (factionToReturn == null)
                    {
                        factionToReturn = FactionScribeManager.factionDetailsToFaction(currentFaction);
                    }


                }
            }

            return factionToReturn;
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

            WorldGenerationData.initializeGenerationDefs();
            List<WorldGenStepDef> worldGenSteps = WorldGenerationData.RT_WorldGenSteps.ToList();

            for (int i = 0; i < worldGenSteps.Count(); i++)
            {
                Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(worldGenSteps, i));
                worldGenSteps[i].worldGenStep.GenerateFresh(seedString);
            }

            if (!ClientValues.needsToGenerateWorld)
            {
                XmlParser.parseGrid(cachedWorldData, tileData);

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
            WorldData worldData = new WorldData();
            worldData.worldStepMode = ((int)CommonEnumerators.WorldStepMode.Required).ToString();

            worldData.seedString = seedString;
            worldData.persistentRandomValue = persistentRandomValue;
            worldData.planetCoverage = planetCoverage.ToString();
            worldData.rainfall = ((int)rainfall).ToString();
            worldData.temperature = ((int)temperature).ToString();
            worldData.population = ((int)population).ToString();
            worldData.pollution = pollution.ToString();

            foreach (FactionDef factionDef in factions)
            {
                FactionData factionData = FactionScribeManager.factionToFactionDetails(factionDef);
                worldData.factions[factionDef.defName] = Serializer.ConvertObjectToBytes(factionData);
            }

            worldData = XmlParser.GetWorldXmlData(worldData);
            Log.Message(worldData.deflateDictionary[worldData.deflateDictionary.Keys.Last()]);
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
