using GameServer.Core;
using GameServer.Files;
using TCPNetwork.Server;
using Shared;

namespace GameServer.Misc
{
    public static class ValueChecker
    {
        public static bool CheckIfCanEvent(UserFile file)
        {
            if (!Master.ServerConfig.TemporalEventProtection) return true;
            else if (!TimeConverter.CheckForEpochTimer(file.EventProtectionTime, Master.ServerConfig.TemporalEventProtectionTime * 1000)) return false;
            else return true;
        }

        public static bool CheckIfCanAid(UserFile file)
        {
            if (!Master.ServerConfig.TemporalAidProtection) return true;
            else if (!TimeConverter.CheckForEpochTimer(file.AidProtectionTime, Master.ServerConfig.TemporalAidProtectionTime * 1000)) return false;
            else return true;
        }
    }
}
