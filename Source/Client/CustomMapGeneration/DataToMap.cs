using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Shared;

namespace GameClient
{
    public static class DataToMap
    {
        public static Map mapToTransferDataTo;
        public static MapDetailsJSON mapDetails;
        public static bool SpawnItems;
        public static bool SpawnHumans;
        public static bool SpawnAnimals;
        public static bool lessSettlementLoot;

        public static IntVec3 GetMapSize(MapDetailsJSON mapDetailsJSON)
        {
            
            //splits the mapSize string into 3 parts and stores each part
            //in a string array
            string[] splitSize = mapDetailsJSON.mapSize.Split('|');

            //turn the string array into an IntVec3
            IntVec3 mapSize = new IntVec3(int.Parse(splitSize[0]), 
                                          int.Parse(splitSize[1]),
                                          int.Parse(splitSize[2]));

            return mapSize;
        }

        public static void addEverythingToMap(Map map, MapDetailsJSON mapDetailsJSON = null)
        {
            mapDetailsJSON = (mapDetailsJSON == null) ? mapDetails : mapDetailsJSON;
            map = (map == null) ? mapToTransferDataTo : map;
            try
            {
                DataToMap.addCaravanThingsToMap(map, mapDetailsJSON);
                DataToMap.addSettlementThingsToMap(map, mapDetailsJSON);
                DataToMap.addHumansToMap(map, mapDetailsJSON);
                DataToMap.addCaravanAnimalsToMap(map, mapDetailsJSON);
                DataToMap.addSettlementAnimalsToMap(map, mapDetailsJSON);
                DataToMap.addTerrainAndRoofingToMap(map, mapDetailsJSON);
            }catch(Exception e)
            {
                Logs.Message(e.ToString());
            }
        }

        public static void addCaravanThingsToMap(Map map, MapDetailsJSON mapDetailsJSON = null)
        {
            //stopwatch for debugging
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            List<Thing> thingsToGetInThisTile = new List<Thing>();

            //add all the items to a list of "Things" in the caravan
            foreach (string str in mapDetailsJSON.itemDetailsJSONS)
            {
                try
                {
                    Thing toGet = DeepScribeManager.GetItemSimple(Serializer.SerializeFromString<ItemDetailsJSON>(str));
                    thingsToGetInThisTile.Add(toGet);
                }
                catch { }
            }

            //add caravan items to the map
            foreach (Thing thing in thingsToGetInThisTile)
            {
                try { GenPlace.TryPlaceThing(thing, thing.Position, RT_MapGenerator.mapBeingGenerated, ThingPlaceMode.Direct, rot: thing.Rotation); }
                catch { Logs.Warning($"Failed to place thing {thing.def.defName} at {thing.Position}"); }
            }

            stopWatch.Stop();
            Logs.Message($"{"adding caravan items to Map took",-40} {stopWatch.ElapsedMilliseconds,-10} ms");
            stopWatch.Reset();
        }

        //      Map             : the map to add items to
        //      hideLoot        : set true if some loot should become invisible (for scenarios like raids) 
        //      MapDetailsJSON  : MapDetailsJson file to grab data from
        public static void addSettlementThingsToMap(Map map, MapDetailsJSON mapDetailsJSON = null)
        {
            //stopwatch for debugging
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            List<Thing> thingsToGetInThisTile = new List<Thing>();
            Random rnd = new Random();

            foreach (string str in mapDetailsJSON.playerItemDetailsJSON)
            {
                try
                {
                    Thing toGet = DeepScribeManager.GetItemSimple(Serializer.SerializeFromString<ItemDetailsJSON>(str));

                    //if lessLoot is true, Some items will not be generated
                    if (lessSettlementLoot)
                    {
                        if (rnd.Next(1, 100) > 70) thingsToGetInThisTile.Add(toGet);
                        else continue;
                    }
                    else thingsToGetInThisTile.Add(toGet);
                }
                catch { }
            }

            //add Settlement items to the map
            foreach (Thing thing in thingsToGetInThisTile)
            {
                try { GenPlace.TryPlaceThing(thing, thing.Position, RT_MapGenerator.mapBeingGenerated, ThingPlaceMode.Direct, rot: thing.Rotation); }
                catch { Logs.Warning($"Failed to place thing {thing.def.defName} at {thing.Position}"); }
            }

            stopWatch.Stop();
            Logs.Message($"{"adding Settlement items to Map took",-40} {stopWatch.ElapsedMilliseconds,-10} ms");
            
        }

        //      Map             : the map to add items to
        //      MapDetailsJSON  : MapDetailsJson file to grab data from
        public static void addHumansToMap(Map map,MapDetailsJSON mapDetailsJSON)
        {
            //stopwatch for debugging
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            foreach (string str in mapDetailsJSON.humanDetailsJSONS)
            {
                HumanDetailsJSON humanDetailsJSON = Serializer.SerializeFromString<HumanDetailsJSON>(str);

                try
                {
                    Pawn human = DeepScribeManager.GetHumanSimple(humanDetailsJSON);
                    human.SetFaction(FactionValues.yourOnlineFaction);

                    GenSpawn.Spawn(human, human.Position, map, Rot4.Random);
                }
                catch { Logs.Warning($"Failed to spawn human {humanDetailsJSON.name}"); }
            }

            foreach (string str in mapDetailsJSON.playerHumanDetailsJSON)
            {
                HumanDetailsJSON humanDetailsJSON = Serializer.SerializeFromString<HumanDetailsJSON>(str);

                try
                {
                    Pawn human = DeepScribeManager.GetHumanSimple(humanDetailsJSON);
                    human.SetFaction(FactionValues.neutralPlayer);

                    GenSpawn.Spawn(human, human.Position, map, Rot4.Random);
                }
                catch { Logs.Warning($"Failed to spawn human {humanDetailsJSON.name}"); }
            }

            stopWatch.Stop();
            Logs.Message($"{"Spawning items took",-40} {stopWatch.ElapsedMilliseconds,-10} ms");
            

        }


        //      Map             : the map to add items to
        //      MapDetailsJSON  : MapDetailsJson file to grab data from
        public static void addCaravanAnimalsToMap(Map map, MapDetailsJSON mapDetailsJSON)
        {
            //stopwatch for debugging
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            //Spawn Each Caravan animal from animalDetailsJSON in mapDetailsJSON
            foreach (string str in mapDetailsJSON.animalDetailsJSON)
            {
                AnimalDetailsJSON animalDetailsJSON = Serializer.SerializeFromString<AnimalDetailsJSON>(str);

                try
                {
                    Pawn animal = DeepScribeManager.GetAnimalSimple(animalDetailsJSON);
                    animal.SetFaction(FactionValues.yourOnlineFaction);

                    GenSpawn.Spawn(animal, animal.Position, map, Rot4.Random);
                }
                catch { Logs.Warning($"Failed to spawn animal {animalDetailsJSON.name}"); }
            }

            
            stopWatch.Stop();
            Logs.Message($"{"spawning Caravan animals to Map took",-40} {stopWatch.ElapsedMilliseconds,-10} ms");

        }

        public static void addSettlementAnimalsToMap(Map map, MapDetailsJSON mapDetailsJSON)
        {
            //stopwatch for debugging
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            //Spawn Each Settlement animal from playerAnimalDetailsJSON in mapDetailsJSON
            foreach (string str in mapDetailsJSON.playerAnimalDetailsJSON)
            {
                AnimalDetailsJSON animalDetailsJSON = Serializer.SerializeFromString<AnimalDetailsJSON>(str);

                try
                {
                    Pawn animal = DeepScribeManager.GetAnimalSimple(animalDetailsJSON);
                    animal.SetFaction(FactionValues.neutralPlayer);

                    GenSpawn.Spawn(animal, animal.Position, map, Rot4.Random);
                }
                catch { Logs.Warning($"Failed to spawn animal {animalDetailsJSON.name}"); }
            }

            stopWatch.Stop();
            Logs.Message($"{"spawning Settlement animals to Map took",-40} {stopWatch.ElapsedMilliseconds,-10} ms");


        }

        public static void addTerrainAndRoofingToMap(Map map, MapDetailsJSON mapDetailsJSON)
        {
            //stopwatch for debugging
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            //add terrain and roofing to the map
            int index = 0;
            for (int z = 0; z < map.Size.z; ++z)
            {
                for (int x = 0; x < map.Size.x; ++x)
                {
                    IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                    //get and place terrain
                    try
                    {
                        TerrainDef terrainToUse = DefDatabase<TerrainDef>.AllDefs.ToList().Find(fetch => fetch.defName ==
                            mapDetailsJSON.tileDefNames[index]);
                        map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);

                    }
                    catch { Logs.Warning($"Failed to set terrain at {vectorToCheck}"); }

                    //get and place roofs
                    try
                    {
                        RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.ToList().Find(fetch => fetch.defName ==
                                    mapDetailsJSON.roofDefNames[index]);

                        map.roofGrid.SetRoof(vectorToCheck, roofToUse);
                    }
                    catch { Logs.Warning($"Failed to set roof at {vectorToCheck}"); }

                    index++;
                }
            }
            map.roofCollapseBuffer.Clear();
            map.roofGrid.Drawer.SetDirty();

            stopWatch.Stop();
            Logs.Message($"{"Adding Terrain and Roofing took",-40} {stopWatch.ElapsedMilliseconds,-10} ms");


        }

    }
}
