namespace Shared
{
    public class CommonEnumerators
    {
        public enum ClientNetworkState { Disconnected, Connecting, Connected }

        public enum GenStepMode { Scenario, Storyteller, Difficulty }

        public enum WorldObjectMode { Settlement, Site, Caravan }

        public enum ResponseStepMode { IllegalAction, UserUnavailable, Pop }

        public enum SaveStepMode { Send, Receive, Reset }

        public enum SpyStepMode { Request, Accept, Deny }

        public enum LogMode { Message, Warning, Error, Title, Outsider }

        public enum LogImportanceMode { Normal, Verbose, Extreme }

        public enum CommandMode { Op, Deop, Broadcast, ForceSave }

        public enum EventStepMode { Send, Receive, Recover, Customize, Set }

        public enum AidStepMode { Send, Receive, Accept, Reject }

        public enum CaravanStepMode { Add, Remove, Move }

        public enum RoadStepMode { Add, Remove }

        public enum ModConfigStepMode { Send, Ask }

        public enum GuildStepMode { Create, Delete, NameInUse, NoPower, AddMember, RemoveMember, AcceptInvite, Promote, Demote, AdminProtection, MemberList }

        public enum FactionRanks { Member, Moderator, Admin }

        public enum Goodwill { Enemy, Neutral, Ally, Faction, Personal }

        public enum GoodwillTarget { Settlement, Site }

        public enum ActivityStepMode { Request, Deny }

        public enum ActivityType { None, Raid, Zoom }

        public enum SiteStepMode { Accept, Deny, Build, Destroy, Info, Config, Rewards}

        public enum SettlementStepMode { Add, Remove }

        public enum WorldStepMode { AskFor, Required, Sent }

        public enum SaveMode { Disconnect, Autosave, Strict }

        public enum UserColor { Normal, Admin, Console, Private, Discord, Server }

        public enum MessageColor { Normal, Admin, Console, Private, Discord, Server }

        public enum ModType { Required, Optional, Forbidden };

        public enum LoginResponse { InvalidLogin, BannedLogin, RegisterError, ExtraLogin, WrongMods, WrongVersion, ServerFull, Whitelist, NoWorld }
    }
}

