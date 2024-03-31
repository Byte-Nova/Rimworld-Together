using Shared;

namespace GameServer
{
    public static class CustomDifficultyManager
    {
        public static void ParseDifficultyPacket(ServerClient client, Packet packet)
        {
            DifficultyValuesJSON difficultyValuesJSON = (DifficultyValuesJSON)Serializer.ConvertBytesToObject(packet.contents);
            SetCustomDifficulty(client, difficultyValuesJSON);
        }

        public static void SetCustomDifficulty(ServerClient client, DifficultyValuesJSON difficultyValuesJSON)
        {
            if (!client.isAdmin) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to set the custom difficulty while not being an admin");
            else
            {
                DifficultyValuesFile newDifficultyValues = new DifficultyValuesFile();

                newDifficultyValues.ThreatScale = difficultyValuesJSON.ThreatScale;

                newDifficultyValues.AllowBigThreats = difficultyValuesJSON.AllowBigThreats;

                newDifficultyValues.AllowViolentQuests = difficultyValuesJSON.AllowViolentQuests;

                newDifficultyValues.AllowIntroThreats = difficultyValuesJSON.AllowIntroThreats;

                newDifficultyValues.PredatorsHuntHumanlikes = difficultyValuesJSON.PredatorsHuntHumanlikes;

                newDifficultyValues.AllowExtremeWeatherIncidents = difficultyValuesJSON.AllowExtremeWeatherIncidents;

                newDifficultyValues.CropYieldFactor = difficultyValuesJSON.CropYieldFactor;

                newDifficultyValues.MineYieldFactor = difficultyValuesJSON.MineYieldFactor;

                newDifficultyValues.ButcherYieldFactor = difficultyValuesJSON.ButcherYieldFactor;

                newDifficultyValues.ResearchSpeedFactor = difficultyValuesJSON.ResearchSpeedFactor;

                newDifficultyValues.QuestRewardValueFactor = difficultyValuesJSON.QuestRewardValueFactor;

                newDifficultyValues.RaidLootPointsFactor = difficultyValuesJSON.RaidLootPointsFactor;

                newDifficultyValues.TradePriceFactorLoss = difficultyValuesJSON.TradePriceFactorLoss;

                newDifficultyValues.MaintenanceCostFactor = difficultyValuesJSON.MaintenanceCostFactor;

                newDifficultyValues.ScariaRotChance = difficultyValuesJSON.ScariaRotChance;

                newDifficultyValues.EnemyDeathOnDownedChanceFactor = difficultyValuesJSON.EnemyDeathOnDownedChanceFactor;

                newDifficultyValues.ColonistMoodOffset = difficultyValuesJSON.ColonistMoodOffset;

                newDifficultyValues.FoodPoisonChanceFactor = difficultyValuesJSON.FoodPoisonChanceFactor;

                newDifficultyValues.ManhunterChanceOnDamageFactor = difficultyValuesJSON.ManhunterChanceOnDamageFactor;

                newDifficultyValues.PlayerPawnInfectionChanceFactor = difficultyValuesJSON.PlayerPawnInfectionChanceFactor;

                newDifficultyValues.DiseaseIntervalFactor = difficultyValuesJSON.DiseaseIntervalFactor;

                newDifficultyValues.EnemyReproductionRateFactor = difficultyValuesJSON.EnemyReproductionRateFactor;

                newDifficultyValues.DeepDrillInfestationChanceFactor = difficultyValuesJSON.DeepDrillInfestationChanceFactor;

                newDifficultyValues.FriendlyFireChanceFactor = difficultyValuesJSON.FriendlyFireChanceFactor;

                newDifficultyValues.AllowInstantKillChance = difficultyValuesJSON.AllowInstantKillChance;

                newDifficultyValues.PeacefulTemples = difficultyValuesJSON.PeacefulTemples;

                newDifficultyValues.AllowCaveHives = difficultyValuesJSON.AllowCaveHives;

                newDifficultyValues.UnwaveringPrisoners = difficultyValuesJSON.UnwaveringPrisoners;

                newDifficultyValues.AllowTraps = difficultyValuesJSON.AllowTraps;

                newDifficultyValues.AllowTurrets = difficultyValuesJSON.AllowTurrets;

                newDifficultyValues.AllowMortars = difficultyValuesJSON.AllowMortars;

                newDifficultyValues.ClassicMortars = difficultyValuesJSON.ClassicMortars;

                newDifficultyValues.AdaptationEffectFactor = difficultyValuesJSON.AdaptationEffectFactor;

                newDifficultyValues.AdaptationGrowthRateFactorOverZero = difficultyValuesJSON.AdaptationGrowthRateFactorOverZero;

                newDifficultyValues.FixedWealthMode = difficultyValuesJSON.FixedWealthMode;

                newDifficultyValues.LowPopConversionBoost = difficultyValuesJSON.LowPopConversionBoost;

                newDifficultyValues.NoBabiesOrChildren = difficultyValuesJSON.NoBabiesOrChildren;

                newDifficultyValues.BabiesAreHealthy = difficultyValuesJSON.BabiesAreHealthy;

                newDifficultyValues.ChildRaidersAllowed = difficultyValuesJSON.ChildRaidersAllowed;

                newDifficultyValues.ChildAgingRate = difficultyValuesJSON.ChildAgingRate;

                newDifficultyValues.AdultAgingRate = difficultyValuesJSON.AdultAgingRate;

                newDifficultyValues.WastepackInfestationChanceFactor = difficultyValuesJSON.WastepackInfestationChanceFactor;

                Logger.WriteToConsole($"[Set difficulty] > {client.username}", Logger.LogMode.Warning);

                SaveCustomDifficulty(newDifficultyValues);
            }
        }

        public static void SaveCustomDifficulty(DifficultyValuesFile newDifficultyValues)
        {
            string path = Path.Combine(Master.corePath, "DifficultyValues.json");

            Serializer.SerializeToFile(path, newDifficultyValues);

            LoadCustomDifficulty();
        }

        public static void LoadCustomDifficulty()
        {
            string path = Path.Combine(Master.corePath, "DifficultyValues.json");

            if (File.Exists(path)) Master.difficultyValues = Serializer.SerializeFromFile<DifficultyValuesFile>(path);
            else
            {
                Master.difficultyValues = new DifficultyValuesFile();
                Serializer.SerializeToFile(path, Master.difficultyValues);
            }

            Logger.WriteToConsole("Loaded difficulty values", Logger.LogMode.Warning);
        }
    }
}
