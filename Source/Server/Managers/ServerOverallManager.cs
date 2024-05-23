using Shared;

namespace GameServer
{
    public static class ServerOverallManager
    {
        public static void SendServerOveralls(ServerClient client)
        {
            ServerGlobalData globalData = new ServerGlobalData();

            globalData = GetServerValues(globalData);

            globalData = GetClientValues(client, globalData);

            globalData = GetEventCosts(globalData);

            globalData = GetSiteData(globalData);

            globalData = GetServerDifficulty(globalData);

            globalData = GetServerSettlements(client, globalData);

            globalData = GetServerSites(client, globalData);

            globalData = GetActionsCost(client, globalData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ServerValuesPacket), globalData);
            client.listener.EnqueuePacket(packet);
        }

        private static ServerGlobalData GetServerValues(ServerGlobalData globalData)
        {
            ServerValuesFile svf = Master.serverValues;
            globalData.AllowCustomScenarios = svf.AllowCustomScenarios;

            return globalData;
        }

        private static ServerGlobalData GetClientValues(ServerClient client, ServerGlobalData globalData)
        {
            globalData.isClientAdmin = client.isAdmin;
            globalData.isClientOperator = client.isOperator;
            globalData.isClientFactionMember = client.hasFaction;

            return globalData;
        }

        private static ServerGlobalData GetEventCosts(ServerGlobalData globalData)
        {
            EventValuesFile eventValues = Master.eventValues;
            globalData.RaidCost = eventValues.RaidCost;
            globalData.InfestationCost = eventValues.InfestationCost;
            globalData.MechClusterCost = eventValues.MechClusterCost;
            globalData.ToxicFalloutCost = eventValues.ToxicFalloutCost;
            globalData.ManhunterCost = eventValues.ManhunterCost;
            globalData.WandererCost = eventValues.WandererCost;
            globalData.FarmAnimalsCost = eventValues.FarmAnimalsCost;
            globalData.ShipChunkCost = eventValues.ShipChunkCost;
            globalData.TraderCaravanCost = eventValues.TraderCaravanCost;

            return globalData;
        }

        private static ServerGlobalData GetSiteData(ServerGlobalData globalData)
        {
            SiteValuesFile siteValues = Master.siteValues;
            globalData.PersonalFarmlandCost = siteValues.PersonalFarmlandCost;
            globalData.FactionFarmlandCost = siteValues.FactionFarmlandCost;
            globalData.FarmlandRewardCount = siteValues.FarmlandRewardCount;

            globalData.PersonalQuarryCost = siteValues.PersonalQuarryCost;
            globalData.FactionQuarryCost = siteValues.FactionQuarryCost;
            globalData.QuarryRewardCount = siteValues.QuarryRewardCount;

            globalData.PersonalSawmillCost = siteValues.PersonalSawmillCost;
            globalData.FactionSawmillCost = siteValues.FactionSawmillCost;
            globalData.SawmillRewardCount = siteValues.SawmillRewardCount;

            globalData.PersonalBankCost = siteValues.PersonalBankCost;
            globalData.FactionBankCost = siteValues.FactionBankCost;
            globalData.BankRewardCount = siteValues.BankRewardCount;

            globalData.PersonalLaboratoryCost = siteValues.PersonalLaboratoryCost;
            globalData.FactionLaboratoryCost = siteValues.FactionLaboratoryCost;
            globalData.LaboratoryRewardCount = siteValues.LaboratoryRewardCount;

            globalData.PersonalRefineryCost = siteValues.PersonalRefineryCost;
            globalData.FactionRefineryCost = siteValues.FactionRefineryCost;
            globalData.RefineryRewardCount = siteValues.RefineryRewardCount;

            globalData.PersonalHerbalWorkshopCost = siteValues.PersonalHerbalWorkshopCost;
            globalData.FactionHerbalWorkshopCost = siteValues.FactionHerbalWorkshopCost;
            globalData.HerbalWorkshopRewardCount = siteValues.HerbalWorkshopRewardCount;

            globalData.PersonalTextileFactoryCost = siteValues.PersonalTextileFactoryCost;
            globalData.FactionTextileFactoryCost = siteValues.FactionTextileFactoryCost;
            globalData.TextileFactoryRewardCount = siteValues.TextileFactoryRewardCount;

            globalData.PersonalFoodProcessorCost = siteValues.PersonalFoodProcessorCost;
            globalData.FactionFoodProcessorCost = siteValues.FactionFoodProcessorCost;
            globalData.FoodProcessorRewardCount = siteValues.FoodProcessorRewardCount;

            return globalData;
        }

        private static ServerGlobalData GetServerDifficulty(ServerGlobalData globalData)
        {
            DifficultyValuesFile difficultyValues = Master.difficultyValues;
            globalData.UsingCustomDifficulty = difficultyValues.UseCustomDifficulty;
            globalData.ThreatScale = difficultyValues.ThreatScale;
            globalData.ThreatScale = difficultyValues.ThreatScale;
            globalData.AllowBigThreats = difficultyValues.AllowBigThreats;
            globalData.AllowViolentQuests = difficultyValues.AllowViolentQuests;
            globalData.AllowIntroThreats = difficultyValues.AllowIntroThreats;
            globalData.PredatorsHuntHumanlikes = difficultyValues.PredatorsHuntHumanlikes;
            globalData.AllowExtremeWeatherIncidents = difficultyValues.AllowExtremeWeatherIncidents;
            globalData.CropYieldFactor = difficultyValues.CropYieldFactor;
            globalData.MineYieldFactor = difficultyValues.MineYieldFactor;
            globalData.ButcherYieldFactor = difficultyValues.ButcherYieldFactor;
            globalData.ResearchSpeedFactor = difficultyValues.ResearchSpeedFactor;
            globalData.QuestRewardValueFactor = difficultyValues.QuestRewardValueFactor;
            globalData.RaidLootPointsFactor = difficultyValues.RaidLootPointsFactor;
            globalData.TradePriceFactorLoss = difficultyValues.TradePriceFactorLoss;
            globalData.MaintenanceCostFactor = difficultyValues.MaintenanceCostFactor;
            globalData.ScariaRotChance = difficultyValues.ScariaRotChance;
            globalData.EnemyDeathOnDownedChanceFactor = difficultyValues.EnemyDeathOnDownedChanceFactor;
            globalData.ColonistMoodOffset = difficultyValues.ColonistMoodOffset;
            globalData.FoodPoisonChanceFactor = difficultyValues.FoodPoisonChanceFactor;
            globalData.ManhunterChanceOnDamageFactor = difficultyValues.ManhunterChanceOnDamageFactor;
            globalData.PlayerPawnInfectionChanceFactor = difficultyValues.PlayerPawnInfectionChanceFactor;
            globalData.DiseaseIntervalFactor = difficultyValues.DiseaseIntervalFactor;
            globalData.EnemyReproductionRateFactor = difficultyValues.EnemyReproductionRateFactor;
            globalData.DeepDrillInfestationChanceFactor = difficultyValues.DeepDrillInfestationChanceFactor;
            globalData.FriendlyFireChanceFactor = difficultyValues.FriendlyFireChanceFactor;
            globalData.AllowInstantKillChance = difficultyValues.AllowInstantKillChance;
            globalData.PeacefulTemples = difficultyValues.PeacefulTemples;
            globalData.AllowCaveHives = difficultyValues.AllowCaveHives;
            globalData.UnwaveringPrisoners = difficultyValues.UnwaveringPrisoners;
            globalData.AllowTraps = difficultyValues.AllowTraps;
            globalData.AllowTurrets = difficultyValues.AllowTurrets;
            globalData.AllowMortars = difficultyValues.AllowMortars;
            globalData.ClassicMortars = difficultyValues.ClassicMortars;
            globalData.AdaptationEffectFactor = difficultyValues.AdaptationEffectFactor;
            globalData.AdaptationGrowthRateFactorOverZero = difficultyValues.AdaptationGrowthRateFactorOverZero;
            globalData.FixedWealthMode = difficultyValues.FixedWealthMode;
            globalData.LowPopConversionBoost = difficultyValues.LowPopConversionBoost;
            globalData.NoBabiesOrChildren = difficultyValues.NoBabiesOrChildren;
            globalData.BabiesAreHealthy = difficultyValues.BabiesAreHealthy;
            globalData.ChildRaidersAllowed = difficultyValues.ChildRaidersAllowed;
            globalData.ChildAgingRate = difficultyValues.ChildAgingRate;
            globalData.AdultAgingRate = difficultyValues.AdultAgingRate;
            globalData.WastepackInfestationChanceFactor = difficultyValues.WastepackInfestationChanceFactor;

            return globalData;
        }

        private static ServerGlobalData GetServerSettlements(ServerClient client, ServerGlobalData globalData)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == client.username) continue;
                else
                {
                    globalData.settlementTiles.Add(settlement.tile);
                    globalData.settlementOwners.Add(settlement.owner);
                    globalData.settlementGoodwills.Add(GoodwillManager.GetSettlementGoodwill(client, settlement).ToString());
                }
            }

            return globalData;
        }

        private static ServerGlobalData GetServerSites(ServerClient client, ServerGlobalData globalData)
        {
            SiteFile[] sites = SiteManager.GetAllSites();
            foreach (SiteFile site in sites)
            {
                globalData.siteTiles.Add(site.tile);
                globalData.siteOwners.Add(site.owner);
                globalData.siteGoodwills.Add(GoodwillManager.GetSiteGoodwill(client, site).ToString());
                globalData.siteTypes.Add(site.type);
                globalData.isFromFactions.Add(site.isFromFaction);
            }

            return globalData;
        }

        private static ServerGlobalData GetActionsCost(ServerClient client, ServerGlobalData globalData)
        {
            ActionValuesFile actionValues = Master.actionValues;

            globalData.SpyCost = actionValues.SpyCost;

            return globalData;
        }
    }
}
