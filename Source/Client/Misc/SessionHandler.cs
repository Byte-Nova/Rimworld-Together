using GameClient.Defs;
using GameClient.Managers;
using GameClient.Patches.Pages;
using GameClient.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Shared.Files.Actions;
using Shared.Files.Configs;
using Shared.Files.Configs.Mods;
using System.Collections.Generic;
using System.Linq;
using TCPNetwork.Packets;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Misc
{
    public static class SessionHandler
    {
        public static string Username { get; set; } = string.Empty;

        public static ClientNetworkState CurrentNetworkState = ClientNetworkState.Disconnected;

        public static ActivityType latestActivity { get; set; } = ActivityType.None;

        public static RTSettlement ChosenSettlement { get; set; } = null;

        public static RTSite ChosenSite { get; set; } = null;

        public static Caravan ChosenCaravan { get; set; } = null;

        public static IEnumerable<IThingHolder> ChosenPods { get; set; } = null;

        public static TransferData OutgoingManifest { get; set; } = new TransferData();

        public static TransferData IncomingManifest { get; set; } = new TransferData();

        public static ActionsConfigFile CurrentActionValues { get; set; } = null;

        public static ModsConfigFile CurrentModConfig { get; set; } = null;

        public static ScenarioConfigFile CurrentScenario { get; set; } = null;

        public static StorytellerConfigFile CurrentStoryteller { get; set; } = null;

        public static DifficultyConfigFile CurrentDifficulty { get; set; } = null;

        public static PlanetConfigFile CurrentWorld { get; set; } = null;

        public static bool IsAdmin { get; set; } = false;

        public static bool HasFaction { get; set; } = false;

        public static bool IsGeneratingFreshWorld { get; set; } = false;

        public static bool IsReadyToPlay { get; set; } = false;

        public static bool IsSavingGame { get; set; } = false;

        public static bool IsInTransfer { get; set; } = false;

        public static bool IsUsingScriber { get; set; } = false;

        public static List<Faction> PlayerFactions { get; set; } = new List<Faction>();

        public static List<FactionDef> PlayerFactionDefs { get; set; } = new List<FactionDef>();

        public static Faction EnemyFaction { get; set; } = null;

        public static Faction AllyFaction { get; set; } = null;

        public static Faction NeutralFaction { get; set; } = null;

        public static Faction GuildFaction { get; set; } = null;

        public static TradeMode LastTradeStep { get; set; } = CommonEnumerators.TradeMode.None;

        public static bool IsExiting { get; set; } = false;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            IsAdmin = serverGlobalData._isClientAdmin;
            HasFaction = serverGlobalData._isClientFactionMember;
            CurrentActionValues = serverGlobalData._actionValues;
        }

        [OnUpdate]
        private static void ForcePermadeath()
        {
            try { Current.Game.Info.permadeathMode = true; }
            catch { }
        }

        [OnUpdate]
        private static void ManageDevOptions()
        {
            try { if (!IsAdmin) Prefs.DevMode = false; }
            catch { }
        }

        [OnSessionEnd]
        private static void CleanValues()
        {
            latestActivity = ActivityType.None;

            ChosenSettlement = null;
            ChosenCaravan = null;
            ChosenSite = null;

            OutgoingManifest = new TransferData();
            IncomingManifest = new TransferData();

            IsGeneratingFreshWorld = false;
            IsReadyToPlay = false;
            IsInTransfer = false;
            IsSavingGame = false;
            IsUsingScriber = false;
            LastTradeStep = TradeMode.None;
            ChatManager.ShouldScrollChat = true;
            IsExiting = false;

            Patch_Page_SelectScenario_DoWindowContents.executedMessage = false;
            Patch_DialogOptions_DoModOptions.executedMessage = false;
            Patch_Page_SelectStoryteller_DoWindowContents.executedMessage = false;
        }
    }
}