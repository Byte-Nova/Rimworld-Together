using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.Profile;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class WorldGeneratorManager
    {
        public static WorldValuesFile cachedWorldValues;

        private static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      orderby x.order, x.index
                                                                      select x;

        public static void SetValuesFromGame(string seedString, float planetCoverage, OverallRainfall rainfall, OverallTemperature temperature, OverallPopulation population, List<FactionDef> factions, float pollution)
        {
            cachedWorldValues = new WorldValuesFile();
            cachedWorldValues.SeedString = seedString;
            cachedWorldValues.PersistentRandomValue = GenText.StableStringHash(seedString);
            cachedWorldValues.PlanetCoverage = planetCoverage;
            cachedWorldValues.Rainfall = (int)rainfall;
            cachedWorldValues.Temperature = (int)temperature;
            cachedWorldValues.Population = (int)population;
            cachedWorldValues.Pollution = pollution;
            cachedWorldValues.NPCFactionDefNames = WorldGeneratorHelper.GetDefNamesFromFactionDefs(factions.ToArray());
        }

        public static void SetValuesFromServer(WorldData worldData) { cachedWorldValues = worldData.worldValuesFile; }

        public static void GeneratePatchedWorld()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Find.GameInitData.ResetWorldRelatedMapInitData();
                Current.Game.World = GenerateWorld();
                LongEventHandler.ExecuteWhenFinished(delegate 
                {
                    Find.World.renderer.RegenerateAllLayersNow();
                    MemoryUtility.UnloadUnusedUnityAssets();
                    Current.CreatingWorld = null;
                    PostWorldGeneration();
                });
            }, "GeneratingWorld", doAsynchronously: true, null);
        }

        private static World GenerateWorld()
        {
            Rand.PushState(cachedWorldValues.PersistentRandomValue);

            Current.CreatingWorld = new World();
            Current.CreatingWorld.info.seedString = cachedWorldValues.SeedString;
            Current.CreatingWorld.info.persistentRandomValue = cachedWorldValues.PersistentRandomValue;
            Current.CreatingWorld.info.planetCoverage = cachedWorldValues.PlanetCoverage;
            Current.CreatingWorld.info.overallRainfall = (OverallRainfall)cachedWorldValues.Rainfall;
            Current.CreatingWorld.info.overallTemperature = (OverallTemperature)cachedWorldValues.Temperature;
            Current.CreatingWorld.info.overallPopulation = (OverallPopulation)cachedWorldValues.Population;
            Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld);
            Current.CreatingWorld.info.factions = WorldGeneratorHelper.GetFactionDefsFromDefNames(cachedWorldValues.NPCFactionDefNames).ToList();
            Current.CreatingWorld.info.pollution = cachedWorldValues.Pollution;

            WorldGenStepDef[] worldGenSteps = GenStepsInOrder.ToArray();
            for (int i = 0; i < worldGenSteps.Count(); i++) worldGenSteps[i].worldGenStep.GenerateFresh(cachedWorldValues.SeedString);

            Current.CreatingWorld.grid.StandardizeTileData();
            Current.CreatingWorld.FinalizeInit();
            Find.Scenario.PostWorldGenerate();

            if (!ModsConfig.IdeologyActive) Find.Scenario.PostIdeoChosen();
            return Current.CreatingWorld;
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

        public static void SendWorldToServer()
        {
            WorldData worldData = new WorldData();
            worldData.worldStepMode = WorldStepMode.Required;
            worldData.worldValuesFile = WorldGeneratorHelper.PopulateWorldValues();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            Network.listener.EnqueuePacket(packet);
        }
    }

    public static class WorldGeneratorHelper
    {
        public static WorldValuesFile PopulateWorldValues()
        {
            WorldGeneratorManager.cachedWorldValues.NPCSettlements = GetWorldSettlements();
            return WorldGeneratorManager.cachedWorldValues;
        }

        public static string[] GetDefNamesFromFactionDefs(FactionDef[] factions)
        {
            List<string> defList = new List<string>();
            foreach (FactionDef def in factions) defList.Add(def.defName);
            return defList.ToArray();
        }

        public static FactionDef[] GetFactionDefsFromDefNames(string[] defNames)
        {
            List<FactionDef> defList = new List<FactionDef>();
            foreach (string str in defNames)
            {
                FactionDef toFind = DefDatabase<FactionDef>.AllDefs.ToArray().FirstOrDefault(fetch => fetch.defName == str);
                if (toFind != null) defList.Add(toFind);
            }
            return defList.ToArray();
        }

        private static WorldAISettlement[] GetWorldSettlements()
        {
            FactionDef[] worldFactionDefs = GetFactionDefsFromDefNames(WorldGeneratorManager.cachedWorldValues.NPCFactionDefNames);
            List<WorldAISettlement> npcSettlements = new List<WorldAISettlement>();
            foreach (Settlement settlement in Find.World.worldObjects.Settlements.Where(fetch => worldFactionDefs.Contains(fetch.Faction.def)))
            {
                WorldAISettlement worldAISettlement = new WorldAISettlement();
                worldAISettlement.tile = settlement.Tile;
                worldAISettlement.factionDefName = settlement.Faction.def.defName;
                worldAISettlement.name = settlement.Name;

                npcSettlements.Add(worldAISettlement);
            }
            return npcSettlements.ToArray();
        }
    }
}
