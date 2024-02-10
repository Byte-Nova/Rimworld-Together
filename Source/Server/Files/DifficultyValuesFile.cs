namespace RimworldTogether.GameServer.Files
{
    [Serializable]
    public class DifficultyValuesFile
    {
        public bool UseCustomDifficulty = false;

        public float ThreatScale = 1.0f;

        public bool AllowBigThreats = true;

        public bool AllowViolentQuests = true;

        public bool AllowIntroThreats = true;

        public bool PredatorsHuntHumanlikes = true;

        public bool AllowExtremeWeatherIncidents = true;

        public float CropYieldFactor = 1.0f;

        public float MineYieldFactor = 1.0f;

        public float ButcherYieldFactor = 1.0f;

        public float ResearchSpeedFactor = 1.0f;

        public float QuestRewardValueFactor = 1.0f;

        public float RaidLootPointsFactor = 1.0f;

        public float TradePriceFactorLoss = 0.0f;

        public float MaintenanceCostFactor = 1.0f;

        public float ScariaRotChance = 0.6f;

        public float EnemyDeathOnDownedChanceFactor = 1.0f;

        public float ColonistMoodOffset = 0.0f;

        public float FoodPoisonChanceFactor = 1.0f;

        public float ManhunterChanceOnDamageFactor = 1.0f;

        public float PlayerPawnInfectionChanceFactor = 1.0f;

        public float DiseaseIntervalFactor = 1.0f;

        public float DeepDrillInfestationChanceFactor = 1.0f;

        public float FriendlyFireChanceFactor = 0.4f;

        public float AllowInstantKillChance = 1.0f;

        public bool AllowTraps = true;

        public bool AllowTurrets = true;

        public bool AllowMortars = true;

        public float AdaptationEffectFactor = 0.9f;

        public float AdaptationGrowthRateFactorOverZero = 1.0f;

        public bool FixedWealthMode = false;

        public float LowPopConversionBoost = 3.0f;

        public bool NoBabiesOrChildren = false;

        public bool BabiesAreHealthy = false;

        public bool ChildRaidersAllowed = false;

        public float ChildAgingRate = 4.0f;

        public float AdultAgingRate = 1.0f;

        public float WastepackInfestationChanceFactor = 1.0f;
    }
}
