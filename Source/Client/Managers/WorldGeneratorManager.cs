using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.Profile;
using System;
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
        public static List<FactionDef> factions = new List<FactionDef>();
        public static List<FactionDef> initialFactions = new List<FactionDef>();
        public static WorldData cachedWorldData;
        public static bool firstGeneration;


        //A dictionary of faction defs and their corrosponding FactionData
        public static Dictionary<string, FactionData> factionDictionary = new Dictionary<string, FactionData>();

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


            //TODO
            //We might want to add a message for the players to let them know factions are missing
            //For now, we output into the console for debugging purposes

            Dictionary<string, byte[]> cacheDetailsFactionDict = new Dictionary<string, byte[]>();

            //Create the default faction list
            ResetFactionCounts();

            //Convert the string-byte[] dictionary into a string-FactionData dictionary
            foreach (string str in worldData.factions.Keys)
                factionDictionary[str] = (FactionData)Serializer.ConvertBytesToObject(worldData.factions[str]);

            //for each faction in worldDetails, try to add it to the client's world
            factions = new List<FactionDef>();
            List<string> excludedFactionDefs = new List<string>();
            foreach (string factionName in factionDictionary.Keys)
            {
                //find the faction def with the best match to the one the server provided
                FactionDef factionToAdd = DefDatabase<FactionDef>.AllDefs.BestMatch(factionDictionary[factionName] ,fetch => fetch.defName == factionName);
                
                //the local def name may be different from the server def name i.e. "pirate" and "yttakinPirate" respectively
                factionDictionary[factionName].localDefName = factionToAdd.defName;
                excludedFactionDefs.Add(factionToAdd.defName);

                factions.Add(factionToAdd);
            }



            worldData.factions = cacheDetailsFactionDict;
            cachedWorldData = worldData;
        }

        private static void ResetFactionCounts()
        {
            factions = new List<FactionDef>();
            foreach (FactionDef configurableFaction in FactionGenerator.ConfigurableFactions)
            {
                if (configurableFaction.startingCountAtWorldCreation > 0)
                {
                    for (int i = 0; i < configurableFaction.startingCountAtWorldCreation; i++)
                    {
                        factions.Add(configurableFaction);
                    }
                }
            }

            foreach (FactionDef faction in FactionGenerator.ConfigurableFactions)
            {
                if (faction.replacesFaction != null)
                {
                    factions.RemoveAll((FactionDef x) => x == faction.replacesFaction);
                }
            }

            initialFactions = new List<FactionDef>();
            initialFactions.AddRange(factions);
            factions.Clear();
        }

        public static FactionDef BestMatch(this IEnumerable<FactionDef> factionDefs,FactionData currentFaction, Func<FactionDef,bool> predicate)
        {
            //Try to find the exact faction saved on the server
            FactionDef factionToReturn = factionDefs.FirstOrDefault(predicate);


            if (factionToReturn == null)
            {
                //try to find a similar faction that is currently generated in the world

                factionToReturn = initialFactions.FirstOrDefault(
                                fetch => (fetch.permanentEnemy == currentFaction.permanentEnemy) &&
                                (fetch.naturalEnemy == currentFaction.naturalEnemy) &&
                                ((byte)fetch.techLevel == currentFaction.techLevel) &&
                                (fetch.hidden == currentFaction.hidden));

                //try to find a similar faction in all factionDefs
                if (factionToReturn == null)
                {
                    factionToReturn = factionDefs.FirstOrDefault(
                        fetch => (fetch.permanentEnemy == currentFaction.permanentEnemy) &&
                                (fetch.naturalEnemy == currentFaction.naturalEnemy) &&
                                ((byte)fetch.techLevel == currentFaction.techLevel) &&
                                (fetch.hidden == currentFaction.hidden));

                    //if a faction cannot be found with similar details, then use the bare minimum faction
                    if (factionToReturn == null)
                    {
                        factionToReturn = DefDatabase<FactionDef>.AllDefs.FirstOrDefault(
                            fetch => (fetch.permanentEnemy == currentFaction.permanentEnemy) &&
                                     (fetch.naturalEnemy == currentFaction.naturalEnemy) &&
                                     (fetch.hidden == currentFaction.hidden));
                    }


                }
            }

            return factionToReturn;
        }

        public static void GeneratePatchedWorld(bool firstGeneration)
        {
            WorldGeneratorManager.firstGeneration = firstGeneration;
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
            List<WorldGenStepDef> worldGenSteps;

            worldGenSteps = WorldGenerationData.RT_WorldGenSteps.ToList();

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
            worldData.worldStepMode = WorldStepMode.Required;

            worldData.seedString = seedString;
            worldData.persistentRandomValue = persistentRandomValue;
            worldData.planetCoverage = planetCoverage.ToString();
            worldData.rainfall = ((int)rainfall).ToString();
            worldData.temperature = ((int)temperature).ToString();
            worldData.population = ((int)population).ToString();
            worldData.pollution = pollution.ToString();

            

            //Save factions
            foreach (Faction faction in Find.World.factionManager.AllFactions)
            {
                if (faction.IsPlayer) continue;
                FactionData factionData = FactionScribeManager.factionToFactionDetails(faction.def);
                factionData.Name = faction.Name;
                factionData.colorFromSpectrum = faction.colorFromSpectrum;
                factionData.neverFlee = faction.neverFlee;
                worldData.factions[faction.def.defName] = Serializer.ConvertObjectToBytes(factionData);
            }

            //save settlements
            List<Settlement> settlementList = Find.WorldObjects.SettlementBases.ToList();
            settlementList.RemoveAll(fetch => fetch.Faction == Find.FactionManager.OfPlayer);
            foreach (Settlement settlement in settlementList)
            {
                SettlementData settlementData = new();
                settlementData.tile = settlement.Tile;
                settlementData.settlementName = settlement.Name;
                settlementData.owner = settlement.Faction.Name;

                worldData.SettlementDatas.Add(Serializer.ConvertObjectToBytes(settlementData));
            }


            XmlParser.GetWorldXmlData(worldData);
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
