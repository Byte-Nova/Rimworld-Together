using System;

namespace Shared
{
    [Serializable]
    public class ServerGlobalData
    {
        public bool _isClientAdmin;

        public bool _isClientFactionMember;

        public SiteValuesFile _siteValues;

        public MarketValuesFile _marketValues;

        public EventFile[] _eventValues;

        public ActionValuesFile _actionValues;

        public RoadValuesFile _roadValues;

        public DifficultyValuesFile _difficultyValues;

        public PlanetNPCSettlement[] _npcSettlements;

        public SettlementFile[] _playerSettlements;

        public SiteFile[] _playerSites;

        public CaravanFile[] _playerCaravans;

        public RoadDetails[] _roads;

        public PollutionDetails[] _pollutedTiles;
    }
}