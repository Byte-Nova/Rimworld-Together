using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.Managers;
using RimWorld;
using Shared.Files;
using Shared.Files.Maps;
using Shared.Misc;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Misc
{
    public static class MapSaveLoader
    {
        public static MapFile MapToString(Map map, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, 
            bool factionAnimals, bool nonFactionAnimals)
        {
            MapFile mapFile = new MapFile();

            mapFile.Tile = map.Tile;

            mapFile.Size = ValueParser.IntVec3ToArray(map.Size);

            mapFile.Wealth = (int)map.wealthWatcher.WealthTotal;

            mapFile.WeatherByte = (byte)DefDatabase<WeatherDef>.AllDefs.FirstIndexOf(fetch => fetch == map.weatherManager.curWeather);

            mapFile.Mods = ModManagerH.GetRunningModList();

            GetMapTerrain(mapFile, map);

            GetMapThings(mapFile, map, factionThings, nonFactionThings);

            GetMapHumans(mapFile, map, factionHumans, nonFactionHumans);

            GetMapAnimals(mapFile, map, factionAnimals, nonFactionAnimals);

            return mapFile;
        }

        public static Map StringToMap(MapFile mapFile, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, 
            bool factionAnimals, bool nonFactionAnimals, bool lessLoot = false)
        {
            Map map = SetEmptyMap(mapFile, SessionHandler.ChosenSettlement.Tile);

            SetMapTerrain(mapFile, map);

            if (factionThings || nonFactionThings) SetMapThings(mapFile, map, factionThings, nonFactionThings, lessLoot);

            if (factionHumans || nonFactionHumans) SetMapHumans(mapFile, map, factionHumans, nonFactionHumans);

            if (factionAnimals || nonFactionAnimals) SetMapAnimals(mapFile, map, factionAnimals, nonFactionAnimals);

            SetWeatherData(mapFile, map);

            SetMapFog(map);

            SetMapRoofs(map);

            return map;
        }

        private static void GetMapTerrain(MapFile mapFile, Map map)
        {
            try
            {
                for (int z = 0; z < map.Size.z; ++z)
                {
                    for (int x = 0; x < map.Size.x; ++x)
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
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapThings(MapFile mapFile, Map map, bool factionThings, bool nonFactionThings)
        {
            foreach (Thing thing in map.listerThings.AllThings.Where(fetch => !ScriberH.CheckIfThingIsHuman(fetch) && !ScriberH.CheckIfThingIsAnimal(fetch)))
            {
                try
                {
                    string data = ScribeManager.SerializeToString(thing, ScribeManager.SerializableType.Thing, thing.stackCount);
                    if (thing.def.alwaysHaulable && factionThings) mapFile.FactionThings.Add(data);
                    else if (!thing.def.alwaysHaulable && nonFactionThings) mapFile.NonFactionThings.Add(data);
                }
                catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
            }
        }

        private static void GetMapHumans(MapFile mapFile, Map map, bool factionHumans, bool nonFactionHumans)
        {
            foreach (Thing thing in map.listerThings.AllThings.Where(fetch => ScriberH.CheckIfThingIsHuman(fetch)))
            {
                try
                {
                    HumanFile humanData = ScribeManager.HumanToString(thing as Pawn);
                    if (thing.Faction == Faction.OfPlayer && factionHumans) mapFile.FactionHumans.Add(humanData);
                    else if (thing.Faction != Faction.OfPlayer && nonFactionHumans) mapFile.NonFactionHumans.Add(humanData);
                }
                catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
            }
        }

        private static void GetMapAnimals(MapFile mapFile, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            foreach (Thing thing in map.listerThings.AllThings.Where(fetch => ScriberH.CheckIfThingIsAnimal(fetch)))
            {
                try
                {
                    string animalData = ScribeManager.SerializeToString(thing as Pawn, ScribeManager.SerializableType.Thing);
                    if (thing.Faction == Faction.OfPlayer && factionAnimals) mapFile.FactionAnimals.Add(animalData);
                    else if (thing.Faction != Faction.OfPlayer && nonFactionAnimals) mapFile.NonFactionAnimals.Add(animalData);
                }
                catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
            }
        }

        private static Map SetEmptyMap(MapFile mapFile, int tileToUse)
        {
            Map toReturn = null;

            try
            {
                IntVec3 mapSize = ValueParser.ArrayToIntVec3(mapFile.Size);

                PlanetManagerHelper.SetOverrideGenerators();
                toReturn = GetOrGenerateMapUtility.GetOrGenerateMap(tileToUse, mapSize, null);
                PlanetManagerHelper.SetDefaultGenerators();

                return toReturn;
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }

            return toReturn;
        }

        private static void SetMapTerrain(MapFile mapFile, Map map)
        {
            try
            {
                int index = 0;

                for (int z = 0; z < map.Size.z; ++z)
                {
                    for (int x = 0; x < map.Size.x; ++x)
                    {
                        MapTile component = mapFile.Tiles[index];
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                        try
                        {
                            TerrainDef terrainToUse = DefDatabase<TerrainDef>.AllDefs.ToList()[component.TileByte];
                            map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);
                            map.pollutionGrid.SetPolluted(vectorToCheck, component.IsPolluted);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }

                        try
                        {
                            RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.ToList()[component.RoofByte];
                            map.roofGrid.SetRoof(vectorToCheck, roofToUse);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }

                        index++;
                    }
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapThings(MapFile mapFile, Map map, bool factionThings, bool nonFactionThings, bool lessLoot)
        {
            try
            {
                List<Thing> thingsToGetInThisTile = new List<Thing>();

                if (factionThings)
                {
                    Random rnd = new Random();

                    foreach (string item in mapFile.FactionThings)
                    {
                        try
                        {
                            Thing toGet = (Thing)ScribeManager.SerializeFromString<Thing>(item);

                            if (lessLoot)
                            {
                                if (rnd.Next(1, 100) > 70) thingsToGetInThisTile.Add(toGet);
                                else continue;
                            }
                            else thingsToGetInThisTile.Add(toGet);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }

                if (nonFactionThings)
                {
                    foreach (string item in mapFile.NonFactionThings)
                    {
                        try
                        {
                            Thing toGet = (Thing)ScribeManager.SerializeFromString<Thing>(item);
                            thingsToGetInThisTile.Add(toGet);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }

                foreach (Thing thing in thingsToGetInThisTile)
                {
                    try
                    {
                        if (thing.def.CanHaveFaction) thing.SetFaction(SessionHandler.NeutralFaction);
                        GenPlace.TryPlaceThing(thing, thing.Position, map, ThingPlaceMode.Direct, rot: thing.Rotation);
                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapHumans(MapFile mapFile, Map map, bool factionHumans, bool nonFactionHumans)
        {
            try
            {
                if (factionHumans)
                {
                    foreach (HumanFile pawn in mapFile.FactionHumans)
                    {
                        try
                        {
                            Pawn human = ScribeManager.StringtoHuman(pawn);
                            human.SetFaction(SessionHandler.NeutralFaction);

                            GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }

                if (nonFactionHumans)
                {
                    foreach (HumanFile pawn in mapFile.NonFactionHumans)
                    {
                        try
                        {
                            Pawn human = ScribeManager.StringtoHuman(pawn);
                            GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapAnimals(MapFile mapFile, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            try
            {
                if (factionAnimals)
                {
                    foreach (string pawn in mapFile.FactionAnimals)
                    {
                        try
                        {
                            Pawn animal = (Pawn)ScribeManager.SerializeFromString<Pawn>(pawn);
                            animal.SetFaction(SessionHandler.NeutralFaction);

                            GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }

                if (nonFactionAnimals)
                {
                    foreach (string pawn in mapFile.NonFactionAnimals)
                    {
                        try
                        {
                            Pawn animal = (Pawn)ScribeManager.SerializeFromString<Pawn>(pawn);
                            GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetWeatherData(MapFile mapFile, Map map)
        {
            try
            {
                WeatherDef weatherDef = DefDatabase<WeatherDef>.AllDefs.ToList()[mapFile.WeatherByte];
                map.weatherManager.TransitionTo(weatherDef);
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapFog(Map map)
        {
            try { FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map); }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapRoofs(Map map)
        {
            try
            {
                map.roofCollapseBuffer.Clear();
                map.roofGrid.Drawer.SetDirty();
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }
    }
}
