namespace Shared.Misc
{
    public class CommonEnumerators
    {
        //Commands

        public enum CommandType { Op, Deop, Ban, Disconnect, Quit, Broadcast, ForceSave }

        //Events

        public enum EventStepMode { Send, Receive, Recover }

        //Factions

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

        //Likelihoods

        public enum Likelihoods { Enemy, Neutral, Ally, Faction, Personal }

        public enum LikelihoodTarget { Settlement, Site }

        //Transfers

        public enum TransferMode { Gift, Trade, Rebound, Pod }

        public enum TransferLocation { Caravan, Settlement, Pod }

        public enum TransferStepMode { TradeRequest, TradeAccept, TradeReject, TradeReRequest, TradeReAccept, TradeReReject, Recover, Pod }

        //Offline visit

        public enum OfflineVisitStepMode { Request, Deny }

        //Raids

        public enum RaidStepMode { Request, Deny }

        //Sites

        public enum SiteStepMode { Accept, Build, Destroy, Info, Deposit, Retrieve, Reward }

        //Spying

        public enum SpyStepMode { Request, Deny }

        //Visits

        public enum VisitStepMode { Request, Accept, Reject, Unavailable, Action, Stop }

        public enum ActionTargetType { Thing, Human, Animal, Cell }

        //Settlements

        public enum SettlementStepMode { Add, Remove }

        //Saving

        public enum SaveStepMode { Disconnect, Quit, Autosave, Transfer, Event }

        public enum SaveMode { Disconnect, Quit, Autosave, Transfer, Event }

        //Chat

        public enum UserColor { Normal, Admin, Console }

        public enum MessageColor { Normal, Admin, Console }

        //Login

        public enum LoginMode { Login, Register }

        public enum LoginResponse 
        { 
            InvalidLogin, 
            BannedLogin, 
            RegisterSuccess, 
            RegisterInUse, 
            RegisterError, 
            ExtraLogin, 
            WrongMods, 
            ServerFull,
            Whitelist
        }

        //World generation

        public enum WorldStepMode { Required, Existing, Saved }
    }
}
