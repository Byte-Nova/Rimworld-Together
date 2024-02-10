using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using Verse;

namespace RimworldTogether.GameClient.Managers
{
    public static class CustomDifficultyManager
    {
        public static void SetCustomDifficulty(ServerOverallJSON serverOverallJSON)
        {
            DifficultyValues.UseCustomDifficulty = serverOverallJSON.UsingCustomDifficulty;

            DifficultyValues.ThreatScale = serverOverallJSON.ThreatScale;

            DifficultyValues.AllowBigThreats = serverOverallJSON.AllowBigThreats;

            DifficultyValues.AllowViolentQuests = serverOverallJSON.AllowViolentQuests;

            DifficultyValues.AllowIntroThreats = serverOverallJSON.AllowIntroThreats;

            DifficultyValues.PredatorsHuntHumanlikes = serverOverallJSON.PredatorsHuntHumanlikes;

            DifficultyValues.AllowExtremeWeatherIncidents = serverOverallJSON.AllowExtremeWeatherIncidents;

            DifficultyValues.CropYieldFactor = serverOverallJSON.CropYieldFactor;

            DifficultyValues.MineYieldFactor = serverOverallJSON.MineYieldFactor;

            DifficultyValues.ButcherYieldFactor = serverOverallJSON.ButcherYieldFactor;

            DifficultyValues.ResearchSpeedFactor = serverOverallJSON.ResearchSpeedFactor;

            DifficultyValues.QuestRewardValueFactor = serverOverallJSON.QuestRewardValueFactor;

            DifficultyValues.RaidLootPointsFactor = serverOverallJSON.RaidLootPointsFactor;

            DifficultyValues.TradePriceFactorLoss = serverOverallJSON.TradePriceFactorLoss;

            DifficultyValues.MaintenanceCostFactor = serverOverallJSON.MaintenanceCostFactor;

            DifficultyValues.ScariaRotChance = serverOverallJSON.ScariaRotChance;

            DifficultyValues.EnemyDeathOnDownedChanceFactor = serverOverallJSON.EnemyDeathOnDownedChanceFactor;

            DifficultyValues.ColonistMoodOffset = serverOverallJSON.ColonistMoodOffset;

            DifficultyValues.FoodPoisonChanceFactor = serverOverallJSON.FoodPoisonChanceFactor;

            DifficultyValues.ManhunterChanceOnDamageFactor = serverOverallJSON.ManhunterChanceOnDamageFactor;

            DifficultyValues.PlayerPawnInfectionChanceFactor = serverOverallJSON.PlayerPawnInfectionChanceFactor;

            DifficultyValues.DiseaseIntervalFactor = serverOverallJSON.DiseaseIntervalFactor;

            DifficultyValues.DeepDrillInfestationChanceFactor = serverOverallJSON.DeepDrillInfestationChanceFactor;

            DifficultyValues.FriendlyFireChanceFactor = serverOverallJSON.FriendlyFireChanceFactor;

            DifficultyValues.AllowInstantKillChance = serverOverallJSON.AllowInstantKillChance;

            DifficultyValues.AllowTraps = serverOverallJSON.AllowTraps;

            DifficultyValues.AllowTurrets = serverOverallJSON.AllowTurrets;

            DifficultyValues.AllowMortars = serverOverallJSON.AllowMortars;

            DifficultyValues.AdaptationEffectFactor = serverOverallJSON.AdaptationEffectFactor;

            DifficultyValues.AdaptationGrowthRateFactorOverZero = serverOverallJSON.AdaptationGrowthRateFactorOverZero;

            DifficultyValues.FixedWealthMode = serverOverallJSON.FixedWealthMode;

            DifficultyValues.LowPopConversionBoost = serverOverallJSON.LowPopConversionBoost;

            DifficultyValues.NoBabiesOrChildren = serverOverallJSON.NoBabiesOrChildren;

            DifficultyValues.BabiesAreHealthy = serverOverallJSON.BabiesAreHealthy;

            DifficultyValues.ChildRaidersAllowed = serverOverallJSON.ChildRaidersAllowed;

            DifficultyValues.ChildAgingRate = serverOverallJSON.ChildAgingRate;

            DifficultyValues.AdultAgingRate = serverOverallJSON.AdultAgingRate;

            DifficultyValues.WastepackInfestationChanceFactor = serverOverallJSON.WastepackInfestationChanceFactor;
        }

        public static void SendCustomDifficulty()
        {
            DifficultyValuesJSON difficultyValuesJSON = new DifficultyValuesJSON();

            difficultyValuesJSON.ThreatScale = Current.Game.storyteller.difficulty.threatScale;

            difficultyValuesJSON.AllowBigThreats = Current.Game.storyteller.difficulty.allowBigThreats;

            difficultyValuesJSON.AllowViolentQuests = Current.Game.storyteller.difficulty.allowViolentQuests;

            difficultyValuesJSON.AllowIntroThreats = Current.Game.storyteller.difficulty.allowIntroThreats;

            difficultyValuesJSON.PredatorsHuntHumanlikes = Current.Game.storyteller.difficulty.predatorsHuntHumanlikes;

            difficultyValuesJSON.AllowExtremeWeatherIncidents = Current.Game.storyteller.difficulty.allowExtremeWeatherIncidents;

            difficultyValuesJSON.CropYieldFactor = Current.Game.storyteller.difficulty.cropYieldFactor;

            difficultyValuesJSON.MineYieldFactor = Current.Game.storyteller.difficulty.mineYieldFactor;

            difficultyValuesJSON.ButcherYieldFactor = Current.Game.storyteller.difficulty.butcherYieldFactor;

            difficultyValuesJSON.ResearchSpeedFactor = Current.Game.storyteller.difficulty.researchSpeedFactor;

            difficultyValuesJSON.QuestRewardValueFactor = Current.Game.storyteller.difficulty.questRewardValueFactor;

            difficultyValuesJSON.RaidLootPointsFactor = Current.Game.storyteller.difficulty.raidLootPointsFactor;

            difficultyValuesJSON.TradePriceFactorLoss = Current.Game.storyteller.difficulty.tradePriceFactorLoss;

            difficultyValuesJSON.MaintenanceCostFactor = Current.Game.storyteller.difficulty.maintenanceCostFactor;

            difficultyValuesJSON.ScariaRotChance = Current.Game.storyteller.difficulty.scariaRotChance;

            difficultyValuesJSON.EnemyDeathOnDownedChanceFactor = Current.Game.storyteller.difficulty.enemyDeathOnDownedChanceFactor;

            difficultyValuesJSON.ColonistMoodOffset = Current.Game.storyteller.difficulty.colonistMoodOffset;

            difficultyValuesJSON.FoodPoisonChanceFactor = Current.Game.storyteller.difficulty.foodPoisonChanceFactor;

            difficultyValuesJSON.ManhunterChanceOnDamageFactor = Current.Game.storyteller.difficulty.manhunterChanceOnDamageFactor;

            difficultyValuesJSON.PlayerPawnInfectionChanceFactor = Current.Game.storyteller.difficulty.playerPawnInfectionChanceFactor;

            difficultyValuesJSON.DiseaseIntervalFactor = Current.Game.storyteller.difficulty.diseaseIntervalFactor;

            difficultyValuesJSON.DeepDrillInfestationChanceFactor = Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor;

            difficultyValuesJSON.FriendlyFireChanceFactor = Current.Game.storyteller.difficulty.friendlyFireChanceFactor;

            difficultyValuesJSON.AllowInstantKillChance = Current.Game.storyteller.difficulty.allowInstantKillChance;

            difficultyValuesJSON.AllowTraps = Current.Game.storyteller.difficulty.allowTraps;

            difficultyValuesJSON.AllowTurrets = Current.Game.storyteller.difficulty.allowTurrets;

            difficultyValuesJSON.AllowMortars = Current.Game.storyteller.difficulty.allowMortars;

            difficultyValuesJSON.AdaptationEffectFactor = Current.Game.storyteller.difficulty.adaptationEffectFactor;

            difficultyValuesJSON.AdaptationGrowthRateFactorOverZero = Current.Game.storyteller.difficulty.adaptationGrowthRateFactorOverZero;

            difficultyValuesJSON.FixedWealthMode = Current.Game.storyteller.difficulty.fixedWealthMode;

            difficultyValuesJSON.LowPopConversionBoost = Current.Game.storyteller.difficulty.lowPopConversionBoost;

            difficultyValuesJSON.NoBabiesOrChildren = Current.Game.storyteller.difficulty.noBabiesOrChildren;

            difficultyValuesJSON.BabiesAreHealthy = Current.Game.storyteller.difficulty.babiesAreHealthy;

            difficultyValuesJSON.ChildRaidersAllowed = Current.Game.storyteller.difficulty.childRaidersAllowed;

            difficultyValuesJSON.ChildAgingRate = Current.Game.storyteller.difficulty.childAgingRate;

            difficultyValuesJSON.AdultAgingRate = Current.Game.storyteller.difficulty.adultAgingRate;

            difficultyValuesJSON.WastepackInfestationChanceFactor = Current.Game.storyteller.difficulty.wastepackInfestationChanceFactor;

            Packet packet = Packet.CreatePacketFromJSON("CustomDifficultyPacket", difficultyValuesJSON);
            Network.Network.serverListener.SendData(packet);
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

                Current.Game.storyteller.difficulty.deepDrillInfestationChanceFactor = DifficultyValues.DeepDrillInfestationChanceFactor;

                Current.Game.storyteller.difficulty.friendlyFireChanceFactor = DifficultyValues.FriendlyFireChanceFactor;

                Current.Game.storyteller.difficulty.allowInstantKillChance = DifficultyValues.AllowInstantKillChance;

                Current.Game.storyteller.difficulty.allowTraps = DifficultyValues.AllowTraps;

                Current.Game.storyteller.difficulty.allowTurrets = DifficultyValues.AllowTurrets;

                Current.Game.storyteller.difficulty.allowMortars = DifficultyValues.AllowMortars;

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

        public static float DeepDrillInfestationChanceFactor;

        public static float FriendlyFireChanceFactor;

        public static float AllowInstantKillChance;

        public static bool AllowTraps;

        public static bool AllowTurrets;

        public static bool AllowMortars;

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
