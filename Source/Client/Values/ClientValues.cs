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

        public static bool isReadyToPlay;

        public static bool isSavingGame;

        public static bool isQuickConnecting;

        public static bool isSendingSaveToServer;

        public static bool isInTransfer;

        public static bool isInVisit;

        public static Settlement chosenSettlement;
        public static Caravan chosenCaravan;
        public static Site chosenSite;
        public static CompLaunchable chosendPods;

        public static TransferData outgoingManifest = new TransferData();
        public static TransferData incomingManifest = new TransferData();
        public static List<Tradeable> listToShowInTradesMenu = new List<Tradeable>();

        public static string username;

        public static string[] serverBrowserContainer = new string[] { "127.0.0.1|25555" };

        //ModStuff values go below. Do not change manually

        public static bool verboseBool;
        public static bool muteSoundBool;
        public static bool rejectTransferBool;
        public static bool rejectSiteRewardsBool;
        public static bool saveMessageBool;

        public static float autosaveDays = 1.0f;
        public static float autosaveCurrentTicks;
        public static float autosaveInternalTicks = autosaveDays * 60000f;

        public static void ForcePermadeath() { Current.Game.Info.permadeathMode = true; }

        public static void ManageDevOptions()
        {
            if (ServerValues.isAdmin) return;
            else Prefs.DevMode = false;
        }

        public static void ToggleGenerateWorld(bool mode) { needsToGenerateWorld = mode; }
    
        public static void SetIntentionalDisconnect(bool mode, DisconnectionManager.DCReason reason = DisconnectionManager.DCReason.None) 
        { 
            DisconnectionManager.isIntentionalDisconnect = mode;
            DisconnectionManager.intentionalDisconnectReason = reason; 
        }

        public static void ToggleReadyToPlay(bool mode) 
        { 
            isReadyToPlay = mode;

            ReadyToPlayData readyToPlayData = new ReadyToPlayData();
            readyToPlayData.ReadyToPlay = mode;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ReadyToPlayPacket), readyToPlayData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void ToggleTransfer(bool mode) { isInTransfer = mode; }

        public static void ToggleVisit(bool mode) { isInVisit = mode; }

        public static void ToggleChatScroll(bool mode) { OnlineChatManager.shouldScrollChat = mode; }

        public static void ToggleSavingGame(bool mode) { isSavingGame = mode; }

        public static void ToggleQuickConnecting(bool mode) { isQuickConnecting = mode; }

        public static void ToggleSendingSaveToServer(bool mode) { isSendingSaveToServer = mode; }

        public static void CleanValues()
        {
            ToggleGenerateWorld(false);
            SetIntentionalDisconnect(false);
            ToggleReadyToPlay(false);
            ToggleTransfer(false);
            ToggleVisit(false);
            ToggleSavingGame(false);
            ToggleQuickConnecting(false);
            ToggleSendingSaveToServer(false);

            chosenSettlement = null;
            chosenCaravan = null;
            chosenSite = null;

            outgoingManifest = new TransferData();
            incomingManifest = new TransferData();
            listToShowInTradesMenu = new List<Tradeable>();
        }
    }
}