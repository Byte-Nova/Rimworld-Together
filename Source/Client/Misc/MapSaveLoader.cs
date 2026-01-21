using GameClient.Defs;
using GameClient.Managers;
using RimWorld;
using Shared.Files;
using Shared.Files.Maps;
using Shared.Misc;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Misc
{
    public static class MapSaveLoader
    {
        public static MapFile MapToString(Map map)
        {
            MapFile mapFile = new MapFile();

            mapFile.Tile = map.Tile;

            mapFile.Size = ValueParser.IntVec3ToArray(map.Size);

            mapFile.Wealth = (int)map.wealthWatcher.WealthTotal;

            mapFile.WeatherByte = (byte)DefDatabase<WeatherDef>.AllDefs.FirstIndexOf(fetch => fetch == map.weatherManager.curWeather);

            GetMapTerrain(mapFile, map);

            GetMapThings(mapFile, map);

            GetMapPawns(mapFile, map);

            return mapFile;
        }

        public static Map StringToMap(MapFile mapFile, bool factionThings, bool nonFactionThings, bool factionPawns, bool nonFactionPawns, 
            bool lessLoot = false, bool enforceIDs = false)
        {
            SetOverrideGenerators();

            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(mapFile.Tile, ValueParser.ArrayToIntVec3(mapFile.Size), null);

            SetMapTerrain(mapFile, map);

            SetMapThings(mapFile, map, factionThings, nonFactionThings, lessLoot, enforceIDs);

            SetMapPawns(mapFile, map, factionPawns, nonFactionPawns, enforceIDs);

            PostGenerationSteps(mapFile, map);

            return map;
        }

        private static void GetMapTerrain(MapFile mapFile, Map map)
        {
            for (int z = 0; z < map.Size.z; ++z)
            {
                for (int x = 0; x < map.Size.x; ++x)
                {
                    try
                    {
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                        MapTile component = new MapTile();

                        TerrainDef terrainDef = map.terrainGrid.TerrainAt(vectorToCheck);
                        if (terrainDef != null) component.TileByte = (byte)DefDatabase<TerrainDef>.AllDefs.FirstIndexOf(fetch => fetch == terrainDef);

                        component.IsPolluted = map.pollutionGrid.IsPolluted(vectorToCheck);

                        RoofDef roofDef = map.roofGrid.RoofAt(vectorToCheck);
                        if (roofDef != null) component.RoofByte = (byte)DefDatabase<RoofDef>.AllDefs.FirstIndexOf(fetch => fetch == roofDef);

                        mapFile.Tiles.Add(component);

                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }
        }

        private static void GetMapThings(MapFile mapFile, Map map)
        {
            Thing[] allThings = map.listerThings.AllThings.Where(fetch => !ScriberH.CheckIfThingIsHuman(fetch) && !ScriberH.CheckIfThingIsAnimal(fetch)).ToArray();
            foreach (Thing thing in allThings)
            {
                try
                {
                    MapThing mapThing = new MapThing();
                    mapThing.ScribeData = ScribeManager.SerializeToString(thing, ScribeManager.SerializableType.Thing);
                    mapThing.IsFromFaction = thing.def.alwaysHaulable;

                    mapFile.Things.Add(mapThing);
                }
                catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
            }
        }

        private static void GetMapPawns(MapFile mapFile, Map map)
        {
            Thing[] allPawns = map.listerThings.AllThings.Where(fetch => ScriberH.CheckIfThingIsHuman(fetch) || ScriberH.CheckIfThingIsAnimal(fetch)).ToArray();
            foreach (Thing pawn in allPawns)
            {
                try
                {
                    MapPawn mapPawn = new MapPawn();
                    mapPawn.ScribeData = ScribeManager.SerializeToString(pawn, ScribeManager.SerializableType.Pawn);
                    mapPawn.IsFromFaction = pawn.Faction == Faction.OfPlayer;

                    mapFile.Pawns.Add(mapPawn);
                }
                catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
            }
        }

        private static void SetMapTerrain(MapFile mapFile, Map map)
        {
            int index = 0;

            for (int z = 0; z < map.Size.z; ++z)
            {
                for (int x = 0; x < map.Size.x; ++x)
                {
                    try
                    {
                        MapTile component = mapFile.Tiles[index];
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                        TerrainDef terrainToUse = DefDatabase<TerrainDef>.AllDefs.ToList()[component.TileByte];
                        map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);
                        map.pollutionGrid.SetPolluted(vectorToCheck, component.IsPolluted);

                        RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.ToList()[component.RoofByte];
                        map.roofGrid.SetRoof(vectorToCheck, roofToUse);

                        index++;
                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetMapThings(MapFile mapFile, Map map, bool faction, bool nonFaction, bool lessLoot, bool enforceIDs)
        {
            List<Thing> tileThings = new List<Thing>();
            Random rnd = new Random();

            if (faction)
            {
                foreach (MapThing mapThing in mapFile.Things.Where(fetch => fetch.IsFromFaction))
                {
                    try
                    {
                        if (lessLoot && rnd.Next(1, 100) <= 70) continue;
                        else
                        {
                            Thing thing = ScribeManager.SerializeFromString<Thing>(mapThing.ScribeData, ScribeManager.SerializableType.Thing, enforceIDs);
                            if (thing.def.CanHaveFaction) thing.SetFaction(SessionHandler.NeutralFaction);
                            RimworldManager.PlaceThingIntoMap(thing, map, thing.Position);
                        }
                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }

            if (nonFaction)
            {
                foreach (MapThing mapThing in mapFile.Things.Where(fetch => !fetch.IsFromFaction))
                {
                    try 
                    {
                        Thing thing = ScribeManager.SerializeFromString<Thing>(mapThing.ScribeData, ScribeManager.SerializableType.Thing, enforceIDs);
                        if (thing.def.CanHaveFaction) thing.SetFaction(SessionHandler.NeutralFaction);
                        RimworldManager.PlaceThingIntoMap(thing, map, thing.Position);
                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetMapPawns(MapFile mapFile, Map map, bool faction, bool nonFaction, bool enforceIDs)
        {
            if (faction)
            {
                foreach (MapPawn mapPawn in mapFile.Pawns.Where(fetch => fetch.IsFromFaction))
                {
                    try
                    {
                        Pawn pawn = ScribeManager.SerializeFromString<Pawn>(mapPawn.ScribeData, ScribeManager.SerializableType.Pawn, enforceIDs);
                        pawn.SetFaction(SessionHandler.NeutralFaction);
                        RimworldManager.PlaceThingIntoMap(pawn, map, pawn.PositionHeld);
                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }

            if (nonFaction)
            {
                foreach (MapPawn mapPawn in mapFile.Pawns.Where(fetch => !fetch.IsFromFaction))
                {
                    try
                    {
                        Pawn pawn = ScribeManager.SerializeFromString<Pawn>(mapPawn.ScribeData, ScribeManager.SerializableType.Pawn, enforceIDs);
                        RimworldManager.PlaceThingIntoMap(pawn, map, pawn.PositionHeld);
                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }
        }

        private static void PostGenerationSteps(MapFile mapFile, Map map)
        {
            try
            {
                map.weatherManager.TransitionTo(DefDatabase<WeatherDef>.AllDefs.ToList()[mapFile.WeatherByte]);

                FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map);

                map.roofCollapseBuffer.Clear();
                map.roofGrid.Drawer.SetDirty();
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        public static void SetOverrideGenerators()
        {
            MapGeneratorDef emptyGenerator = DefDatabase<MapGeneratorDef>.AllDefs.First(fetch => fetch.defName == "Empty");

            WorldObjectDef settlement = RTWorldObjectDefOf.RTSettlement;
            settlement.mapGenerator = emptyGenerator;

            WorldObjectDef site = RTWorldObjectDefOf.RTSite;
            site.mapGenerator = emptyGenerator;
        }
    }
}
