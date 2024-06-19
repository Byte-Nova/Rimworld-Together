namespace Shared
{
    public class CommonEnumerators
    {
        public enum LogMode { Message, Warning, Error, Title }

        public enum CommandMode { Op, Deop, Broadcast, ForceSave }

        public enum EventStepMode { Send, Receive, Recover }

        public enum MarketStepMode { Add, Request, Reload }

        public enum FactionManifestMode
        {
            Create,
            Delete,
            NameInUse,
            NoPower,
            AddMember,
            RemoveMember,
            AcceptInvite,
            Promote,
            Demote,
            AdminProtection,
            MemberList
        }

        public enum FactionRanks { Member, Moderator, Admin }

        public enum Goodwill { Enemy, Neutral, Ally, Faction, Personal }

        public enum GoodwillTarget { Settlement, Site }

        public enum TransferMode { Gift, Trade, Rebound, Pod, Market }

        public enum TransferLocation { Caravan, Settlement, Pod, World }

        public enum TransferStepMode { TradeRequest, TradeAccept, TradeReject, TradeReRequest, TradeReAccept, TradeReReject, Recover, Pod, Market }

        public enum OfflineVisitStepMode { Request, Deny, Unavailable }

        public enum OfflineRaidStepMode { Request, Deny, Unavailable }

        public enum OfflineSpyStepMode { Request, Deny, Unavailable }

        public enum OnlineActivityStepMode { Request, Accept, Reject, Unavailable, Action, Create, Destroy, Damage, Hediff, TimeSpeed, Stop }

        public enum OnlineActivityTargetFaction { Faction, NonFaction, None }

        public enum OnlineActivityApplyMode { Add, Remove }

        public enum OnlineActivityType { None, Visit, Raid, Misc }

        public enum ActionTargetType { Thing, Human, Animal, Cell, Invalid }

        public enum CreationType { Human, Animal, Thing }

        public enum SiteStepMode { Accept, Build, Destroy, Info, Deposit, Retrieve, Reward, WorkerError }

        public enum SettlementStepMode { Add, Remove }

        public enum SaveMode { Disconnect, Autosave, Strict }

        public enum UserColor { Normal, Admin, Console }

        public enum MessageColor { Normal, Admin, Console }

        public enum LoginMode { Login, Register }

        public enum LoginResponse 
        { 
            InvalidLogin, 
            BannedLogin,
            RegisterInUse, 
            RegisterError, 
            ExtraLogin, 
            WrongMods, 
            ServerFull,
            Whitelist,
            WrongVersion,
            NoWorld
        }

        public enum WorldStepMode { Required, Existing }
    }
}
