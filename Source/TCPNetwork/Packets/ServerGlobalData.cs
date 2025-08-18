using Shared;
using Shared.Files;

namespace TCPNetwork.Packets
{

    public class ServerGlobalData
    {
        public bool _isClientAdmin { get; set; } = false;

        public bool _isClientFactionMember { get; set; } = false;

        public ServerValuesFile _serverValues { get; set; } = null;

        public SiteValuesFile _siteValues { get; set; } = null;

        public EventFile[] _eventValues { get; set; } = null;

        public ActionValuesFile _actionValues { get; set; } = null;

        public RoadValuesFile _roadValues { get; set; } = null;

        public ScenarioValuesFile _scenarioValues { get; set; } = null;

        public StorytellerValuesFile _storytellerValues { get; set; } = null;

        public DifficultyValuesFile _difficultyValues { get; set; } = null;

        public PlanetNPCSettlementDetails[] _npcSettlements { get; set; } = null;

        public SettlementFile[] _playerSettlements { get; set; } = null;

        public SiteFile[] _playerSites { get; set; } = null;

        public RoadDetails[] _roads { get; set; } = null;

        public PollutionDetails[] _pollutedTiles { get; set; } = null;

        public ModConfigFile _modConfigs { get; set; } = null;

        public override string ToString()
        {
            return $"ServerGlobalData:|{_isClientAdmin}|{_isClientFactionMember}|{_serverValues}|{_siteValues}|{_eventValues}" +
                $"|{_actionValues}|{_roadValues}|{_scenarioValues}|{_storytellerValues}|{_difficultyValues}|{_npcSettlements?.Length ?? 0}|{_playerSettlements?.Length ?? 0}" +
                $"|{_playerSites?.Length ?? 0}|{_roads?.Length ?? 0}|{_pollutedTiles?.Length ?? 0}|{_modConfigs}";
        }
    }
}