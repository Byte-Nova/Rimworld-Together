using System;

namespace Shared
{
    [Serializable]
    public class ServerGlobalData
    {
        public bool isClientAdmin;

        public bool AllowCustomScenarios;

        public bool isClientFactionMember;

        public SiteValuesFile siteValues;

        public MarketValuesFile marketValues;

        public EventFile[] eventValues;

        public ActionValuesFile actionValues;

        public RoadValuesFile roadValues;

        public DifficultyValuesFile difficultyValues;

        public PlanetNPCSettlement[] npcSettlements;

        public OnlineSettlementFile[] playerSettlements;

        public OnlineSiteFile[] playerSites;

        public CaravanFile[] playerCaravans;

        public RoadDetails[] roads;

        public PollutionDetails[] pollutedTiles;
    }
}