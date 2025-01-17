
using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using RimWorld;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Scribers
{
    public static class MapScriber
    {
        //Functions

        public static MapFile MapToString(Map map, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, bool factionAnimals, bool nonFactionAnimals)
        {
            MapFile mapFile = new MapFile();

            GetMapTile(mapFile, map);

            GetMapSize(mapFile, map);

            GetMapTerrain(mapFile, map);

            GetMapThings(mapFile, map, factionThings, nonFactionThings);

            GetMapHumans(mapFile, map, factionHumans, nonFactionHumans);

            GetMapAnimals(mapFile, map, factionAnimals, nonFactionAnimals);

            GetMapWeather(mapFile, map);

            GetMapMods(mapFile);

            return mapFile;
        }

        public static Map StringToMap(MapFile mapFile, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, bool factionAnimals, bool nonFactionAnimals, bool lessLoot = false, bool overrideID = false)
        {
            Map map = SetEmptyMap(mapFile);

            SetMapTerrain(mapFile, map);

            if (factionThings || nonFactionThings) SetMapThings(mapFile, map, factionThings, nonFactionThings, lessLoot, overrideID);

            if (factionHumans || nonFactionHumans) SetMapHumans(mapFile, map, factionHumans, nonFactionHumans, overrideID);

            if (factionAnimals || nonFactionAnimals) SetMapAnimals(mapFile, map, factionAnimals, nonFactionAnimals, overrideID);

            SetWeatherData(mapFile, map);

            SetMapFog(map);

            SetMapRoofs(map);

            return map;
        }

        //Getters

        private static void GetMapTile(MapFile mapFile, Map map)
        {
            try { mapFile.Tile = map.Tile; }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapSize(MapFile mapFile, Map map)
        {
            try { mapFile.Size = ValueParser.IntVec3ToArray(map.Size); }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapTerrain(MapFile mapFile, Map map)
        {
            try
            {
                List<TileComponent> toGet = new List<TileComponent>();

                for (int z = 0; z < map.Size.z; ++z)
                {
                    for (int x = 0; x < map.Size.x; ++x)
                    {
                        TileComponent component = new TileComponent();
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);
                        component.DefName = map.terrainGrid.TerrainAt(vectorToCheck).defName;
                        component.IsPolluted = map.pollutionGrid.IsPolluted(vectorToCheck);

                        if (map.roofGrid.RoofAt(vectorToCheck) == null) component.RoofDefName = "null";
                        else component.RoofDefName = map.roofGrid.RoofAt(vectorToCheck).defName;

                        toGet.Add(component);
                    }
                }

                mapFile.Tiles = toGet.ToArray();
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapThings(MapFile mapFile, Map map, bool factionThings, bool nonFactionThings)
        {
            try
            {
                List<ThingFile> tempFactionThings = new List<ThingFile>();
                List<ThingFile> tempNonFactionThings = new List<ThingFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (!ScriberH.CheckIfThingIsHuman(thing) && !ScriberH.CheckIfThingIsAnimal(thing))
                    {
                        ThingFile thingData = ThingScriber.ThingToString(thing, thing.stackCount);

                        if (thing.def.alwaysHaulable && factionThings) tempFactionThings.Add(thingData);
                        else if (!thing.def.alwaysHaulable && nonFactionThings) tempNonFactionThings.Add(thingData);
                    }
                }

                mapFile.FactionThings = tempFactionThings.ToArray();
                mapFile.NonFactionThings = tempNonFactionThings.ToArray();
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapHumans(MapFile mapFile, Map map, bool factionHumans, bool nonFactionHumans)
        {
            try
            {
                List<HumanFile> tempFactionHumans = new List<HumanFile>();
                List<HumanFile> tempNonFactionHumans = new List<HumanFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (ScriberH.CheckIfThingIsHuman(thing))
                    {
                        HumanFile humanData = HumanScriber.HumanToString(thing as Pawn);

                        if (thing.Faction == Faction.OfPlayer && factionHumans) tempFactionHumans.Add(humanData);
                        else if (thing.Faction != Faction.OfPlayer && nonFactionHumans) tempNonFactionHumans.Add(humanData);
                    }
                }

                mapFile.FactionHumans = tempFactionHumans.ToArray();
                mapFile.NonFactionHumans = tempNonFactionHumans.ToArray();
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapAnimals(MapFile mapFile, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            try
            {
                List<AnimalFile> tempFactionAnimals = new List<AnimalFile>();
                List<AnimalFile> tempNonFactionAnimals = new List<AnimalFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (ScriberH.CheckIfThingIsAnimal(thing))
                    {
                        AnimalFile animalData = AnimalScriber.AnimalToString(thing as Pawn);

                        if (thing.Faction == Faction.OfPlayer && factionAnimals) tempFactionAnimals.Add(animalData);
                        else if (thing.Faction != Faction.OfPlayer && nonFactionAnimals) tempNonFactionAnimals.Add(animalData);
                    }
                }

                mapFile.FactionAnimals = tempFactionAnimals.ToArray();
                mapFile.NonFactionAnimals = tempNonFactionAnimals.ToArray();
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapWeather(MapFile mapFile, Map map)
        {
            try { mapFile.CurWeatherDefName = map.weatherManager.curWeather.defName; }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void GetMapMods(MapFile mapFile)
        {
            try { mapFile.Mods = ModManagerHelper.GetRunningModList(); }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        //Setters

        private static Map SetEmptyMap(MapFile mapFile)
        {
            Map toReturn = null;

            try
            {
                IntVec3 mapSize = ValueParser.ArrayToIntVec3(mapFile.Size);

                PlanetManagerHelper.SetOverrideGenerators();
                toReturn = GetOrGenerateMapUtility.GetOrGenerateMap(SessionValues.chosenSettlement.Tile, mapSize, null);
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
                        TileComponent component = mapFile.Tiles[index];
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                        try
                        {
                            TerrainDef terrainToUse = DefDatabase<TerrainDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                            map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);
                            map.pollutionGrid.SetPolluted(vectorToCheck, component.IsPolluted);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }

                        try
                        {
                            RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.RoofDefName);
                            map.roofGrid.SetRoof(vectorToCheck, roofToUse);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }

                        index++;
                    }
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapThings(MapFile mapFile, Map map, bool factionThings, bool nonFactionThings, bool lessLoot, bool overrideID)
        {
            try
            {
                List<Thing> thingsToGetInThisTile = new List<Thing>();

                if (factionThings)
                {
                    Random rnd = new Random();

                    foreach (ThingFile item in mapFile.FactionThings)
                    {
                        try
                        {
                            Thing toGet = ThingScriber.StringToThing(item, overrideID);

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
                    foreach (ThingFile item in mapFile.NonFactionThings)
                    {
                        try
                        {
                            Thing toGet = ThingScriber.StringToThing(item, overrideID);
                            thingsToGetInThisTile.Add(toGet);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }

                foreach (Thing thing in thingsToGetInThisTile)
                {
                    try
                    {
                        if (thing.def.CanHaveFaction) thing.SetFaction(FactionValues.neutralPlayer);
                        GenPlace.TryPlaceThing(thing, thing.Position, map, ThingPlaceMode.Direct, rot: thing.Rotation);
                    }
                    catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapHumans(MapFile mapFile, Map map, bool factionHumans, bool nonFactionHumans, bool overrideID)
        {
            try
            {
                if (factionHumans)
                {
                    foreach (HumanFile pawn in mapFile.FactionHumans)
                    {
                        try
                        {
                            Pawn human = HumanScriber.StringtoHuman(pawn, overrideID);
                            human.SetFaction(FactionValues.neutralPlayer);

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
                            Pawn human = HumanScriber.StringtoHuman(pawn, overrideID);
                            GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }
            }
            catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
        }

        private static void SetMapAnimals(MapFile mapFile, Map map, bool factionAnimals, bool nonFactionAnimals, bool overrideID)
        {
            try
            {
                if (factionAnimals)
                {
                    foreach (AnimalFile pawn in mapFile.FactionAnimals)
                    {
                        try
                        {
                            Pawn animal = AnimalScriber.StringToAnimal(pawn, overrideID);
                            animal.SetFaction(FactionValues.neutralPlayer);

                            GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                        }
                        catch (Exception e) { Printer.Warning(e.ToString(), LogImportanceMode.Verbose); }
                    }
                }

                if (nonFactionAnimals)
                {
                    foreach (AnimalFile pawn in mapFile.NonFactionAnimals)
                    {
                        try
                        {
                            Pawn animal = AnimalScriber.StringToAnimal(pawn, overrideID);
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
                WeatherDef weatherDef = DefDatabase<WeatherDef>.AllDefs.First(fetch => fetch.defName == mapFile.CurWeatherDefName);
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
