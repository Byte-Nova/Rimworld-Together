﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Updater
{
    [Serializable]
    public class DifficultyData
    {
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

        public float EnemyReproductionRateFactor;

        public float DeepDrillInfestationChanceFactor;

        public float FriendlyFireChanceFactor;

        public float AllowInstantKillChance;

        public bool PeacefulTemples;

        public bool AllowCaveHives;

        public bool UnwaveringPrisoners;

        public bool AllowTraps;

        public bool AllowTurrets;

        public bool AllowMortars;

        public bool ClassicMortars;

        public float AdaptationEffectFactor;

        public float AdaptationGrowthRateFactorOverZero;

        public bool FixedWealthMode;

        public float LowPopConversionBoost;

        public bool NoBabiesOrChildren;

        public bool BabiesAreHealthy;

        public bool ChildRaidersAllowed;

        public float ChildAgingRate;

        public float AdultAgingRate;

        public float WastepackInfestationChanceFactor;
    }
}
