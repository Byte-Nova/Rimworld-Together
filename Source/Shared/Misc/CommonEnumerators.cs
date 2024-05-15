﻿namespace Shared
{
    public class CommonEnumerators
    {
        //Logger
        public enum LogMode { Message, Warning, Error, Title }

        public enum FetchMode { Host, Player }

        public enum SearchLocation { Caravan, Settlement }

        //Commands

        public enum CommandType { Op, Deop, Broadcast, ForceSave, SyncSettlements }

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

        //Goodwills

        public enum Goodwills { Enemy, Neutral, Ally, Faction, Personal }

        public enum GoodwillTarget { Settlement, Site }

        //Transfers

        public enum TransferMode { Gift, Trade, Rebound, Pod }

        public enum TransferLocation { Caravan, Settlement, Pod }

        public enum TransferStepMode { TradeRequest, TradeAccept, TradeReject, TradeReRequest, TradeReAccept, TradeReReject, Recover, Pod }

        //Offline visit

        public enum OfflineVisitStepMode { Request, Deny }

        //Raids

        public enum RaidStepMode { Request, Deny }

        //Sites

        public enum SiteStepMode { Accept, Build, Destroy, Info, Deposit, Retrieve, Reward, WorkerError }

        //Spying

        public enum SpyStepMode { Request, Deny }

        //Visits

        public enum VisitStepMode { Request, Accept, Reject, Unavailable, Action, Stop }

        public enum ActionTargetType { Thing, Human, Animal, Cell }

        //Settlements

        public enum SettlementStepMode { Add, Remove }

        //Saving

        public enum SaveMode { Disconnect, Autosave }

        //Chat

        public enum UserColor { Normal, Admin, Console }

        public enum MessageColor { Normal, Admin, Console }

        //Login

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

        //World generation

        public enum WorldStepMode { Required, Existing }
    }
}
