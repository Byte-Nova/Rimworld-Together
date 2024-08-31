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
    public static class PlanetGeneratorManager
    {
        public static WorldValuesFile cachedWorldValues;

        private static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      orderby x.order, x.index
                                                                      select x;

        private static readonly List<Type> stepsToIgnoreIfNotFresh = new List<Type>()
        {
            typeof(WorldGenStep_Roads),
            typeof(WorldGenStep_AncientRoads),
            typeof(WorldGenStep_Rivers),
            typeof(WorldGenStep_Pollution)
        };

        public static void ParseWorldPacket(Packet packet)
        {
            WorldData worldData = Serializer.ConvertBytesToObject<WorldData>(packet.contents);

            switch (worldData._stepMode)
            {
                case WorldStepMode.Required:
                    OnRequireWorld();
                    break;

                case WorldStepMode.Existing:
                    OnExistingWorld(worldData);
                    break;
            }
        }

        public static void OnRequireWorld()
        {
            DialogManager.PopWaitDialog();

            ClientValues.ToggleGenerateWorld(true);

            Page toUse = new Page_SelectScenario();
            toUse.next = new Page_SelectStartingSite();
            DialogManager.PushNewDialog(toUse);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "RTDialogFirstPlayer".Translate(),
                "RTDialogYouConfigure".Translate() });

            DialogManager.PushNewDialog(d1);
        }

        public static void OnExistingWorld(WorldData worldData)
        {
            DialogManager.PopWaitDialog();

            SetValuesFromServer(worldData);

            DialogManager.PushNewDialog(new Page_SelectScenario());
        }

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
            cachedWorldValues.NPCFactions = PlanetGeneratorManagerHelper.GetNPCFactionsFromDef(factions.ToArray());
        }

        public static void SetValuesFromServer(WorldData worldData) { cachedWorldValues = worldData._worldValuesFile; }

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
            Current.CreatingWorld.info.factions = PlanetGeneratorManagerHelper.GetFactionDefsFromNPCFaction(cachedWorldValues.NPCFactions).ToList();
            Current.CreatingWorld.info.pollution = cachedWorldValues.Pollution;

            WorldGenStepDef[] worldGenSteps = GenStepsInOrder.ToArray();
            for (int i = 0; i < worldGenSteps.Count(); i++)
            {
                WorldGenStep toGenerate = worldGenSteps[i].worldGenStep;
                if (stepsToIgnoreIfNotFresh.Contains(toGenerate.GetType()))
                {
                    //If not creating a world, we skip gen step

                    if (!ClientValues.isGeneratingFreshWorld) continue;
                    else toGenerate.GenerateFresh(cachedWorldValues.SeedString);
                }
                else toGenerate.GenerateFresh(cachedWorldValues.SeedString);
            }

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
            worldData._stepMode = WorldStepMode.Required;
            worldData._worldValuesFile = PlanetGeneratorManagerHelper.PopulateWorldValues();

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.WorldPacket), worldData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void SetPlanetFeatures()
        {
            WorldFeature[] worldFeatures = Find.WorldFeatures.features.ToArray();
            foreach (WorldFeature feature in worldFeatures) Find.WorldFeatures.features.Remove(feature);

            for (int i = 0; i < cachedWorldValues.Features.Length; i++)
            {
                PlanetFeature planetFeature = cachedWorldValues.Features[i];

                try
                {
                    WorldFeature worldFeature = new WorldFeature();
                    worldFeature.def = DefDatabase<FeatureDef>.AllDefs.First(fetch => fetch.defName == planetFeature.defName);
                    worldFeature.uniqueID = i;
                    worldFeature.name = planetFeature.name;
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

            for (int i = 0; i < cachedWorldValues.NPCFactions.Length; i++)
            {
                PlanetNPCFaction faction = cachedWorldValues.NPCFactions[i];

                try
                {
                    Faction toModify = planetFactions[i];

                    toModify.Name = faction.name;

                    toModify.color = new Color(faction.color[0],
                        faction.color[1],
                        faction.color[2],
                        faction.color[3]);
                }
                catch (Exception e) { Logger.Error($"Failed set planet faction from def '{faction.defName}'. Reason: {e}"); }
            }
        }
    }

    public static class PlanetGeneratorManagerHelper
    {
        public static WorldValuesFile PopulateWorldValues()
        {
            PlanetGeneratorManager.cachedWorldValues.Features = GetPlanetFeatures();
            PlanetGeneratorManager.cachedWorldValues.Roads = RoadManagerHelper.GetPlanetRoads();
            PlanetGeneratorManager.cachedWorldValues.Rivers = RiverManagerHelper.GetPlanetRivers();
            PlanetGeneratorManager.cachedWorldValues.PollutedTiles = PollutionManagerHelper.GetPlanetPollutedTiles();
            PlanetGeneratorManager.cachedWorldValues.NPCSettlements = GetPlanetNPCSettlements();
            PlanetGeneratorManager.cachedWorldValues.NPCFactions = GetPlanetNPCFactions();
            return PlanetGeneratorManager.cachedWorldValues;
        }

        public static PlanetNPCFaction[] GetNPCFactionsFromDef(FactionDef[] factionDefs)
        {
            List<PlanetNPCFaction> npcFactions = new List<PlanetNPCFaction>();
            foreach (FactionDef faction in factionDefs)
            {
                try
                {
                    PlanetNPCFaction toCreate = new PlanetNPCFaction();
                    toCreate.defName = faction.defName;
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
                try { defList.Add(DefDatabase<FactionDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == faction.defName)); }
                catch (Exception e) { Logger.Error($"Failed get FactionDef '{faction.defName}' from server. Reason: {e}"); }
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
                        planetFaction.defName = faction.def.defName;
                        planetFaction.name = faction.Name;
                        planetFaction.color = new float[] { faction.Color.r, faction.Color.g, faction.Color.b, faction.Color.a };

                        planetFactions.Add(planetFaction);
                    }
                }
                catch (Exception e) { Logger.Error($"Failed get NPC faction '{faction.def.defName}' to populate. Reason: {e}"); }
            }

            return planetFactions.ToArray();
        }

        public static PlanetNPCSettlement[] GetPlanetNPCSettlements()
        {
            Faction[] worldNPCFactions = Find.FactionManager.AllFactions.Where(fetch => !FactionValues.playerFactions.Contains(fetch) &&
                fetch != Faction.OfPlayer).ToArray();

            List<FactionDef> worldNPCFactionDefs = new List<FactionDef>();
            foreach (Faction faction in worldNPCFactions) worldNPCFactionDefs.Add(faction.def);

            List<PlanetNPCSettlement> npcSettlements = new List<PlanetNPCSettlement>();
            foreach (Settlement settlement in Find.World.worldObjects.Settlements.Where(fetch => worldNPCFactionDefs.Contains(fetch.Faction.def)))
            {
                try
                {
                    PlanetNPCSettlement PlanetNPCSettlement = new PlanetNPCSettlement();
                    PlanetNPCSettlement.tile = settlement.Tile;
                    PlanetNPCSettlement.defName = settlement.Faction.def.defName;
                    PlanetNPCSettlement.name = settlement.Name;

                    npcSettlements.Add(PlanetNPCSettlement);
                }
                catch (Exception e) { Logger.Error($"Failed get NPC settlement '{settlement.Tile}' to populate. Reason: {e}"); }
            }
            return npcSettlements.ToArray();
        }

        public static PlanetFeature[] GetPlanetFeatures()
        {
            List<PlanetFeature> planetFeatures = new List<PlanetFeature>();
            WorldFeature[] worldFeatures = Find.World.features.features.ToArray();
            foreach (WorldFeature worldFeature in worldFeatures)
            {
                try
                {
                    PlanetFeature planetFeature = new PlanetFeature();
                    planetFeature.name = worldFeature.name;
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
