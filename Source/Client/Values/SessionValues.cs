using GameClient.Patches.Pages;
using GameClient.WorldObjects;
using TCPNetwork.Packets;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Shared.Files;
using System.Collections.Generic;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Values
{
    public static class SessionValues
    {
        public static ClientNetworkState CurrentNetworkState = ClientNetworkState.Disconnected;

        public static ActivityType latestActivity { get; set; } = ActivityType.None;

        public static bool IsActivityHost { get; set; } = false;

        public static bool IsActivityReady { get; set; } = false;

        public static RTSettlement ChosenSettlement { get; set; } = null;

        public static Site ChosenSite { get; set; } = null;

        public static Caravan ChosenCaravan { get; set; } = null;

        public static IEnumerable<IThingHolder> ChosenPods { get; set; } = null;

        public static TransferData OutgoingManifest { get; set; } = new TransferData();

        public static TransferData IncomingManifest { get; set; } = new TransferData();

        public static ActionValuesFile ActionValues { get; set; } = null;

        public static ModConfigFile ConfigFile { get; set; } = null;

        public static ScenarioValuesFile ScenarioFile { get; set; } = null;

        public static StorytellerValuesFile StorytellerFile { get; set; } = null;

        public static DifficultyValuesFile DifficultyFile { get; set; } = null;

        public static WorldValuesFile WorldFile { get; set; } = null;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            ActionValues = serverGlobalData._actionValues;
        }

        public static void ToggleActivity(ActivityType type) { latestActivity = type; }

        public static void CleanValues()
        {
            ToggleActivity(ActivityType.None);

            ChosenSettlement = null;
            ChosenCaravan = null;
            ChosenSite = null;

            OutgoingManifest = new TransferData();
            IncomingManifest = new TransferData();

            Patch_Page_SelectScenario_DoWindowContents.executedMessage = false;
            Patch_DialogOptions_DoModOptions.executedMessage = false;
            Patch_Page_SelectStoryteller_DoWindowContents.executedMessage = false;
        }
    }
}