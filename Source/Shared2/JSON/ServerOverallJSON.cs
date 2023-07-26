using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class ServerOverallJSON
    {
        public bool AllowCustomScenarios;

        public bool isClientAdmin;

        public bool isClientFactionMember;

        public string RaidCost;
        public string InfestationCost;
        public string MechClusterCost;
        public string ToxicFalloutCost;
        public string ManhunterCost;
        public string WandererCost;
        public string FarmAnimalsCost;
        public string ShipChunkCost;
        public string TraderCaravanCost;

        public bool UsingCustomDifficulty;
        public float ThreatScale;
        public bool AllowBigThreats;
        public bool AllowViolentQuests;
        public bool AllowIntroThreats;
        public bool PredatorsHuntHumanlikes;
        public bool AllowExtremeWeatherIncidents;
        public float CropYieldFactor;
        public float MineYieldFactor;
        public float ButcherYieldFactor;
        public float ResearchSpeedFactor;
        public float QuestRewardValueFactor;
        public float RaidLootPointsFactor;
        public float TradePriceFactorLoss;
        public float MaintenanceCostFactor;
        public float ScariaRotChance;
        public float EnemyDeathOnDownedChanceFactor;
        public float ColonistMoodOffset;
        public float FoodPoisonChanceFactor;
        public float ManhunterChanceOnDamageFactor;
        public float PlayerPawnInfectionChanceFactor;
        public float DiseaseIntervalFactor;
        public float DeepDrillInfestationChanceFactor;
        public float FriendlyFireChanceFactor;
        public float AllowInstantKillChance;
        public bool AllowTraps;
        public bool AllowTurrets;
        public bool AllowMortars;
        public float AdaptationEffectFactor;
        public float AdaptationGrowthRateFactorOverZero;
        public bool FixedWealthMode;
        public float LowPopConversionBoost;
        public bool NoBabiesOrChildren;
        public bool babiesAreHealthy;
        public bool ChildRaidersAllowed;
        public float ChildAgingRate;
        public float AdultAgingRate;
        public float WastepackInfestationChanceFactor;

        public List<string> settlementTiles = new List<string>();
        public List<string> settlementOwners = new List<string>();
        public List<string> settlementLikelihoods = new List<string>();

        public List<string> siteTiles = new List<string>();
        public List<string> siteOwners = new List<string>();
        public List<string> siteLikelihoods = new List<string>();
        public List<string> siteTypes = new List<string>();
        public List<bool> isFromFactions = new List<bool>();

        public string PersonalFarmlandCost;
        public string PersonalQuarryCost;
        public string PersonalSawmillCost;
        public string PersonalBankCost;
        public string PersonalLaboratoryCost;
        public string PersonalRefineryCost;
        public string PersonalHerbalWorkshopCost;
        public string PersonalTextileFactoryCost;
        public string PersonalFoodProcessorCost;

        public string FactionFarmlandCost;
        public string FactionQuarryCost;
        public string FactionSawmillCost;
        public string FactionBankCost;
        public string FactionLaboratoryCost;
        public string FactionRefineryCost;
        public string FactionHerbalWorkshopCost;
        public string FactionTextileFactoryCost;
        public string FactionFoodProcessorCost;

        public string FarmlandRewardCount;
        public string QuarryRewardCount;
        public string SawmillRewardCount;
        public string BankRewardCount;
        public string LaboratoryRewardCount;
        public string RefineryRewardCount;
        public string HerbalWorkshopRewardCount;
        public string TextileFactoryRewardCount;
        public string FoodProcessorRewardCount;

        public string SpyCost;
    }
}