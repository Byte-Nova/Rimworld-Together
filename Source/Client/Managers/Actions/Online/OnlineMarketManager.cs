using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public static class OnlineMarketManager
    {
        public static void ParseMarketPacket(Packet packet)
        {
            MarketData marketData = (MarketData)Serializer.ConvertBytesToObject(packet.contents);

            switch (marketData.marketStepMode)
            {
                default:
                    break;
            }
        }

        public static void OnGlobalMarketOpen()
        {
            string[] elements = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            RT_Dialog_MarketListing dialog = new RT_Dialog_MarketListing("Global Market", "Trade with the rest of the world remotely", elements, null, null);
            DialogManager.PushNewDialog(dialog);
        }

        public static void OnFactionMarketOpen()
        {
            string[] elements = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            RT_Dialog_MarketListing dialog = new RT_Dialog_MarketListing("Faction Market", "Trade with your faction members remotely", elements, null, null);
            DialogManager.PushNewDialog(dialog);
        }
    }
}
