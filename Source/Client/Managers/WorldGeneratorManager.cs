using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RimWorld;
using RimWorld.Planet;
using Shared;
using UnityEngine;
using Verse;
using Verse.Profile;
using static Shared.CommonEnumerators;
using static UnityEngine.GraphicsBuffer;

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
        }

        public static void SendWorldToServer()
        {
            WorldData worldData = new WorldData();
            worldData.worldStepMode = WorldStepMode.Required;
            worldData.worldValuesFile = WorldGeneratorHelper.PopulateWorldValues();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldPacket), worldData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void SetPlanetFeatures()
        {
            WorldFeature[] worldFeatures = Find.WorldFeatures.features.ToArray();
            foreach (WorldFeature feature in worldFeatures) Find.WorldFeatures.features.Remove(feature);

            PlanetFeature[] planetFeatures = cachedWorldValues.Features.ToArray();
            for (int i = 0; i < planetFeatures.Length; i++)
            {
                PlanetFeature planetFeature = planetFeatures[i];

                try
                {
                    WorldFeature worldFeature = new WorldFeature();
                    worldFeature.def = DefDatabase<FeatureDef>.AllDefs.First(fetch => fetch.defName == planetFeature.defName);
                    worldFeature.uniqueID = i;
                    worldFeature.name = planetFeature.featureName;
                    worldFeature.maxDrawSizeInTiles = planetFeature.maxDrawSizeInTiles;
                    worldFeature.drawCenter = new Vector3(planetFeature.drawCenter[0], planetFeature.drawCenter[1], planetFeature.drawCenter[2]);

                    Find.WorldFeatures.features.Add(worldFeature);
                }
                catch (Exception e) { Logger.Error($"Failed set planet feature from def '{planetFeature.defName}'. Reason: {e}"); }
            }

            Find.WorldFeatures.textsCreated = false;
            Find.WorldFeatures.UpdateFeatures();
        }

        public static void SetPlanetFactions()
        {
            Faction[] planetFactions = Find.World.factionManager.AllFactions.ToArray();
            foreach (PlanetNPCFaction faction in cachedWorldValues.NPCFactions)
            {
                try
                {
                    Faction toModify = Find.World.factionManager.AllFactions.First(fetch => fetch.def.defName == faction.factionDefName);

                    toModify.Name = faction.factionName;

                    toModify.color = new Color(faction.factionColor[0],
                        faction.factionColor[1],
                        faction.factionColor[2],
                        faction.factionColor[3]);
                }
                catch (Exception e) { Logger.Error($"Failed set planet faction from def '{faction.factionDefName}'. Reason: {e}"); }
            }
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
                try
                {
                    PlanetNPCFaction toCreate = new PlanetNPCFaction();
                    toCreate.factionDefName = faction.defName;
                    npcFactions.Add(toCreate);
                }
                catch (Exception e) { Logger.Error($"Failed transform faction '{faction.defName}' from game. Reason: {e}"); }
            }
            return npcFactions.ToArray();
        }

        public static FactionDef[] GetFactionDefsFromNPCFaction(PlanetNPCFaction[] factions)
        {
            List<FactionDef> defList = new List<FactionDef>();
            foreach (PlanetNPCFaction faction in factions)
            {
                try { defList.Add(DefDatabase<FactionDef>.AllDefs.ToArray().First(fetch => fetch.defName == faction.factionDefName)); }
                catch (Exception e) { Logger.Error($"Failed get FactionDef '{faction.factionDefName}' from server. Reason: {e}"); }
            }
            return defList.ToArray();
        }

        public static PlanetNPCFaction[] GetPlanetNPCFactions()
        {
            List<PlanetNPCFaction> planetFactions = new List<PlanetNPCFaction>();
            Faction[] existingFactions = Find.World.factionManager.AllFactions.ToArray();

            foreach(Faction faction in existingFactions)
            {
                try
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
                catch (Exception e) { Logger.Error($"Failed get NPC faction '{faction.def.defName}' to populate. Reason: {e}"); }
            }

            return planetFactions.ToArray();
        }

        private static PlanetNPCSettlement[] GetPlanetNPCSettlements()
        {
            FactionDef[] worldFactionDefs = GetFactionDefsFromNPCFaction(WorldGeneratorManager.cachedWorldValues.NPCFactions);
            List<PlanetNPCSettlement> npcSettlements = new List<PlanetNPCSettlement>();
            foreach (Settlement settlement in Find.World.worldObjects.Settlements.Where(fetch => worldFactionDefs.Contains(fetch.Faction.def)))
            {
                try
                {
                    PlanetNPCSettlement PlanetNPCSettlement = new PlanetNPCSettlement();
                    PlanetNPCSettlement.tile = settlement.Tile;
                    PlanetNPCSettlement.factionDefName = settlement.Faction.def.defName;
                    PlanetNPCSettlement.name = settlement.Name;

                    npcSettlements.Add(PlanetNPCSettlement);
                }
                catch (Exception e) { Logger.Error($"Failed get NPC settlement '{settlement.Tile}' to populate. Reason: {e}"); }
            }
            return npcSettlements.ToArray();
        }

        private static PlanetFeature[] GetPlanetFeatures()
        {
            List<PlanetFeature> planetFeatures = new List<PlanetFeature>();
            WorldFeature[] worldFeatures = Find.World.features.features.ToArray();
            foreach (WorldFeature worldFeature in worldFeatures)
            {
                try
                {
                    PlanetFeature planetFeature = new PlanetFeature();
                    planetFeature.featureName = worldFeature.name;
                    planetFeature.defName = worldFeature.def.defName;
                    planetFeature.maxDrawSizeInTiles = worldFeature.maxDrawSizeInTiles;
                    planetFeature.drawCenter = new float[] { worldFeature.drawCenter.x, worldFeature.drawCenter.y, worldFeature.drawCenter.z };

                    planetFeatures.Add(planetFeature);
                }
                catch (Exception e) { Logger.Error($"Failed get feature '{worldFeature.def.defName}' to populate. Reason: {e}"); }
            }

            return planetFeatures.ToArray();
        }
    }
}
