using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Shared;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class SessionValues
    {
        public static OnlineActivityType currentRealTimeEvent;

        public static OfflineActivityType latestOfflineActivity;

        public static Settlement chosenSettlement;

        public static Caravan chosenCaravan;

        public static Site chosenSite;
        
        public static CompLaunchable chosendPods;

        public static TransferData outgoingManifest = new TransferData();

        public static TransferData incomingManifest = new TransferData();
        
        public static List<Tradeable> listToShowInTradesMenu = new List<Tradeable>();

        public static ActionValuesFile actionValues;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            actionValues = serverGlobalData._actionValues;
        }

        public static void ToggleOnlineFunction(OnlineActivityType type) { currentRealTimeEvent = type; }

        public static void ToggleOfflineFunction(OfflineActivityType type) { latestOfflineActivity = type; }
        
        public static void CleanValues()
        {
            ToggleOnlineFunction(OnlineActivityType.None);
            ToggleOfflineFunction(OfflineActivityType.None);

            chosenSettlement = null;
            chosenCaravan = null;
            chosenSite = null;

            outgoingManifest = new TransferData();
            incomingManifest = new TransferData();
            listToShowInTradesMenu = new List<Tradeable>();
        }
    }
}