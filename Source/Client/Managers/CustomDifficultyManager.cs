using Shared;
using Verse;

namespace GameClient
{
    public static class CustomDifficultyManager
    {
        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            DifficultyValues.UseCustomDifficulty = serverGlobalData.difficultyValues.UseCustomDifficulty;

            DifficultyValues.ThreatScale = serverGlobalData.difficultyValues.ThreatScale;

            DifficultyValues.AllowBigThreats = serverGlobalData.difficultyValues.AllowBigThreats;

            DifficultyValues.AllowViolentQuests = serverGlobalData.difficultyValues.AllowViolentQuests;

            DifficultyValues.AllowIntroThreats = serverGlobalData.difficultyValues.AllowIntroThreats;

            DifficultyValues.PredatorsHuntHumanlikes = serverGlobalData.difficultyValues.PredatorsHuntHumanlikes;

            DifficultyValues.AllowExtremeWeatherIncidents = serverGlobalData.difficultyValues.AllowExtremeWeatherIncidents;

            DifficultyValues.CropYieldFactor = serverGlobalData.difficultyValues.CropYieldFactor;

            DifficultyValues.MineYieldFactor = serverGlobalData.difficultyValues.MineYieldFactor;

            DifficultyValues.ButcherYieldFactor = serverGlobalData.difficultyValues.ButcherYieldFactor;

            DifficultyValues.ResearchSpeedFactor = serverGlobalData.difficultyValues.ResearchSpeedFactor;

            DifficultyValues.QuestRewardValueFactor = serverGlobalData.difficultyValues.QuestRewardValueFactor;

            DifficultyValues.RaidLootPointsFactor = serverGlobalData.difficultyValues.RaidLootPointsFactor;

            DifficultyValues.TradePriceFactorLoss = serverGlobalData.difficultyValues.TradePriceFactorLoss;

            DifficultyValues.MaintenanceCostFactor = serverGlobalData.difficultyValues.MaintenanceCostFactor;

            DifficultyValues.ScariaRotChance = serverGlobalData.difficultyValues.ScariaRotChance;

            DifficultyValues.EnemyDeathOnDownedChanceFactor = serverGlobalData.difficultyValues.EnemyDeathOnDownedChanceFactor;

            DifficultyValues.ColonistMoodOffset = serverGlobalData.difficultyValues.ColonistMoodOffset;

            DifficultyValues.FoodPoisonChanceFactor = serverGlobalData.difficultyValues.FoodPoisonChanceFactor;

            DifficultyValues.ManhunterChanceOnDamageFactor = serverGlobalData.difficultyValues.ManhunterChanceOnDamageFactor;

            DifficultyValues.PlayerPawnInfectionChanceFactor = serverGlobalData.difficultyValues.PlayerPawnInfectionChanceFactor;

            DifficultyValues.DiseaseIntervalFactor = serverGlobalData.difficultyValues.DiseaseIntervalFactor;

            DifficultyValues.EnemyReproductionRateFactor = serverGlobalData.difficultyValues.EnemyReproductionRateFactor;

            DifficultyValues.DeepDrillInfestationChanceFactor = serverGlobalData.difficultyValues.DeepDrillInfestationChanceFactor;

            DifficultyValues.FriendlyFireChanceFactor = serverGlobalData.difficultyValues.FriendlyFireChanceFactor;

            DifficultyValues.AllowInstantKillChance = serverGlobalData.difficultyValues.AllowInstantKillChance;

            DifficultyValues.PeacefulTemples = serverGlobalData.difficultyValues.PeacefulTemples;

            DifficultyValues.AllowCaveHives = serverGlobalData.difficultyValues.AllowCaveHives;

            DifficultyValues.UnwaveringPrisoners = serverGlobalData.difficultyValues.UnwaveringPrisoners;

            DifficultyValues.AllowTraps = serverGlobalData.difficultyValues.AllowTraps;

            DifficultyValues.AllowTurrets = serverGlobalData.difficultyValues.AllowTurrets;

            DifficultyValues.AllowMortars = serverGlobalData.difficultyValues.AllowMortars;

            DifficultyValues.ClassicMortars = serverGlobalData.difficultyValues.ClassicMortars;

            DifficultyValues.AdaptationEffectFactor = serverGlobalData.difficultyValues.AdaptationEffectFactor;

            DifficultyValues.AdaptationGrowthRateFactorOverZero = serverGlobalData.difficultyValues.AdaptationGrowthRateFactorOverZero;

            DifficultyValues.FixedWealthMode = serverGlobalData.difficultyValues.FixedWealthMode;

            DifficultyValues.LowPopConversionBoost = serverGlobalData.difficultyValues.LowPopConversionBoost;

            DifficultyValues.NoBabiesOrChildren = serverGlobalData.difficultyValues.NoBabiesOrChildren;

            DifficultyValues.BabiesAreHealthy = serverGlobalData.difficultyValues.BabiesAreHealthy;

            DifficultyValues.ChildRaidersAllowed = serverGlobalData.difficultyValues.ChildRaidersAllowed;

            DifficultyValues.ChildAgingRate = serverGlobalData.difficultyValues.ChildAgingRate;

            DifficultyValues.AdultAgingRate = serverGlobalData.difficultyValues.AdultAgingRate;

            DifficultyValues.WastepackInfestationChanceFactor = serverGlobalData.difficultyValues.WastepackInfestationChanceFactor;
        }

        public static void SendCustomDifficulty()
        {
            DifficultyData difficultyData = new DifficultyData();

            difficultyData.ThreatScale = Current.Game.storyteller.difficulty.threatScale;

            difficultyData.AllowBigThreats = Current.Game.storyteller.difficulty.allowBigThreats;

            difficultyData.AllowViolentQuests = Current.Game.storyteller.difficulty.allowViolentQuests;

            difficultyData.AllowIntroThreats = Current.Game.storyteller.difficulty.allowIntroThreats;

            difficultyData.PredatorsHuntHumanlikes = Current.Game.storyteller.difficulty.predatorsHuntHumanlikes;

            difficultyData.AllowExtremeWeatherIncidents = Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents;

            difficultyData.CropYieldFactor = Current.Game.storyteller.difficulty.cropYieldFactor;

            difficultyData.MineYieldFactor = Current.Game.storyteller.difficulty.mineYieldFactor;

            difficultyData.ButcherYieldFactor = Current.Game.storyteller.difficulty.butcherYieldFactor;

            difficultyData.ResearchSpeedFactor = Current.Game.storyteller.difficulty.researchSpeedFactor;

            difficultyData.QuestRewardValueFactor = Current.Game.storyteller.difficulty.questRewardValueFactor;

            difficultyData.RaidLootPointsFactor = Current.Game.storyteller.difficulty.raidLootPointsFactor;

            difficultyData.TradePriceFactorLoss = Current.Game.storyteller.difficulty.tradePriceFactorLoss;

            difficultyData.MaintenanceCostFactor = Current.Game.storyteller.difficulty.maintenanceCostFactor;

            difficultyData.ScariaRotChance = Current.Game.storyteller.difficulty.scariaRotChance;

            difficultyData.EnemyDeathOnDownedChanceFactor = Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor;

            difficultyData.ColonistMoodOffset = Current.Game.storyteller.difficulty.colonistMoodOffset;

            difficultyData.FoodPoisonChanceFactor = Current.Game.storyteller.difficulty.foodPoisonChanceFactor;

            difficultyData.ManhunterChanceOnDamageFactor = Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor;

            difficultyData.PlayerPawnInfectionChanceFactor = Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor;

            difficultyData.DiseaseIntervalFactor = Current.Game.storyteller.difficulty.diseaseIntervalFactor;

            difficultyData.EnemyReproductionRateFactor = Current.Game.storyteller.difficulty.enemyReproductionRateFactor;

            difficultyData.DeepDrillInfestationChanceFactor = Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor;

            difficultyData.FriendlyFireChanceFactor = Current.Game.storyteller.difficulty.friendlyFireChanceFactor;

            difficultyData.AllowInstantKillChance = Current.Game.storyteller.difficulty.allowInstantKillChance;

            difficultyData.PeacefulTemples = Current.Game.storyteller.difficulty.peacefulTemples;

            difficultyData.AllowCaveHives = Current.Game.storyteller.difficulty.allowCaveHives;

            difficultyData.UnwaveringPrisoners = Current.Game.storyteller.difficulty.unwaveringPrisoners;

            difficultyData.AllowTraps = Current.Game.storyteller.difficulty.allowTraps;

            difficultyData.AllowTurrets = Current.Game.storyteller.difficulty.allowTurrets;

            difficultyData.AllowMortars = Current.Game.storyteller.difficulty.allowMortars;

            difficultyData.ClassicMortars = Current.Game.storyteller.difficulty.classicMortars;

            difficultyData.AdaptationEffectFactor = Current.Game.storyteller.difficulty.adaptationEffectFactor;

            difficultyData.AdaptationGrowthRateFactorOverZero = Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero;

            difficultyData.FixedWealthMode = Current.Game.storyteller.difficulty.fixedWealthMode;

            difficultyData.LowPopConversionBoost = Current.Game.storyteller.difficulty.lowPopConversionBoost;

            difficultyData.NoBabiesOrChildren = Current.Game.storyteller.difficulty.noBabiesOrChildren;

            difficultyData.BabiesAreHealthy = Current.Game.storyteller.difficulty.babiesAreHealthy;

            difficultyData.ChildRaidersAllowed = Current.Game.storyteller.difficulty.childRaidersAllowed;

            difficultyData.ChildAgingRate = Current.Game.storyteller.difficulty.childAgingRate;

            difficultyData.AdultAgingRate = Current.Game.storyteller.difficulty.adultAgingRate;

            difficultyData.WastepackInfestationChanceFactor = Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.CustomDifficultyPacket), difficultyData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void EnforceCustomDifficulty()
        {
            if (!DifficultyValues.UseCustomDifficulty) return;
            else
            {
                Current.Game.storyteller.difficulty.threatScale = DifficultyValues.ThreatScale;

                Current.Game.storyteller.difficulty.allowBigThreats = DifficultyValues.AllowBigThreats;

                Current.Game.storyteller.difficulty.allowViolentQuests = DifficultyValues.AllowViolentQuests;

                Current.Game.storyteller.difficulty.allowIntroThreats = DifficultyValues.AllowIntroThreats;

                Current.Game.storyteller.difficulty.predatorsHuntHumanlikes = DifficultyValues.PredatorsHuntHumanlikes;

                Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents = DifficultyValues.AllowExtremeWeatherIncidents;

                Current.Game.storyteller.difficulty.cropYieldFactor = DifficultyValues.CropYieldFactor;

                Current.Game.storyteller.difficulty.mineYieldFactor = DifficultyValues.MineYieldFactor;

                Current.Game.storyteller.difficulty.butcherYieldFactor = DifficultyValues.ButcherYieldFactor;

                Current.Game.storyteller.difficulty.researchSpeedFactor = DifficultyValues.ResearchSpeedFactor;

                Current.Game.storyteller.difficulty.questRewardValueFactor = DifficultyValues.QuestRewardValueFactor;

                Current.Game.storyteller.difficulty.raidLootPointsFactor = DifficultyValues.RaidLootPointsFactor;

                Current.Game.storyteller.difficulty.tradePriceFactorLoss = DifficultyValues.TradePriceFactorLoss;

                Current.Game.storyteller.difficulty.maintenanceCostFactor = DifficultyValues.MaintenanceCostFactor;

                Current.Game.storyteller.difficulty.scariaRotChance = DifficultyValues.ScariaRotChance;

                Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor = DifficultyValues.EnemyDeathOnDownedChanceFactor;

                Current.Game.storyteller.difficulty.colonistMoodOffset = DifficultyValues.ColonistMoodOffset;

                Current.Game.storyteller.difficulty.foodPoisonChanceFactor = DifficultyValues.FoodPoisonChanceFactor;

                Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor = DifficultyValues.ManhunterChanceOnDamageFactor;

                Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor = DifficultyValues.PlayerPawnInfectionChanceFactor;

                Current.Game.storyteller.difficulty.diseaseIntervalFactor = DifficultyValues.DiseaseIntervalFactor;

                Current.Game.storyteller.difficulty.enemyReproductionRateFactor = DifficultyValues.EnemyReproductionRateFactor;

                Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor = DifficultyValues.DeepDrillInfestationChanceFactor;

                Current.Game.storyteller.difficulty.friendlyFireChanceFactor = DifficultyValues.FriendlyFireChanceFactor;

                Current.Game.storyteller.difficulty.allowInstantKillChance = DifficultyValues.AllowInstantKillChance;

                Current.Game.storyteller.difficulty.peacefulTemples = DifficultyValues.PeacefulTemples;

                Current.Game.storyteller.difficulty.allowCaveHives = DifficultyValues.AllowCaveHives;

                Current.Game.storyteller.difficulty.unwaveringPrisoners = DifficultyValues.UnwaveringPrisoners;

                Current.Game.storyteller.difficulty.allowTraps = DifficultyValues.AllowTraps;

                Current.Game.storyteller.difficulty.allowTurrets = DifficultyValues.AllowTurrets;

                Current.Game.storyteller.difficulty.allowMortars = DifficultyValues.AllowMortars;

                Current.Game.storyteller.difficulty.classicMortars = DifficultyValues.ClassicMortars;

                Current.Game.storyteller.difficulty.adaptationEffectFactor = DifficultyValues.AdaptationEffectFactor;

                Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero = DifficultyValues.AdaptationGrowthRateFactorOverZero;

                Current.Game.storyteller.difficulty.fixedWealthMode = DifficultyValues.FixedWealthMode;

                Current.Game.storyteller.difficulty.lowPopConversionBoost = DifficultyValues.LowPopConversionBoost;

                Current.Game.storyteller.difficulty.noBabiesOrChildren = DifficultyValues.NoBabiesOrChildren;

                Current.Game.storyteller.difficulty.babiesAreHealthy = DifficultyValues.BabiesAreHealthy;

                Current.Game.storyteller.difficulty.childRaidersAllowed = DifficultyValues.ChildRaidersAllowed;

                Current.Game.storyteller.difficulty.childAgingRate = DifficultyValues.ChildAgingRate;

                Current.Game.storyteller.difficulty.adultAgingRate = DifficultyValues.AdultAgingRate;

                Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor = DifficultyValues.WastepackInfestationChanceFactor;
            }
        }
    }

    public static class DifficultyValues
    {
        public static bool UseCustomDifficulty;

        public static float ThreatScale;

        public static bool AllowBigThreats;

        public static bool AllowViolentQuests;

        public static bool AllowIntroThreats;

        public static bool PredatorsHuntHumanlikes;

        public static bool AllowExtremeWeatherIncidents;

        public static float CropYieldFactor;

        public static float MineYieldFactor;

        public static float ButcherYieldFactor;

        public static float ResearchSpeedFactor;

        public static float QuestRewardValueFactor;

        public static float RaidLootPointsFactor;

        public static float TradePriceFactorLoss;

        public static float MaintenanceCostFactor;

        public static float ScariaRotChance;

        public static float EnemyDeathOnDownedChanceFactor;

        public static float ColonistMoodOffset;

        public static float FoodPoisonChanceFactor;

        public static float ManhunterChanceOnDamageFactor;

        public static float PlayerPawnInfectionChanceFactor;

        public static float DiseaseIntervalFactor;

        public static float EnemyReproductionRateFactor;

        public static float DeepDrillInfestationChanceFactor;

        public static float FriendlyFireChanceFactor;

        public static float AllowInstantKillChance;

        public static bool PeacefulTemples;

        public static bool AllowCaveHives;

        public static bool UnwaveringPrisoners;

        public static bool AllowTraps;

        public static bool AllowTurrets;

        public static bool AllowMortars;

        public static bool ClassicMortars;

        public static float AdaptationEffectFactor;

        public static float AdaptationGrowthRateFactorOverZero;

        public static bool FixedWealthMode;

        public static float LowPopConversionBoost;

        public static bool NoBabiesOrChildren;

        public static bool BabiesAreHealthy;

        public static bool ChildRaidersAllowed;

        public static float ChildAgingRate;

        public static float AdultAgingRate;

        public static float WastepackInfestationChanceFactor;
    }
}
