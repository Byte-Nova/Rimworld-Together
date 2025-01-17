using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameClient.Misc;
using GameClient.TCP;
using RimWorld;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    [RTManager]
    public static class GameParameterManager
    {
        public static ScenarioValuesFile scenarioFile;

        public static StorytellerValuesFile storytellerFile;

        public static DifficultyValuesFile difficultyFile;

        public static void SetValues(ServerGlobalData data)
        {
            scenarioFile = data._scenarioValues;
            storytellerFile = data._storytellerValues;
            difficultyFile = data._difficultyValues;
        }

        public static ScenarioValuesFile GetScenario(Page_SelectScenario __instance)
        {
            ScenarioValuesFile file = new ScenarioValuesFile();

            file.ScenarioName = GenManagerH.GetScenarioReference(__instance).name;

            return file;
        }

        public static void SetScenario(ScenarioValuesFile file)
        {
            if (!file.EnforceScenario) return;
            else Current.Game.Scenario = ScenarioLister.AllScenarios().First(fetch => fetch.name == file.ScenarioName);
        }

        public static void SendScenario(ScenarioValuesFile file, bool mode)
        {
            file.EnforceScenario = mode;

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Scenario;
            data._scenario = file;

            Packet packet = Packet.CreatePacketFromObject(nameof(GameParameterManager), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static StorytellerValuesFile GetStoryteller(Page_SelectStoryteller __instance)
        {
            StorytellerValuesFile file = new StorytellerValuesFile();

            file.StorytellerDefname = GenManagerH.GetStorytellerReference(__instance).def.defName;

            return file;
        }

        public static void SetStoryteller(StorytellerValuesFile file)
        {
            if (!file.EnforceStoryteller) return;
            else
            {
                StorytellerDef storytellerDef = DefDatabase<StorytellerDef>.AllDefs.First(fetch => fetch.defName == file.StorytellerDefname);
                DifficultyDef difficultyDef = DifficultyDefOf.Rough;
                Difficulty difficulty = new Difficulty(difficultyDef);

                if (Current.Game.storyteller != null && Current.Game.storyteller.def == storytellerDef) return;
                else
                {
                    if (Current.Game.storyteller != null)
                    {
                        difficultyDef = Current.Game.storyteller.difficultyDef;
                        difficulty = Current.Game.storyteller.difficulty;
                    }

                    Current.Game.storyteller = new Storyteller(storytellerDef, difficultyDef, difficulty);
                }
            }
        }

        public static void SendStoryteller(StorytellerValuesFile file, bool mode)
        {
            file.EnforceStoryteller = mode;

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Storyteller;
            data._storyteller = file;

            Packet packet = Packet.CreatePacketFromObject(nameof(GameParameterManager), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static DifficultyValuesFile GetDifficulty()
        {
            DifficultyValuesFile file = new DifficultyValuesFile();

            file.ThreatScale = Current.Game.storyteller.difficulty.threatScale;

            file.AllowBigThreats = Current.Game.storyteller.difficulty.allowBigThreats;

            file.AllowViolentQuests = Current.Game.storyteller.difficulty.allowViolentQuests;

            file.AllowIntroThreats = Current.Game.storyteller.difficulty.allowIntroThreats;

            file.PredatorsHuntHumanlikes = Current.Game.storyteller.difficulty.predatorsHuntHumanlikes;

            file.AllowExtremeWeatherIncidents = Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents;

            file.CropYieldFactor = Current.Game.storyteller.difficulty.cropYieldFactor;

            file.MineYieldFactor = Current.Game.storyteller.difficulty.mineYieldFactor;

            file.ButcherYieldFactor = Current.Game.storyteller.difficulty.butcherYieldFactor;

            file.ResearchSpeedFactor = Current.Game.storyteller.difficulty.researchSpeedFactor;

            file.QuestRewardValueFactor = Current.Game.storyteller.difficulty.questRewardValueFactor;

            file.RaidLootPointsFactor = Current.Game.storyteller.difficulty.raidLootPointsFactor;

            file.TradePriceFactorLoss = Current.Game.storyteller.difficulty.tradePriceFactorLoss;

            file.MaintenanceCostFactor = Current.Game.storyteller.difficulty.maintenanceCostFactor;

            file.ScariaRotChance = Current.Game.storyteller.difficulty.scariaRotChance;

            file.EnemyDeathOnDownedChanceFactor = Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor;

            file.ColonistMoodOffset = Current.Game.storyteller.difficulty.colonistMoodOffset;

            file.FoodPoisonChanceFactor = Current.Game.storyteller.difficulty.foodPoisonChanceFactor;

            file.ManhunterChanceOnDamageFactor = Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor;

            file.PlayerPawnInfectionChanceFactor = Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor;

            file.DiseaseIntervalFactor = Current.Game.storyteller.difficulty.diseaseIntervalFactor;

            file.EnemyReproductionRateFactor = Current.Game.storyteller.difficulty.enemyReproductionRateFactor;

            file.DeepDrillInfestationChanceFactor = Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor;

            file.FriendlyFireChanceFactor = Current.Game.storyteller.difficulty.friendlyFireChanceFactor;

            file.AllowInstantKillChance = Current.Game.storyteller.difficulty.allowInstantKillChance;

            file.PeacefulTemples = Current.Game.storyteller.difficulty.peacefulTemples;

            file.AllowCaveHives = Current.Game.storyteller.difficulty.allowCaveHives;

            file.UnwaveringPrisoners = Current.Game.storyteller.difficulty.unwaveringPrisoners;

            file.AllowTraps = Current.Game.storyteller.difficulty.allowTraps;

            file.AllowTurrets = Current.Game.storyteller.difficulty.allowTurrets;

            file.AllowMortars = Current.Game.storyteller.difficulty.allowMortars;

            file.ClassicMortars = Current.Game.storyteller.difficulty.classicMortars;

            file.AdaptationEffectFactor = Current.Game.storyteller.difficulty.adaptationEffectFactor;

            file.AdaptationGrowthRateFactorOverZero = Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero;

            file.FixedWealthMode = Current.Game.storyteller.difficulty.fixedWealthMode;

            file.LowPopConversionBoost = Current.Game.storyteller.difficulty.lowPopConversionBoost;

            file.NoBabiesOrChildren = Current.Game.storyteller.difficulty.noBabiesOrChildren;

            file.BabiesAreHealthy = Current.Game.storyteller.difficulty.babiesAreHealthy;

            file.ChildRaidersAllowed = Current.Game.storyteller.difficulty.childRaidersAllowed;

            file.ChildAgingRate = Current.Game.storyteller.difficulty.childAgingRate;

            file.AdultAgingRate = Current.Game.storyteller.difficulty.adultAgingRate;

            file.WastepackInfestationChanceFactor = Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor;

            return file;
        }

        public static void SetDifficulty(DifficultyValuesFile file)
        {
            if (!file.EnforceDifficulty) return;
            else
            {
                Current.Game.storyteller.difficulty.threatScale = file.ThreatScale;

                Current.Game.storyteller.difficulty.allowBigThreats = file.AllowBigThreats;

                Current.Game.storyteller.difficulty.allowViolentQuests = file.AllowViolentQuests;

                Current.Game.storyteller.difficulty.allowIntroThreats = file.AllowIntroThreats;

                Current.Game.storyteller.difficulty.predatorsHuntHumanlikes = file.PredatorsHuntHumanlikes;

                Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents = file.AllowExtremeWeatherIncidents;

                Current.Game.storyteller.difficulty.cropYieldFactor = file.CropYieldFactor;

                Current.Game.storyteller.difficulty.mineYieldFactor = file.MineYieldFactor;

                Current.Game.storyteller.difficulty.butcherYieldFactor = file.ButcherYieldFactor;

                Current.Game.storyteller.difficulty.researchSpeedFactor = file.ResearchSpeedFactor;

                Current.Game.storyteller.difficulty.questRewardValueFactor = file.QuestRewardValueFactor;

                Current.Game.storyteller.difficulty.raidLootPointsFactor = file.RaidLootPointsFactor;

                Current.Game.storyteller.difficulty.tradePriceFactorLoss = file.TradePriceFactorLoss;

                Current.Game.storyteller.difficulty.maintenanceCostFactor = file.MaintenanceCostFactor;

                Current.Game.storyteller.difficulty.scariaRotChance = file.ScariaRotChance;

                Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor = file.EnemyDeathOnDownedChanceFactor;

                Current.Game.storyteller.difficulty.colonistMoodOffset = file.ColonistMoodOffset;

                Current.Game.storyteller.difficulty.foodPoisonChanceFactor = file.FoodPoisonChanceFactor;

                Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor = file.ManhunterChanceOnDamageFactor;

                Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor = file.PlayerPawnInfectionChanceFactor;

                Current.Game.storyteller.difficulty.diseaseIntervalFactor = file.DiseaseIntervalFactor;

                Current.Game.storyteller.difficulty.enemyReproductionRateFactor = file.EnemyReproductionRateFactor;

                Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor = file.DeepDrillInfestationChanceFactor;

                Current.Game.storyteller.difficulty.friendlyFireChanceFactor = file.FriendlyFireChanceFactor;

                Current.Game.storyteller.difficulty.allowInstantKillChance = file.AllowInstantKillChance;

                Current.Game.storyteller.difficulty.peacefulTemples = file.PeacefulTemples;

                Current.Game.storyteller.difficulty.allowCaveHives = file.AllowCaveHives;

                Current.Game.storyteller.difficulty.unwaveringPrisoners = file.UnwaveringPrisoners;

                Current.Game.storyteller.difficulty.allowTraps = file.AllowTraps;

                Current.Game.storyteller.difficulty.allowTurrets = file.AllowTurrets;

                Current.Game.storyteller.difficulty.allowMortars = file.AllowMortars;

                Current.Game.storyteller.difficulty.classicMortars = file.ClassicMortars;

                Current.Game.storyteller.difficulty.adaptationEffectFactor = file.AdaptationEffectFactor;

                Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero = file.AdaptationGrowthRateFactorOverZero;

                Current.Game.storyteller.difficulty.fixedWealthMode = file.FixedWealthMode;

                Current.Game.storyteller.difficulty.lowPopConversionBoost = file.LowPopConversionBoost;

                Current.Game.storyteller.difficulty.noBabiesOrChildren = file.NoBabiesOrChildren;

                Current.Game.storyteller.difficulty.babiesAreHealthy = file.BabiesAreHealthy;

                Current.Game.storyteller.difficulty.childRaidersAllowed = file.ChildRaidersAllowed;

                Current.Game.storyteller.difficulty.childAgingRate = file.ChildAgingRate;

                Current.Game.storyteller.difficulty.adultAgingRate = file.AdultAgingRate;

                Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor = file.WastepackInfestationChanceFactor;
            }
        }

        public static void SendDifficulty(DifficultyValuesFile file, bool mode)
        {
            file.EnforceDifficulty = mode;

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Difficulty;
            data._difficulty = file;

            Packet packet = Packet.CreatePacketFromObject(nameof(GameParameterManager), data);
            Network.listener.EnqueuePacket(packet);
        }
    }

    public static class GenManagerH
    {
        public static Scenario GetScenarioReference(Page_SelectScenario __instance)
        {
            return (Scenario)typeof(Page_SelectScenario).GetField("curScen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        }

        public static Storyteller GetStorytellerReference(Page_SelectStoryteller __instance)
        {
            StorytellerDef toGet = (StorytellerDef)typeof(Page_SelectStoryteller).GetField("storyteller", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(__instance);

            return new Storyteller(toGet, DifficultyDefOf.Rough, new Difficulty(DifficultyDefOf.Rough));
        }
    }
}
