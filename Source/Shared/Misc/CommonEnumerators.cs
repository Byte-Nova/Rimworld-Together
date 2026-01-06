namespace Shared
{
    public class CommonEnumerators
    {
        public enum ClientNetworkState { Disconnected, Connected }

        public enum GenStepMode { Scenario, Storyteller, Difficulty }

        public enum WorldObjectMode { Settlement, Site, Caravan }

        public enum ResponseStepMode { IllegalAction, UserUnavailable, Pop, NoPower }

        public enum SaveStepMode : byte { Send, Receive, Reset } 

        public enum SpyStepMode { Request, Accept, Deny }

        public enum LogMode { Message, Warning, Error, Title, Outsider }

        public enum LogImportanceMode { Normal, Verbose, Extreme }

        public enum CommandMode { Op, Deop, Broadcast, ForceSave }

        public enum EventStepMode { Send, Receive, Recover, Customize, Set }

        public enum AidStepMode { Send, Receive, Accept, Reject }

        public enum CaravanStepMode { Add, Remove, Move }

        public enum RoadStepMode { Add, Remove }

        public enum ModConfigStepMode { Send, Ask }

        public enum GuildStepMode { Create, Delete, NameInUse, Invite, RemoveMember, AddMember, Promote, Demote, AdminProtection, MemberList }

        public enum Goodwill { Enemy, Neutral, Ally, Guild, Personal }

        public enum GoodwillTarget { Settlement, Site }

        public enum ActivityStepMode { Request, Deny }

        public enum ActivityType { None, Raid, Zoom }

        public enum SiteStepMode { Accept, Build, Destroy, Info, Config, Rewards, Worker }

        public enum SettlementStepMode { Add, Remove }

        public enum WorldStepMode { AskFor, Required, Sent }

        public enum SaveMode { Disconnect, Autosave, Strict }

        public enum ChatColor { Normal, Admin, Console, Private, Discord, Server }

        public enum LoginResponse { Invalid, Ban, Duplicate, Mods, Version, Full, Whitelist, NoWorld }

        public enum TradeMode { None, Sending, Receiving }

        public enum VerboseMode { None, Verbose, Extreme }

        public enum EnforcedSimulatedLag { None, Small, Medium, Big, ENORMOUS }
    }
}

