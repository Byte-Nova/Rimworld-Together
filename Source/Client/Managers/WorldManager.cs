using GameClient.Core;
using GameClient.Dialogs;
using GameClient.Misc;
using TCPNetwork.Packets;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Profile;
using static Shared.CommonEnumerators;
using Shared.Details.Planet;
using Shared.Files.Configs;
using Shared.Misc;
using GameClient.Hooks.TCPNetwork;

namespace GameClient.Managers
{
    public static class WorldManager
    {
        public static string tempWorldPath => Path.Combine(Master.AppdataTempPath, "World.temp");

        private static IEnumerable<GameSetupStepDef> SetupStepsInOrder => from x in DefDatabase<GameSetupStepDef>.AllDefs
                                                                          orderby x.order, x.index
                                                                          select x;

        private static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                       orderby x.order, x.index
                                                                       select x;

        private static readonly List<Type> stepsToUseIfNotFresh = new List<Type>()
        {
            typeof(WorldGenStep_Tiles),
            typeof(WorldGenStep_Terrain),
            typeof(WorldGenStep_Factions),
            typeof(WorldGenStep_Features)
        };

        [HandlesPacket(PacketHeader.WorldManager)]
        private static void ParsePacket(byte[] bytes)
        {
            WorldData data = Serializer.ConvertBytesToObject<WorldData>(bytes);

            switch (data._stepMode)
            {
                case WorldStepMode.AskFor:
                    OnAskForWorld();
                    break;

                case WorldStepMode.Sent:
                    WorldManager.OnReceiveWorld(data);
                    break;
            }
        }

        public static void OnAskForWorld()
        {
            SessionHandler.IsGeneratingFreshWorld = true;

            RT_Dialog_Wait.Instance.Close();

            RT_Dialog_Base.PushNewDialog(new Page_SelectScenario());
        }

        public static void OnExistingWorld()
        {
            SessionHandler.IsGeneratingFreshWorld = false;

            RT_Dialog_Wait.Instance.Close();

            RT_Dialog_Base.PushNewDialog(new Page_SelectScenario());
        }

        public static void SendWorld()
        {
            WorldManagerH.PopulateWorldValues();

            WorldData data = new WorldData();
            data._stepMode = WorldStepMode.Sent;
            data._fileBytes = Serializer.ConvertObjectToBytes(SessionHandler.CurrentWorld);

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.WorldManager, data);

            OnWorldSent();
        }

        private static void OnWorldSent()
        {
            File.Delete(WorldManager.tempWorldPath);

            SessionHandler.IsGeneratingFreshWorld = false;
        }

        public static void OnReceiveWorld(WorldData data)
        {
            SetValuesFromServer(data);
            WorldManager.OnExistingWorld();
        }

        public static void SetValuesFromGame(string seedString, float planetCoverage, OverallRainfall rainfall, OverallTemperature temperature, 
            OverallPopulation population, LandmarkDensity density, List<FactionDef> factions, float pollution)
        {
            SessionHandler.CurrentWorld = new PlanetConfigFile();
            SessionHandler.CurrentWorld.SeedString = seedString;
            SessionHandler.CurrentWorld.PersistentRandomValue = GenText.StableStringHash(seedString);
            SessionHandler.CurrentWorld.PlanetCoverage = planetCoverage;
            SessionHandler.CurrentWorld.Rainfall = (int)rainfall;
            SessionHandler.CurrentWorld.Temperature = (int)temperature;
            SessionHandler.CurrentWorld.Population = (int)population;
            SessionHandler.CurrentWorld.LandmarkDensity = (int)density;
            SessionHandler.CurrentWorld.Pollution = pollution;
            SessionHandler.CurrentWorld.NPCFactions = WorldManagerH.GetNPCFactionsFromDef(factions.ToArray());
        }

        private static void SetValuesFromServer(WorldData data)
        {
            SessionHandler.CurrentWorld = Serializer.ConvertBytesToObject<PlanetConfigFile>(data._fileBytes);
        }

        public static void GenerateNormalWorld()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Find.GameInitData.ResetWorldRelatedMapInitData();

                Rand.EnsureStateStackEmpty();
                Rand.PushState(SessionHandler.CurrentWorld.PersistentRandomValue);

                Current.Game.World = WorldGenerator.GenerateWorld(
                    SessionHandler.CurrentWorld.PlanetCoverage,
                    SessionHandler.CurrentWorld.SeedString,
                    (OverallRainfall)SessionHandler.CurrentWorld.Rainfall,
                    (OverallTemperature)SessionHandler.CurrentWorld.Temperature,
                    (OverallPopulation)SessionHandler.CurrentWorld.Population,
                    (LandmarkDensity)SessionHandler.CurrentWorld.LandmarkDensity,
                    WorldManagerH.GetFactionDefsFromNPCFaction(SessionHandler.CurrentWorld.NPCFactions),
                    SessionHandler.CurrentWorld.Pollution);

                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    Find.World.renderer.RegenerateAllLayersNow();
                    MemoryUtility.UnloadUnusedUnityAssets();
                    Current.CreatingWorld = null;
                    PostWorldGeneration();
                });
            }, "GeneratingWorld", doAsynchronously: true, null);

            Rand.EnsureStateStackEmpty();
            Rand.PushState(0);
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

        public static void SetPlanetFeatures()
        {
            WorldFeature[] worldFeatures = Find.WorldFeatures.features.ToArray();
            foreach (WorldFeature feature in worldFeatures) Find.WorldFeatures.features.Remove(feature);

            for (int i = 0; i < SessionHandler.CurrentWorld.Features.Length; i++)
            {
                FeatureDetail planetFeature = SessionHandler.CurrentWorld.Features[i];

                try
                {
                    WorldFeature worldFeature = new WorldFeature();
                    worldFeature.def = DefDatabase<FeatureDef>.AllDefs.First(fetch => fetch.defName == planetFeature.DefName);
                    worldFeature.uniqueID = i;
                    worldFeature.name = planetFeature.Label;
                    worldFeature.maxDrawSizeInTiles = planetFeature.MaxDrawSizeInTiles;
                    worldFeature.drawCenter = new Vector3(planetFeature.DrawCenter[0], planetFeature.DrawCenter[1], planetFeature.DrawCenter[2]);
                    worldFeature.layer = PlanetLayer.Selected;

                    Find.WorldFeatures.features.Add(worldFeature);
                }
                catch (Exception e) { Printer.Warning($"Failed set planet feature from def '{planetFeature.DefName}'. Reason: {e}"); }
            }

            Find.WorldFeatures.textsCreated = false;

            Find.WorldFeatures.UpdateFeatures();
        }

        public static void SetPlanetFactions()
        {
            Faction[] planetFactions = Find.World.factionManager.AllFactions.ToArray();

            for (int i = 0; i < SessionHandler.CurrentWorld.NPCFactions.Length; i++)
            {
                try
                {
                    NPCFactionDetail faction = SessionHandler.CurrentWorld.NPCFactions[i];

                    Faction toModify = planetFactions.First(fetch => fetch.def.defName == SessionHandler.CurrentWorld.NPCFactions[i].DefName);

                    toModify.Name = faction.Name;

                    toModify.color = new Color(faction.Color[0],
                        faction.Color[1],
                        faction.Color[2],
                        faction.Color[3]);
                }
                catch (Exception e) { Printer.Warning($"Failed set planet faction from def '{SessionHandler.CurrentWorld.NPCFactions[i].DefName}'. Reason: {e}"); }
            }
        }
    }

    public static class WorldManagerH
    {
        public static void PopulateWorldValues()
        {
            Printer.Warning("Populating world values", LogImportanceMode.Verbose);
            SessionHandler.CurrentWorld.Features = GetPlanetFeatures();
            SessionHandler.CurrentWorld.Roads = RoadManagerHelper.GetPlanetRoads();
            SessionHandler.CurrentWorld.PollutedTiles = PollutionManagerHelper.GetPlanetPollutedTiles();
            SessionHandler.CurrentWorld.NPCSettlements = GetPlanetNPCSettlements();
            SessionHandler.CurrentWorld.NPCFactions = GetPlanetNPCFactions();
        }

        public static NPCFactionDetail[] GetNPCFactionsFromDef(FactionDef[] factionDefs)
        {
            List<NPCFactionDetail> npcFactions = new List<NPCFactionDetail>();
            foreach (FactionDef faction in factionDefs)
            {
                try
                {
                    NPCFactionDetail toCreate = new NPCFactionDetail();
                    toCreate.DefName = faction.defName;
                    npcFactions.Add(toCreate);
                }
                catch (Exception e) { Printer.Warning($"Failed to get faction '{faction.defName}' from game. Reason: {e}"); }
            }
            return npcFactions.ToArray();
        }

        public static List<FactionDef> GetFactionDefsFromNPCFaction(NPCFactionDetail[] factions)
        {
            List<FactionDef> defList = new List<FactionDef>();
            foreach (NPCFactionDetail faction in factions) defList.Add(DefDatabase<FactionDef>.GetNamed(faction.DefName));

            return defList;
        }

        public static NPCFactionDetail[] GetPlanetNPCFactions()
        {
            List<NPCFactionDetail> planetFactions = new List<NPCFactionDetail>();
            Faction[] existingFactions = Find.World.factionManager.AllFactions.ToArray();

            foreach (Faction faction in existingFactions)
            {
                try
                {
                    if (faction == Faction.OfPlayer) continue;
                    else
                    {
                        NPCFactionDetail planetFaction = new NPCFactionDetail();
                        planetFaction.DefName = faction.def.defName;
                        planetFaction.Name = faction.Name;
                        planetFaction.Color = new float[] { faction.Color.r, faction.Color.g, faction.Color.b, faction.Color.a };

                        planetFactions.Add(planetFaction);
                    }
                }
                catch (Exception e) { Printer.Warning($"Failed to get NPC faction '{faction.def.defName}' to populate. Reason: {e}"); }
            }

            return planetFactions.ToArray();
        }

        public static NPCSettlementDetail[] GetPlanetNPCSettlements()
        {
            Faction[] worldNPCFactions = Find.FactionManager.AllFactions.Where(fetch => !SessionHandler.PlayerFactions.Contains(fetch) &&
                fetch != Faction.OfPlayer).ToArray();

            List<FactionDef> worldNPCFactionDefs = new List<FactionDef>();
            foreach (Faction faction in worldNPCFactions) worldNPCFactionDefs.Add(faction.def);

            List<NPCSettlementDetail> npcSettlements = new List<NPCSettlementDetail>();
            foreach (Settlement settlement in Find.World.worldObjects.Settlements.Where(fetch => worldNPCFactionDefs.Contains(fetch.Faction.def)))
            {
                try
                {
                    NPCSettlementDetail PlanetNPCSettlementDetails = new NPCSettlementDetail();
                    PlanetNPCSettlementDetails.Tile = settlement.Tile;
                    PlanetNPCSettlementDetails.DefName = settlement.Faction.def.defName;
                    PlanetNPCSettlementDetails.Name = settlement.Name;
                    PlanetNPCSettlementDetails.FactionName = settlement.Faction.Name;
                    npcSettlements.Add(PlanetNPCSettlementDetails);
                }
                catch (Exception e) { Printer.Warning($"Failed to get NPC settlement '{settlement.Tile}' to populate. Reason: {e}"); }
            }
            return npcSettlements.ToArray();
        }

        public static FeatureDetail[] GetPlanetFeatures()
        {
            List<FeatureDetail> planetFeatures = new List<FeatureDetail>();
            WorldFeature[] worldFeatures = Find.World.features.features.ToArray();
            foreach (WorldFeature worldFeature in worldFeatures)
            {
                try
                {
                    FeatureDetail planetFeature = new FeatureDetail();
                    planetFeature.Label = worldFeature.name;
                    planetFeature.DefName = worldFeature.def.defName;
                    planetFeature.MaxDrawSizeInTiles = worldFeature.maxDrawSizeInTiles;
                    planetFeature.DrawCenter = new float[] { worldFeature.drawCenter.x, worldFeature.drawCenter.y, worldFeature.drawCenter.z };

                    planetFeatures.Add(planetFeature);
                }
                catch (Exception e) { Printer.Warning($"Failed to get feature '{worldFeature.def.defName}' to populate. Reason: {e}"); }
            }

            return planetFeatures.ToArray();
        }
    }
}
