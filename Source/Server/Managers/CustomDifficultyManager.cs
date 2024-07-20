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
                DifficultyValuesFile newDifficultyValues = new DifficultyValuesFile();

                newDifficultyValues.UseCustomDifficulty = Master.difficultyValues.UseCustomDifficulty;

                newDifficultyValues.ThreatScale = difficultyData.ThreatScale;

                newDifficultyValues.AllowBigThreats = difficultyData.AllowBigThreats;

                newDifficultyValues.AllowViolentQuests = difficultyData.AllowViolentQuests;

                newDifficultyValues.AllowIntroThreats = difficultyData.AllowIntroThreats;

                newDifficultyValues.PredatorsHuntHumanlikes = difficultyData.PredatorsHuntHumanlikes;

                newDifficultyValues.AllowExtremeWeatherIncidents = difficultyData.AllowExtremeWeatherIncidents;

                newDifficultyValues.CropYieldFactor = difficultyData.CropYieldFactor;

                newDifficultyValues.MineYieldFactor = difficultyData.MineYieldFactor;

                newDifficultyValues.ButcherYieldFactor = difficultyData.ButcherYieldFactor;

                newDifficultyValues.ResearchSpeedFactor = difficultyData.ResearchSpeedFactor;

                newDifficultyValues.QuestRewardValueFactor = difficultyData.QuestRewardValueFactor;

                newDifficultyValues.RaidLootPointsFactor = difficultyData.RaidLootPointsFactor;

                newDifficultyValues.TradePriceFactorLoss = difficultyData.TradePriceFactorLoss;

                newDifficultyValues.MaintenanceCostFactor = difficultyData.MaintenanceCostFactor;

                newDifficultyValues.ScariaRotChance = difficultyData.ScariaRotChance;

                newDifficultyValues.EnemyDeathOnDownedChanceFactor = difficultyData.EnemyDeathOnDownedChanceFactor;

                newDifficultyValues.ColonistMoodOffset = difficultyData.ColonistMoodOffset;

                newDifficultyValues.FoodPoisonChanceFactor = difficultyData.FoodPoisonChanceFactor;

                newDifficultyValues.ManhunterChanceOnDamageFactor = difficultyData.ManhunterChanceOnDamageFactor;

                newDifficultyValues.PlayerPawnInfectionChanceFactor = difficultyData.PlayerPawnInfectionChanceFactor;

                newDifficultyValues.DiseaseIntervalFactor = difficultyData.DiseaseIntervalFactor;

                newDifficultyValues.EnemyReproductionRateFactor = difficultyData.EnemyReproductionRateFactor;

                newDifficultyValues.DeepDrillInfestationChanceFactor = difficultyData.DeepDrillInfestationChanceFactor;

                newDifficultyValues.FriendlyFireChanceFactor = difficultyData.FriendlyFireChanceFactor;

                newDifficultyValues.AllowInstantKillChance = difficultyData.AllowInstantKillChance;

                newDifficultyValues.PeacefulTemples = difficultyData.PeacefulTemples;

                newDifficultyValues.AllowCaveHives = difficultyData.AllowCaveHives;

                newDifficultyValues.UnwaveringPrisoners = difficultyData.UnwaveringPrisoners;

                newDifficultyValues.AllowTraps = difficultyData.AllowTraps;

                newDifficultyValues.AllowTurrets = difficultyData.AllowTurrets;

                newDifficultyValues.AllowMortars = difficultyData.AllowMortars;

                newDifficultyValues.ClassicMortars = difficultyData.ClassicMortars;

                newDifficultyValues.AdaptationEffectFactor = difficultyData.AdaptationEffectFactor;

                newDifficultyValues.AdaptationGrowthRateFactorOverZero = difficultyData.AdaptationGrowthRateFactorOverZero;

                newDifficultyValues.FixedWealthMode = difficultyData.FixedWealthMode;

                newDifficultyValues.LowPopConversionBoost = difficultyData.LowPopConversionBoost;

                newDifficultyValues.NoBabiesOrChildren = difficultyData.NoBabiesOrChildren;

                newDifficultyValues.BabiesAreHealthy = difficultyData.BabiesAreHealthy;

                newDifficultyValues.ChildRaidersAllowed = difficultyData.ChildRaidersAllowed;

                newDifficultyValues.ChildAgingRate = difficultyData.ChildAgingRate;

                newDifficultyValues.AdultAgingRate = difficultyData.AdultAgingRate;

                newDifficultyValues.WastepackInfestationChanceFactor = difficultyData.WastepackInfestationChanceFactor;

                Logger.Warning($"[Set difficulty] > {client.userFile.Username}");

                Master.SaveValueFile(ServerFileMode.Difficulty);
            }
        }
    }
}
