using Newtonsoft.Json;
using Shared;
using Shared.Files.Actions;
using System;
using static System.Collections.Specialized.BitVector32;

namespace TCPNetwork.Files.Client
{
    public class PlayerCooldown
    {
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime EventProtectionTime { get; set; } = DateTime.Now;

        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime AidProtectionTime { get; set; } = DateTime.Now;

        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime PollutionProtectionTime { get; set; } = DateTime.Now;

        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime RoadProtectionTime { get; set; } = DateTime.Now;

        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime NPCProtectionTime { get; set; } = DateTime.Now;

        public void SetEventTimer(UserFile file) 
        { 
            EventProtectionTime = DateTime.Now;
            file.SaveUserFile();
        }

        public void SetAidTimer(UserFile file) 
        { 
            AidProtectionTime = DateTime.Now; 
            file.SaveUserFile();
        }

        public void SetPollutionTimer(UserFile file)
        {
            PollutionProtectionTime = DateTime.Now;
            file.SaveUserFile();
        }

        public void SetRoadTimer(UserFile file)
        {
            RoadProtectionTime = DateTime.Now;
            file.SaveUserFile();
        }

        public void SetNPCTimer(UserFile file)
        {
            NPCProtectionTime = DateTime.Now;
            file.SaveUserFile();
        }

        public static bool CheckIfCanEvent(UserFile file, ACT_Base action)
        {
            if (!action.IsEnabled) return false;
            else if (!TimeConverter.CompareTimes(file.Cooldowns.EventProtectionTime, action.Cooldown)) return false;
            else return true;
        }

        public static bool CheckIfCanAid(UserFile file, ACT_Base action)
        {
            if (!action.IsEnabled) return false;
            else if (!TimeConverter.CompareTimes(file.Cooldowns.AidProtectionTime, action.Cooldown)) return false;
            else return true;
        }

        public static bool CheckIfCanPollute(UserFile file, ACT_Base action)
        {
            if (!action.IsEnabled) return false;
            else if (!TimeConverter.CompareTimes(file.Cooldowns.PollutionProtectionTime, action.Cooldown)) return false;
            else return true;
        }

        public static bool CheckIfCanRoad(UserFile file, ACT_Base action)
        {
            if (!action.IsEnabled) return false;
            else if (!TimeConverter.CompareTimes(file.Cooldowns.RoadProtectionTime, action.Cooldown)) return false;
            else return true;
        }

        public static bool CheckIfCanNPC(UserFile file, ACT_Base action)
        {
            if (!action.IsEnabled) return false;
            else if (!TimeConverter.CompareTimes(file.Cooldowns.NPCProtectionTime, action.Cooldown)) return false;
            else return true;
        }
    }
}
