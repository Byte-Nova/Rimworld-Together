using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;

namespace GameClient
{
    public static class ClientValues
    {
        public static bool needsToGenerateWorld;

        public static bool isSaving;

        public static bool isDisconnecting;

        public static bool isQuiting;

        public static bool isReadyToPlay;

        public static bool requireSaveManipulation;

        //Do not change manually
        public static bool autoDenyTransfers;

        //Do not change manually
        public static bool autoRejectSiteRewards;

        //Do not change manually
        public static bool verboseBool;

        public static bool isInTransfer;

        public static bool isInVisit;

        public static Settlement chosenSettlement;
        public static Caravan chosenCaravan;
        public static Site chosenSite;
        public static CompLaunchable chosendPods;

        public static TransferManifestJSON outgoingManifest = new TransferManifestJSON();
        public static TransferManifestJSON incomingManifest = new TransferManifestJSON();
        public static List<Tradeable> listToShowInTradesMenu = new List<Tradeable>();

        public static int autosaveDays = 1;
        public static float autosaveCurrentTicks;
        public static float autosaveInternalTicks = autosaveDays * 60000f;

        public static string[] serverBrowserContainer = new string[] { "127.0.0.1|25555" };

        public static void ForcePermadeath() { Current.Game.Info.permadeathMode = true; }

        public static void ManageDevOptions()
        {
            if (ServerValues.isAdmin) return;
            else Prefs.DevMode = false;
        }

        public static void ToggleGenerateWorld(bool mode) { needsToGenerateWorld = mode; }

        public static void ToggleSaving(bool mode) { isSaving = mode; }

        public static void ToggleDisconnecting(bool mode) { isDisconnecting = mode; }

        public static void ToggleQuiting(bool mode) { isQuiting = mode; }

        public static void ToggleReadyToPlay(bool mode) { isReadyToPlay = mode; }

        public static void ToggleTransfer(bool mode) { isInTransfer = mode; }

        public static void ToggleVisit(bool mode) { isInVisit = mode; }

        public static void ToggleChatScroll(bool mode) { ChatManager.shouldScrollChat = mode; }

        public static void ToggleRequireSaveManipulation(bool mode) { requireSaveManipulation = mode; }

        public static void CleanValues()
        {
            ToggleGenerateWorld(false);
            ToggleSaving(false);
            ToggleDisconnecting(false);
            ToggleQuiting(false);
            ToggleReadyToPlay(false);
            ToggleTransfer(false);
            ToggleVisit(false);
            ToggleRequireSaveManipulation(false);

            chosenSettlement = null;
            chosenCaravan = null;
            chosenSite = null;

            outgoingManifest = new TransferManifestJSON();
            incomingManifest = new TransferManifestJSON();
            listToShowInTradesMenu = new List<Tradeable>();
        }
    }
}