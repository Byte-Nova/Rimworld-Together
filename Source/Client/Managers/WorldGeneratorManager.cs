using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using UnityEngine;
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
            cachedWorldValues.NPCFactions = WorldGeneratorHelper.GetNPCFactionsFromDef(factions.ToArray());
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
            Current.CreatingWorld.info.factions = WorldGeneratorHelper.GetFactionDefsFromNPCFaction(cachedWorldValues.NPCFactions).ToList();
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
            WorldGeneratorManager.cachedWorldValues.NPCSettlements = GetPlanetNPCSettlements();
            WorldGeneratorManager.cachedWorldValues.NPCFactions = GetPlanetNPCFactions();
            WorldGeneratorManager.cachedWorldValues.Features = GetPlanetFeatures();
            return WorldGeneratorManager.cachedWorldValues;
        }

        public static PlanetNPCFaction[] GetNPCFactionsFromDef(FactionDef[] factionDefs)
        {
            List<PlanetNPCFaction> npcFactions = new List<PlanetNPCFaction>();
            foreach (FactionDef faction in factionDefs)
            {
                PlanetNPCFaction toCreate = new PlanetNPCFaction();
                toCreate.factionDefName = faction.defName;
                npcFactions.Add(toCreate);
            }
            return npcFactions.ToArray();
        }

        public static FactionDef[] GetFactionDefsFromNPCFaction(PlanetNPCFaction[] factions)
        {
            List<FactionDef> defList = new List<FactionDef>();
            foreach (PlanetNPCFaction faction in factions)
            {
                FactionDef toFind = DefDatabase<FactionDef>.AllDefs.ToArray().FirstOrDefault(fetch => fetch.defName == faction.factionDefName);
                if (toFind != null) defList.Add(toFind);
            }
            return defList.ToArray();
        }

        public static PlanetNPCFaction[] GetPlanetNPCFactions()
        {
            List<PlanetNPCFaction> planetFactions = new List<PlanetNPCFaction>();
            Faction[] existingFactions = Find.World.factionManager.AllFactions.ToArray();

            foreach(Faction faction in existingFactions)
            {
                if (faction == Faction.OfPlayer) continue;
                else
                {
                    PlanetNPCFaction planetFaction = new PlanetNPCFaction();
                    planetFaction.factionDefName = faction.def.defName;
                    planetFaction.factionName = faction.Name;
                    planetFaction.factionColor = new float[] { faction.Color.r, faction.Color.g, faction.Color.b, faction.Color.a };

                    planetFactions.Add(planetFaction);
                }
            }

            return planetFactions.ToArray();
        }

        private static PlanetNPCSettlement[] GetPlanetNPCSettlements()
        {
            FactionDef[] worldFactionDefs = GetFactionDefsFromNPCFaction(WorldGeneratorManager.cachedWorldValues.NPCFactions);
            List<PlanetNPCSettlement> npcSettlements = new List<PlanetNPCSettlement>();
            foreach (Settlement settlement in Find.World.worldObjects.Settlements.Where(fetch => worldFactionDefs.Contains(fetch.Faction.def)))
            {
                PlanetNPCSettlement PlanetNPCSettlement = new PlanetNPCSettlement();
                PlanetNPCSettlement.tile = settlement.Tile;
                PlanetNPCSettlement.factionDefName = settlement.Faction.def.defName;
                PlanetNPCSettlement.name = settlement.Name;

                npcSettlements.Add(PlanetNPCSettlement);
            }
            return npcSettlements.ToArray();
        }

        private static PlanetFeature[] GetPlanetFeatures()
        {
            List<PlanetFeature> planetFeatures = new List<PlanetFeature>();
            WorldFeature[] worldFeatures = Find.World.features.features.ToArray();
            foreach (WorldFeature worldFeature in worldFeatures)
            {
                PlanetFeature planetFeature = new PlanetFeature();
                planetFeature.featureName = worldFeature.name;
                planetFeature.defName = worldFeature.def.defName;
                planetFeature.maxDrawSizeInTiles = worldFeature.maxDrawSizeInTiles;
                planetFeature.drawCenter = new float[] { worldFeature.drawCenter.x, worldFeature.drawCenter.y, worldFeature.drawCenter.z };

                planetFeatures.Add(planetFeature);
            }

            return planetFeatures.ToArray();
        }

        public static void SetPlanetFeatures()
        {
            WorldFeature[] worldFeatures = Find.World.features.features.ToArray();
            foreach (WorldFeature feature in worldFeatures) Find.World.features.features.Remove(feature);

            PlanetFeature[] planetFeatures = WorldGeneratorManager.cachedWorldValues.Features.ToArray();
            foreach (PlanetFeature planetFeature in planetFeatures)
            {
                WorldFeature worldFeature = new WorldFeature();

                FeatureDef toGet = DefDatabase<FeatureDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == planetFeature.defName);
                if (toGet == null) continue;
                else worldFeature.def = toGet;

                worldFeature.name = planetFeature.featureName;
                worldFeature.maxDrawSizeInTiles = planetFeature.maxDrawSizeInTiles;
                worldFeature.drawCenter = new Vector3(planetFeature.drawCenter[0], planetFeature.drawCenter[1], planetFeature.drawCenter[2]);

                Find.World.features.features.Add(worldFeature);
            }

            Find.World.features.textsCreated = false;
            Find.World.features.UpdateFeatures();
        }

        public static void SetPlanetFactions()
        {
            Faction[] planetFactions = Find.World.factionManager.AllFactions.ToArray();
            foreach(PlanetNPCFaction faction in WorldGeneratorManager.cachedWorldValues.NPCFactions)
            {
                Faction toModify = Find.World.factionManager.AllFactions.FirstOrDefault(fetch => fetch.def.defName == faction.factionDefName);
                if (toModify == null) continue;
                else
                {
                    toModify.Name = faction.factionName;

                    toModify.color = new Color(faction.factionColor[0], 
                        faction.factionColor[1], 
                        faction.factionColor[2], 
                        faction.factionColor[3]);
                }
            }
        }
    }
}
