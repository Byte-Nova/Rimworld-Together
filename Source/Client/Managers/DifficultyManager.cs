using Shared;
using Verse;

namespace GameClient
{
    public static class DifficultyManager
    {
        public static DifficultyValuesFile difficultyValues;

        public static void SetValues(ServerGlobalData serverGlobalData) { difficultyValues = serverGlobalData._difficultyValues; }

        public static void SendCustomDifficulty()
        {
            DifficultyData difficultyData = new DifficultyData();

            difficultyData._values.ThreatScale = Current.Game.storyteller.difficulty.threatScale;

            difficultyData._values.AllowBigThreats = Current.Game.storyteller.difficulty.allowBigThreats;

            difficultyData._values.AllowViolentQuests = Current.Game.storyteller.difficulty.allowViolentQuests;

            difficultyData._values.AllowIntroThreats = Current.Game.storyteller.difficulty.allowIntroThreats;

            difficultyData._values.PredatorsHuntHumanlikes = Current.Game.storyteller.difficulty.predatorsHuntHumanlikes;

            difficultyData._values.AllowExtremeWeatherIncidents = Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents;

            difficultyData._values.CropYieldFactor = Current.Game.storyteller.difficulty.cropYieldFactor;

            difficultyData._values.MineYieldFactor = Current.Game.storyteller.difficulty.mineYieldFactor;

            difficultyData._values.ButcherYieldFactor = Current.Game.storyteller.difficulty.butcherYieldFactor;

            difficultyData._values.ResearchSpeedFactor = Current.Game.storyteller.difficulty.researchSpeedFactor;

            difficultyData._values.QuestRewardValueFactor = Current.Game.storyteller.difficulty.questRewardValueFactor;

            difficultyData._values.RaidLootPointsFactor = Current.Game.storyteller.difficulty.raidLootPointsFactor;

            difficultyData._values.TradePriceFactorLoss = Current.Game.storyteller.difficulty.tradePriceFactorLoss;

            difficultyData._values.MaintenanceCostFactor = Current.Game.storyteller.difficulty.maintenanceCostFactor;

            difficultyData._values.ScariaRotChance = Current.Game.storyteller.difficulty.scariaRotChance;

            difficultyData._values.EnemyDeathOnDownedChanceFactor = Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor;

            difficultyData._values.ColonistMoodOffset = Current.Game.storyteller.difficulty.colonistMoodOffset;

            difficultyData._values.FoodPoisonChanceFactor = Current.Game.storyteller.difficulty.foodPoisonChanceFactor;

            difficultyData._values.ManhunterChanceOnDamageFactor = Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor;

            difficultyData._values.PlayerPawnInfectionChanceFactor = Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor;

            difficultyData._values.DiseaseIntervalFactor = Current.Game.storyteller.difficulty.diseaseIntervalFactor;

            difficultyData._values.EnemyReproductionRateFactor = Current.Game.storyteller.difficulty.enemyReproductionRateFactor;

            difficultyData._values.DeepDrillInfestationChanceFactor = Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor;

            difficultyData._values.FriendlyFireChanceFactor = Current.Game.storyteller.difficulty.friendlyFireChanceFactor;

            difficultyData._values.AllowInstantKillChance = Current.Game.storyteller.difficulty.allowInstantKillChance;

            difficultyData._values.PeacefulTemples = Current.Game.storyteller.difficulty.peacefulTemples;

            difficultyData._values.AllowCaveHives = Current.Game.storyteller.difficulty.allowCaveHives;

            difficultyData._values.UnwaveringPrisoners = Current.Game.storyteller.difficulty.unwaveringPrisoners;

            difficultyData._values.AllowTraps = Current.Game.storyteller.difficulty.allowTraps;

            difficultyData._values.AllowTurrets = Current.Game.storyteller.difficulty.allowTurrets;

            difficultyData._values.AllowMortars = Current.Game.storyteller.difficulty.allowMortars;

            difficultyData._values.ClassicMortars = Current.Game.storyteller.difficulty.classicMortars;

            difficultyData._values.AdaptationEffectFactor = Current.Game.storyteller.difficulty.adaptationEffectFactor;

            difficultyData._values.AdaptationGrowthRateFactorOverZero = Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero;

            difficultyData._values.FixedWealthMode = Current.Game.storyteller.difficulty.fixedWealthMode;

            difficultyData._values.LowPopConversionBoost = Current.Game.storyteller.difficulty.lowPopConversionBoost;

            difficultyData._values.NoBabiesOrChildren = Current.Game.storyteller.difficulty.noBabiesOrChildren;

            difficultyData._values.BabiesAreHealthy = Current.Game.storyteller.difficulty.babiesAreHealthy;

            difficultyData._values.ChildRaidersAllowed = Current.Game.storyteller.difficulty.childRaidersAllowed;

            difficultyData._values.ChildAgingRate = Current.Game.storyteller.difficulty.childAgingRate;

            difficultyData._values.AdultAgingRate = Current.Game.storyteller.difficulty.adultAgingRate;

            difficultyData._values.WastepackInfestationChanceFactor = Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CustomDifficultyPacket), difficultyData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void EnforceCustomDifficulty()
        {
            if (!difficultyValues.UseCustomDifficulty) return;
            else
            {
                Current.Game.storyteller.difficulty.threatScale = difficultyValues.ThreatScale;

                Current.Game.storyteller.difficulty.allowBigThreats = difficultyValues.AllowBigThreats;

                Current.Game.storyteller.difficulty.allowViolentQuests = difficultyValues.AllowViolentQuests;

                Current.Game.storyteller.difficulty.allowIntroThreats = difficultyValues.AllowIntroThreats;

                Current.Game.storyteller.difficulty.predatorsHuntHumanlikes = difficultyValues.PredatorsHuntHumanlikes;

                Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents = difficultyValues.AllowExtremeWeatherIncidents;

                Current.Game.storyteller.difficulty.cropYieldFactor = difficultyValues.CropYieldFactor;

                Current.Game.storyteller.difficulty.mineYieldFactor = difficultyValues.MineYieldFactor;

                Current.Game.storyteller.difficulty.butcherYieldFactor = difficultyValues.ButcherYieldFactor;

                Current.Game.storyteller.difficulty.researchSpeedFactor = difficultyValues.ResearchSpeedFactor;

                Current.Game.storyteller.difficulty.questRewardValueFactor = difficultyValues.QuestRewardValueFactor;

                Current.Game.storyteller.difficulty.raidLootPointsFactor = difficultyValues.RaidLootPointsFactor;

                Current.Game.storyteller.difficulty.tradePriceFactorLoss = difficultyValues.TradePriceFactorLoss;

                Current.Game.storyteller.difficulty.maintenanceCostFactor = difficultyValues.MaintenanceCostFactor;

                Current.Game.storyteller.difficulty.scariaRotChance = difficultyValues.ScariaRotChance;

                Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor = difficultyValues.EnemyDeathOnDownedChanceFactor;

                Current.Game.storyteller.difficulty.colonistMoodOffset = difficultyValues.ColonistMoodOffset;

                Current.Game.storyteller.difficulty.foodPoisonChanceFactor = difficultyValues.FoodPoisonChanceFactor;

                Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor = difficultyValues.ManhunterChanceOnDamageFactor;

                Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor = difficultyValues.PlayerPawnInfectionChanceFactor;

                Current.Game.storyteller.difficulty.diseaseIntervalFactor = difficultyValues.DiseaseIntervalFactor;

                Current.Game.storyteller.difficulty.enemyReproductionRateFactor = difficultyValues.EnemyReproductionRateFactor;

                Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor = difficultyValues.DeepDrillInfestationChanceFactor;

                Current.Game.storyteller.difficulty.friendlyFireChanceFactor = difficultyValues.FriendlyFireChanceFactor;

                Current.Game.storyteller.difficulty.allowInstantKillChance = difficultyValues.AllowInstantKillChance;

                Current.Game.storyteller.difficulty.peacefulTemples = difficultyValues.PeacefulTemples;

                Current.Game.storyteller.difficulty.allowCaveHives = difficultyValues.AllowCaveHives;

                Current.Game.storyteller.difficulty.unwaveringPrisoners = difficultyValues.UnwaveringPrisoners;

                Current.Game.storyteller.difficulty.allowTraps = difficultyValues.AllowTraps;

                Current.Game.storyteller.difficulty.allowTurrets = difficultyValues.AllowTurrets;

                Current.Game.storyteller.difficulty.allowMortars = difficultyValues.AllowMortars;

                Current.Game.storyteller.difficulty.classicMortars = difficultyValues.ClassicMortars;

                Current.Game.storyteller.difficulty.adaptationEffectFactor = difficultyValues.AdaptationEffectFactor;

                Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero = difficultyValues.AdaptationGrowthRateFactorOverZero;

                Current.Game.storyteller.difficulty.fixedWealthMode = difficultyValues.FixedWealthMode;

                Current.Game.storyteller.difficulty.lowPopConversionBoost = difficultyValues.LowPopConversionBoost;

                Current.Game.storyteller.difficulty.noBabiesOrChildren = difficultyValues.NoBabiesOrChildren;

                Current.Game.storyteller.difficulty.babiesAreHealthy = difficultyValues.BabiesAreHealthy;

                Current.Game.storyteller.difficulty.childRaidersAllowed = difficultyValues.ChildRaidersAllowed;

                Current.Game.storyteller.difficulty.childAgingRate = difficultyValues.ChildAgingRate;

                Current.Game.storyteller.difficulty.adultAgingRate = difficultyValues.AdultAgingRate;

                Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor = difficultyValues.WastepackInfestationChanceFactor;
            }
        }
    }
}
