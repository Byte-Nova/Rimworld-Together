namespace Shared
{
    public class CommonEnumerators
    {
        public enum ClientNetworkState { Disconnected, Connecting, Connected }

        public enum ServerFileMode { Configs, Actions, Sites, Roads, World, Whitelist, Difficulty, Market, Discord }

        public enum LogMode { Message, Warning, Error, Title, Outsider }

        public enum CommandMode { Op, Deop, Broadcast, ForceSave }

        public enum EventStepMode { Send, Receive, Recover }

        public enum MarketStepMode { Add, Request, Reload }

        public enum AidStepMode { Send, Receive, Accept, Reject }

        public enum CaravanStepMode { Add, Remove, Move }

        public enum RoadStepMode { Add, Remove }

        public enum FactionStepMode { Create, Delete, NameInUse, NoPower, AddMember, RemoveMember, AcceptInvite, Promote, Demote, AdminProtection, MemberList }

        public enum FactionRanks { Member, Moderator, Admin }

        public enum Goodwill { Enemy, Neutral, Ally, Faction, Personal }

        public enum GoodwillTarget { Settlement, Site }

        public enum TransferMode { Gift, Trade, Rebound, Pod, Market }

        public enum TransferLocation { Caravan, Settlement, Pod, World }

        public enum TransferStepMode { TradeRequest, TradeAccept, TradeReject, TradeReRequest, TradeReAccept, TradeReReject, Recover, Pod, Market }

        public enum OfflineActivityStepMode { Request, Deny, Unavailable }

        public enum OnlineActivityStepMode { Request, Accept, Reject, Unavailable, Action, Create, Destroy, Damage, Hediff, Kill, TimeSpeed, GameCondition, Weather, Stop }

        public enum OnlineActivityTargetFaction { Faction, NonFaction, None }

        public enum OnlineActivityApplyMode { Add, Remove }

        public enum OnlineActivityType { None, Visit, Raid }

        public enum OfflineActivityType { None, Visit, Raid, Spy }

        public enum ActionTargetType { Thing, Human, Animal, Cell, Invalid }

        public enum CreationType { Human, Animal, Thing }

        public enum SiteStepMode { Accept, Build, Destroy, Info, Deposit, Retrieve, Reward, WorkerError }

        public enum SettlementStepMode { Add, Remove }

        public enum WorldStepMode { Required, Existing }

        public enum SaveMode { Disconnect, Autosave, Strict }

        public enum UserColor { Normal, Admin, Console, Private, Discord }

        public enum MessageColor { Normal, Admin, Console, Private, Discord }

        public enum LoginMode { Login, Register }

        public enum LoginResponse { InvalidLogin, BannedLogin, RegisterInUse, RegisterError, ExtraLogin, WrongMods, WrongVersion, ServerFull, Whitelist, NoWorld }
    }
}

