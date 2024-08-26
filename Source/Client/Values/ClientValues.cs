using Verse;

namespace GameClient
{
    public static class ClientValues
    {
        public static bool isGeneratingFreshWorld;

        public static bool isReadyToPlay;

        public static bool isSavingGame;

        public static bool isQuickConnecting;

        public static bool isSendingSaveToServer;

        public static bool isInTransfer;

        public static bool isRealTimeHost;

        public static string username;

        public static string[] serverBrowserContainer = new string[] { "127.0.0.1|25555" };

        //ModStuff values go below. Do not change manually

        public static bool verboseBool;
        public static bool extremeVerboseBool;
        public static bool muteSoundBool;
        public static bool rejectTransferBool;
        public static bool rejectSiteRewardsBool;

        public static float autosaveDays = 1.0f;
        public static float autosaveCurrentTicks;
        public static float autosaveInternalTicks = autosaveDays * 60000f;

        public static void ForcePermadeath() { Current.Game.Info.permadeathMode = true; }

        public static void ManageDevOptions()
        {
            if (ServerValues.isAdmin) return;
            else Prefs.DevMode = false;
        }

        public static void ToggleGenerateWorld(bool mode) { isGeneratingFreshWorld = mode; }
    
        public static void SetIntentionalDisconnect(bool mode, DisconnectionManager.DCReason reason = DisconnectionManager.DCReason.None) 
        { 
            DisconnectionManager.isIntentionalDisconnect = mode;
            DisconnectionManager.intentionalDisconnectReason = reason; 
        }

        public static void ToggleReadyToPlay(bool mode) { isReadyToPlay = mode; }

        public static void ToggleTransfer(bool mode) { isInTransfer = mode; }

        public static void ToggleChatScroll(bool mode) { ChatManager.shouldScrollChat = mode; }

        public static void ToggleSavingGame(bool mode) { isSavingGame = mode; }

        public static void ToggleQuickConnecting(bool mode) { isQuickConnecting = mode; }

        public static void ToggleSendingSaveToServer(bool mode) { isSendingSaveToServer = mode; }

        public static void ToggleRealTimeHost(bool mode) { isRealTimeHost = mode; }

        public static void CleanValues()
        {
            ToggleGenerateWorld(false);
            SetIntentionalDisconnect(false);
            ToggleReadyToPlay(false);
            ToggleTransfer(false);
            ToggleSavingGame(false);
            ToggleQuickConnecting(false);
            ToggleSendingSaveToServer(false);
            ToggleRealTimeHost(false);
        }
    }
}