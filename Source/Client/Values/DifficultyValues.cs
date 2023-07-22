using Shared.JSON;
using Verse;

namespace RimworldTogether
{
    public static class DifficultyValues
    {
        public static bool UseCustomDifficulty;

        private static float ThreatScale;

        private static bool AllowBigThreats;

        private static bool AllowViolentQuests;

        private static bool AllowIntroThreats;

        private static bool PredatorsHuntHumanlikes;

        private static bool AllowExtremeWeatherIncidents;

        private static float CropYieldFactor;

        private static float MineYieldFactor;

        private static float ButcherYieldFactor;

        private static float ResearchSpeedFactor;

        private static float QuestRewardValueFactor;

        private static float RaidLootPointsFactor;

        private static float TradePriceFactorLoss;

        private static float MaintenanceCostFactor;

        private static float ScariaRotChance;

        private static float EnemyDeathOnDownedChanceFactor;

        private static float ColonistMoodOffset;

        private static float FoodPoisonChanceFactor;

        private static float ManhunterChanceOnDamageFactor;

        private static float PlayerPawnInfectionChanceFactor;

        private static float DiseaseIntervalFactor;

        private static float DeepDrillInfestationChanceFactor;

        private static float FriendlyFireChanceFactor;

        private static float AllowInstantKillChance;

        private static bool AllowTraps;

        private static bool AllowTurrets;

        private static bool AllowMortars;

        private static float AdaptationEffectFactor;

        private static float AdaptationGrowthRateFactorOverZero;

        private static bool FixedWealthMode;

        private static float LowPopConversionBoost;

        private static bool NoBabiesOrChildren;

        private static bool babiesAreHealthy;

        private static bool ChildRaidersAllowed;

        private static float ChildAgingRate;

        private static float AdultAgingRate;

        private static float WastepackInfestationChanceFactor;

        public static void SetDifficultyValues(ServerOverallJSON serverOverallJSON)
        {
            UseCustomDifficulty = serverOverallJSON.UsingCustomDifficulty;
                    
            ThreatScale = serverOverallJSON.ThreatScale;

            AllowBigThreats = serverOverallJSON.AllowBigThreats;

            AllowViolentQuests = serverOverallJSON.AllowViolentQuests;

            AllowIntroThreats = serverOverallJSON.AllowIntroThreats;

            PredatorsHuntHumanlikes = serverOverallJSON.PredatorsHuntHumanlikes;

            AllowExtremeWeatherIncidents = serverOverallJSON.AllowExtremeWeatherIncidents;

            CropYieldFactor = serverOverallJSON.CropYieldFactor;

            MineYieldFactor = serverOverallJSON.MineYieldFactor;
                
            ButcherYieldFactor = serverOverallJSON.ButcherYieldFactor;

            ResearchSpeedFactor = serverOverallJSON.ResearchSpeedFactor;

            QuestRewardValueFactor = serverOverallJSON.QuestRewardValueFactor;

            RaidLootPointsFactor = serverOverallJSON.RaidLootPointsFactor;

            TradePriceFactorLoss = serverOverallJSON.TradePriceFactorLoss;

            MaintenanceCostFactor = serverOverallJSON.MaintenanceCostFactor;

            ScariaRotChance = serverOverallJSON.ScariaRotChance;

            EnemyDeathOnDownedChanceFactor = serverOverallJSON.EnemyDeathOnDownedChanceFactor;

            ColonistMoodOffset = serverOverallJSON.ColonistMoodOffset;

            FoodPoisonChanceFactor = serverOverallJSON.FoodPoisonChanceFactor;

            ManhunterChanceOnDamageFactor = serverOverallJSON.ManhunterChanceOnDamageFactor;

            PlayerPawnInfectionChanceFactor = serverOverallJSON.PlayerPawnInfectionChanceFactor;

            DiseaseIntervalFactor = serverOverallJSON.DiseaseIntervalFactor;

            DeepDrillInfestationChanceFactor = serverOverallJSON.DeepDrillInfestationChanceFactor;

            FriendlyFireChanceFactor = serverOverallJSON.FriendlyFireChanceFactor;

            AllowInstantKillChance = serverOverallJSON.AllowInstantKillChance;

            AllowTraps = serverOverallJSON.AllowTraps;

            AllowTurrets = serverOverallJSON.AllowTurrets;

            AllowMortars = serverOverallJSON.AllowMortars;

            AdaptationEffectFactor = serverOverallJSON.AdaptationEffectFactor;

            AdaptationGrowthRateFactorOverZero = serverOverallJSON.AdaptationGrowthRateFactorOverZero;

            FixedWealthMode = serverOverallJSON.FixedWealthMode;

            LowPopConversionBoost = serverOverallJSON.LowPopConversionBoost;

            NoBabiesOrChildren = serverOverallJSON.NoBabiesOrChildren;

            babiesAreHealthy = serverOverallJSON.babiesAreHealthy;

            ChildRaidersAllowed = serverOverallJSON.ChildRaidersAllowed;

            ChildAgingRate = serverOverallJSON.ChildAgingRate;

            AdultAgingRate = serverOverallJSON.AdultAgingRate;

            WastepackInfestationChanceFactor = serverOverallJSON.WastepackInfestationChanceFactor;
        }

        public static void ForceCustomDifficulty()
        {
            if (!UseCustomDifficulty) return;

            Current.Game.storyteller.difficulty.threatScale = ThreatScale;

            Current.Game.storyteller.difficulty.allowBigThreats = AllowBigThreats;

            Current.Game.storyteller.difficulty.allowViolentQuests = AllowViolentQuests;

            Current.Game.storyteller.difficulty.allowIntroThreats = AllowIntroThreats;

            Current.Game.storyteller.difficulty.predatorsHuntHumanlikes = PredatorsHuntHumanlikes;

            Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents = AllowExtremeWeatherIncidents;

            Current.Game.storyteller.difficulty.cropYieldFactor = CropYieldFactor;

            Current.Game.storyteller.difficulty.mineYieldFactor = MineYieldFactor;

            Current.Game.storyteller.difficulty.butcherYieldFactor = ButcherYieldFactor;

            Current.Game.storyteller.difficulty.researchSpeedFactor = ResearchSpeedFactor;

            Current.Game.storyteller.difficulty.questRewardValueFactor = QuestRewardValueFactor;

            Current.Game.storyteller.difficulty.raidLootPointsFactor = RaidLootPointsFactor;

            Current.Game.storyteller.difficulty.tradePriceFactorLoss = TradePriceFactorLoss;

            Current.Game.storyteller.difficulty.maintenanceCostFactor = MaintenanceCostFactor;

            Current.Game.storyteller.difficulty.scariaRotChance = ScariaRotChance;

            Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor = EnemyDeathOnDownedChanceFactor;

            Current.Game.storyteller.difficulty.colonistMoodOffset = ColonistMoodOffset;

            Current.Game.storyteller.difficulty.foodPoisonChanceFactor = FoodPoisonChanceFactor;

            Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor = ManhunterChanceOnDamageFactor;

            Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor = PlayerPawnInfectionChanceFactor;

            Current.Game.storyteller.difficulty.diseaseIntervalFactor = DiseaseIntervalFactor;

            Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor = DeepDrillInfestationChanceFactor;

            Current.Game.storyteller.difficulty.friendlyFireChanceFactor = FriendlyFireChanceFactor;

            Current.Game.storyteller.difficulty.allowInstantKillChance = AllowInstantKillChance;

            Current.Game.storyteller.difficulty.allowTraps = AllowTraps;

            Current.Game.storyteller.difficulty.allowTurrets = AllowTurrets;

            Current.Game.storyteller.difficulty.allowMortars = AllowMortars;

            Current.Game.storyteller.difficulty.adaptationEffectFactor = AdaptationEffectFactor;

            Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero = AdaptationGrowthRateFactorOverZero;

            Current.Game.storyteller.difficulty.fixedWealthMode = FixedWealthMode;

            Current.Game.storyteller.difficulty.lowPopConversionBoost = LowPopConversionBoost;

            Current.Game.storyteller.difficulty.noBabiesOrChildren = NoBabiesOrChildren;

            Current.Game.storyteller.difficulty.babiesAreHealthy = babiesAreHealthy;        

            Current.Game.storyteller.difficulty.childRaidersAllowed = ChildRaidersAllowed;

            Current.Game.storyteller.difficulty.childAgingRate = ChildAgingRate;

            Current.Game.storyteller.difficulty.adultAgingRate = AdultAgingRate;

            Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor = WastepackInfestationChanceFactor;
        }
    }
}
