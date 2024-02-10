using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Managers
{
    public static class ServerOverallManager
    {
        public static void SendServerOveralls(ServerClient client)
        {
            ServerOverallJSON so = new ServerOverallJSON();

            so = GetServerValues(so);

            so = GetClientValues(client, so);

            so = GetEventCosts(so);

            so = GetSiteDetails(so);

            so = GetServerDifficulty(so);

            so = GetServerSettlements(client, so);

            so = GetServerSites(client, so);

            so = GetActionsCost(client, so);

            Packet packet = Packet.CreatePacketFromJSON("ServerValuesPacket", so);
            client.clientListener.SendData(packet);
        }

        private static ServerOverallJSON GetServerValues(ServerOverallJSON so)
        {
            ServerValuesFile svf = Program.serverValues;
            so.AllowCustomScenarios = svf.AllowCustomScenarios;

            return so;
        }

        private static ServerOverallJSON GetClientValues(ServerClient client, ServerOverallJSON so)
        {
            so.isClientAdmin = client.isAdmin;

            so.isClientFactionMember = client.hasFaction;

            return so;
        }

        private static ServerOverallJSON GetEventCosts(ServerOverallJSON so)
        {
            EventValuesFile ev = Program.eventValues;
            so.RaidCost = ev.RaidCost;
            so.InfestationCost = ev.InfestationCost;
            so.MechClusterCost = ev.MechClusterCost;
            so.ToxicFalloutCost = ev.ToxicFalloutCost;
            so.ManhunterCost = ev.ManhunterCost;
            so.WandererCost = ev.WandererCost;
            so.FarmAnimalsCost = ev.FarmAnimalsCost;
            so.ShipChunkCost = ev.ShipChunkCost;
            so.TraderCaravanCost = ev.TraderCaravanCost;

            return so;
        }

        private static ServerOverallJSON GetSiteDetails(ServerOverallJSON so)
        {
            SiteValuesFile sv = Program.siteValues;
            so.PersonalFarmlandCost = sv.PersonalFarmlandCost;
            so.FactionFarmlandCost = sv.FactionFarmlandCost;
            so.FarmlandRewardCount = sv.FarmlandRewardCount;

            so.PersonalQuarryCost = sv.PersonalQuarryCost;
            so.FactionQuarryCost = sv.FactionQuarryCost;
            so.QuarryRewardCount = sv.QuarryRewardCount;

            so.PersonalSawmillCost = sv.PersonalSawmillCost;
            so.FactionSawmillCost = sv.FactionSawmillCost;
            so.SawmillRewardCount = sv.SawmillRewardCount;

            so.PersonalBankCost = sv.PersonalBankCost;
            so.FactionBankCost = sv.FactionBankCost;
            so.BankRewardCount = sv.BankRewardCount;

            so.PersonalLaboratoryCost = sv.PersonalLaboratoryCost;
            so.FactionLaboratoryCost = sv.FactionLaboratoryCost;
            so.LaboratoryRewardCount = sv.LaboratoryRewardCount;

            so.PersonalRefineryCost = sv.PersonalRefineryCost;
            so.FactionRefineryCost = sv.FactionRefineryCost;
            so.RefineryRewardCount = sv.RefineryRewardCount;

            so.PersonalHerbalWorkshopCost = sv.PersonalHerbalWorkshopCost;
            so.FactionHerbalWorkshopCost = sv.FactionHerbalWorkshopCost;
            so.HerbalWorkshopRewardCount = sv.HerbalWorkshopRewardCount;

            so.PersonalTextileFactoryCost = sv.PersonalTextileFactoryCost;
            so.FactionTextileFactoryCost = sv.FactionTextileFactoryCost;
            so.TextileFactoryRewardCount = sv.TextileFactoryRewardCount;

            so.PersonalFoodProcessorCost = sv.PersonalFoodProcessorCost;
            so.FactionFoodProcessorCost = sv.FactionFoodProcessorCost;
            so.FoodProcessorRewardCount = sv.FoodProcessorRewardCount;

            return so;
        }

        private static ServerOverallJSON GetServerDifficulty(ServerOverallJSON so)
        {
            DifficultyValuesFile dv = Program.difficultyValues;
            so.UsingCustomDifficulty = dv.UseCustomDifficulty;
            so.ThreatScale = dv.ThreatScale;
            so.ThreatScale = dv.ThreatScale;
            so.AllowBigThreats = dv.AllowBigThreats;
            so.AllowViolentQuests = dv.AllowViolentQuests;
            so.AllowIntroThreats = dv.AllowIntroThreats;
            so.PredatorsHuntHumanlikes = dv.PredatorsHuntHumanlikes;
            so.AllowExtremeWeatherIncidents = dv.AllowExtremeWeatherIncidents;
            so.CropYieldFactor = dv.CropYieldFactor;
            so.MineYieldFactor = dv.MineYieldFactor;
            so.ButcherYieldFactor = dv.ButcherYieldFactor;
            so.ResearchSpeedFactor = dv.ResearchSpeedFactor;
            so.QuestRewardValueFactor = dv.QuestRewardValueFactor;
            so.RaidLootPointsFactor = dv.RaidLootPointsFactor;
            so.TradePriceFactorLoss = dv.TradePriceFactorLoss;
            so.MaintenanceCostFactor = dv.MaintenanceCostFactor;
            so.ScariaRotChance = dv.ScariaRotChance;
            so.EnemyDeathOnDownedChanceFactor = dv.EnemyDeathOnDownedChanceFactor;
            so.ColonistMoodOffset = dv.ColonistMoodOffset;
            so.FoodPoisonChanceFactor = dv.FoodPoisonChanceFactor;
            so.ManhunterChanceOnDamageFactor = dv.ManhunterChanceOnDamageFactor;
            so.PlayerPawnInfectionChanceFactor = dv.PlayerPawnInfectionChanceFactor;
            so.DiseaseIntervalFactor = dv.DiseaseIntervalFactor;
            so.DeepDrillInfestationChanceFactor = dv.DeepDrillInfestationChanceFactor;
            so.FriendlyFireChanceFactor = dv.FriendlyFireChanceFactor;
            so.AllowInstantKillChance = dv.AllowInstantKillChance;
            so.AllowTraps = dv.AllowTraps;
            so.AllowTurrets = dv.AllowTurrets;
            so.AllowMortars = dv.AllowMortars;
            so.AdaptationEffectFactor = dv.AdaptationEffectFactor;
            so.AdaptationGrowthRateFactorOverZero = dv.AdaptationGrowthRateFactorOverZero;
            so.FixedWealthMode = dv.FixedWealthMode;
            so.LowPopConversionBoost = dv.LowPopConversionBoost;
            so.NoBabiesOrChildren = dv.NoBabiesOrChildren;
            so.BabiesAreHealthy = dv.BabiesAreHealthy;
            so.ChildRaidersAllowed = dv.ChildRaidersAllowed;
            so.ChildAgingRate = dv.ChildAgingRate;
            so.AdultAgingRate = dv.AdultAgingRate;
            so.WastepackInfestationChanceFactor = dv.WastepackInfestationChanceFactor;

            return so;
        }

        private static ServerOverallJSON GetServerSettlements(ServerClient client, ServerOverallJSON so)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == client.username) continue;
                else
                {
                    so.settlementTiles.Add(settlement.tile);
                    so.settlementOwners.Add(settlement.owner);
                    so.settlementLikelihoods.Add(LikelihoodManager.GetSettlementLikelihood(client, settlement).ToString());
                }
            }

            return so;
        }

        private static ServerOverallJSON GetServerSites(ServerClient client, ServerOverallJSON so)
        {
            SiteFile[] sites = SiteManager.GetAllSites();
            foreach (SiteFile site in sites)
            {
                so.siteTiles.Add(site.tile);
                so.siteOwners.Add(site.owner);
                so.siteLikelihoods.Add(LikelihoodManager.GetSiteLikelihood(client, site).ToString());
                so.siteTypes.Add(site.type);
                so.isFromFactions.Add(site.isFromFaction);
            }

            return so;
        }

        private static ServerOverallJSON GetActionsCost(ServerClient client, ServerOverallJSON so)
        {
            ActionValuesFile av = Program.actionValues;

            so.SpyCost = av.SpyCost;

            return so;
        }
    }
}
