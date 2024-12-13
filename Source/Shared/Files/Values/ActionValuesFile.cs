using System;

namespace Shared
{
    [Serializable]
    public class ActionValuesFile
    {
        public bool EnableOnlineActivities = true;

        public bool EnableOfflineActivities = true;

        public bool EnableEvents = true;

        public bool EnableSites = true;

        public bool EnableRoads = true;

        public bool EnableFactions = true;

        public bool EnableAids = true;

        public bool EnableTrading = true;

        public bool EnableCustomScenarios = true;

        public bool EnableNPCDestruction = false;

        public bool EnablePollutionSpread = true;

        public int EnforcedGameSpeed = 0;

        public int SpyCost = 100;

        public int OnlineActivityTickMS = 500;
    }
}
