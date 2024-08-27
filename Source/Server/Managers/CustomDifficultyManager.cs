using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class CustomDifficultyManager
    {
        public static void ParseDifficultyPacket(ServerClient client, Packet packet)
        {
            DifficultyData difficultyData = Serializer.ConvertBytesToObject<DifficultyData>(packet.contents);
            SetCustomDifficulty(client, difficultyData);
        }

        public static void SetCustomDifficulty(ServerClient client, DifficultyData difficultyData)
        {
            if (!client.userFile.IsAdmin) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to set the custom difficulty while not being an admin");
            else
            {
                Master.difficultyValues.ThreatScale = difficultyData.ThreatScale;

                Master.difficultyValues.AllowBigThreats = difficultyData.AllowBigThreats;

                Master.difficultyValues.AllowViolentQuests = difficultyData.AllowViolentQuests;

                Master.difficultyValues.AllowIntroThreats = difficultyData.AllowIntroThreats;

                Master.difficultyValues.PredatorsHuntHumanlikes = difficultyData.PredatorsHuntHumanlikes;

                Master.difficultyValues.AllowExtremeWeatherIncidents = difficultyData.AllowExtremeWeatherIncidents;

                Master.difficultyValues.CropYieldFactor = difficultyData.CropYieldFactor;

                Master.difficultyValues.MineYieldFactor = difficultyData.MineYieldFactor;

                Master.difficultyValues.ButcherYieldFactor = difficultyData.ButcherYieldFactor;

                Master.difficultyValues.ResearchSpeedFactor = difficultyData.ResearchSpeedFactor;

                Master.difficultyValues.QuestRewardValueFactor = difficultyData.QuestRewardValueFactor;

                Master.difficultyValues.RaidLootPointsFactor = difficultyData.RaidLootPointsFactor;

                Master.difficultyValues.TradePriceFactorLoss = difficultyData.TradePriceFactorLoss;

                Master.difficultyValues.MaintenanceCostFactor = difficultyData.MaintenanceCostFactor;

                Master.difficultyValues.ScariaRotChance = difficultyData.ScariaRotChance;

                Master.difficultyValues.EnemyDeathOnDownedChanceFactor = difficultyData.EnemyDeathOnDownedChanceFactor;

                Master.difficultyValues.ColonistMoodOffset = difficultyData.ColonistMoodOffset;

                Master.difficultyValues.FoodPoisonChanceFactor = difficultyData.FoodPoisonChanceFactor;

                Master.difficultyValues.ManhunterChanceOnDamageFactor = difficultyData.ManhunterChanceOnDamageFactor;

                Master.difficultyValues.PlayerPawnInfectionChanceFactor = difficultyData.PlayerPawnInfectionChanceFactor;

                Master.difficultyValues.DiseaseIntervalFactor = difficultyData.DiseaseIntervalFactor;

                Master.difficultyValues.EnemyReproductionRateFactor = difficultyData.EnemyReproductionRateFactor;

                Master.difficultyValues.DeepDrillInfestationChanceFactor = difficultyData.DeepDrillInfestationChanceFactor;

                Master.difficultyValues.FriendlyFireChanceFactor = difficultyData.FriendlyFireChanceFactor;

                Master.difficultyValues.AllowInstantKillChance = difficultyData.AllowInstantKillChance;

                Master.difficultyValues.PeacefulTemples = difficultyData.PeacefulTemples;

                Master.difficultyValues.AllowCaveHives = difficultyData.AllowCaveHives;

                Master.difficultyValues.UnwaveringPrisoners = difficultyData.UnwaveringPrisoners;

                Master.difficultyValues.AllowTraps = difficultyData.AllowTraps;

                Master.difficultyValues.AllowTurrets = difficultyData.AllowTurrets;

                Master.difficultyValues.AllowMortars = difficultyData.AllowMortars;

                Master.difficultyValues.ClassicMortars = difficultyData.ClassicMortars;

                Master.difficultyValues.AdaptationEffectFactor = difficultyData.AdaptationEffectFactor;

                Master.difficultyValues.AdaptationGrowthRateFactorOverZero = difficultyData.AdaptationGrowthRateFactorOverZero;

                Master.difficultyValues.FixedWealthMode = difficultyData.FixedWealthMode;

                Master.difficultyValues.LowPopConversionBoost = difficultyData.LowPopConversionBoost;

                Master.difficultyValues.NoBabiesOrChildren = difficultyData.NoBabiesOrChildren;

                Master.difficultyValues.BabiesAreHealthy = difficultyData.BabiesAreHealthy;

                Master.difficultyValues.ChildRaidersAllowed = difficultyData.ChildRaidersAllowed;

                Master.difficultyValues.ChildAgingRate = difficultyData.ChildAgingRate;

                Master.difficultyValues.AdultAgingRate = difficultyData.AdultAgingRate;

                Master.difficultyValues.WastepackInfestationChanceFactor = difficultyData.WastepackInfestationChanceFactor;

                Logger.Warning($"[Set difficulty] > {client.userFile.Username}");

                Main_.SaveValueFile(ServerFileMode.Difficulty);
            }
        }
    }
}
