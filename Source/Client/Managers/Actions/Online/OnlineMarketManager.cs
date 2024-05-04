using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class OnlineMarketManager
    {
        public static void ParseMarketPacket(Packet packet)
        {
            MarketData marketData = (MarketData)Serializer.ConvertBytesToObject(packet.contents);

            switch (marketData.marketStepMode)
            {
                case (int)MarketStepMode.Reload:
                    ReloadMarketStock(marketData);
                    break;
            }
        }

        public static void RequestMarketStock(MarketType marketType)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for server response"));

            MarketData marketData = new MarketData();
            marketData.marketStepMode = (int)MarketStepMode.Reload;
            marketData.marketType = (int)marketType;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void ReloadMarketStock(MarketData marketData)
        {
            DialogManager.PopWaitDialog();

            if (marketData.marketType == (int)MarketType.Global)
            {
                RT_Dialog_MarketListing dialog = new RT_Dialog_MarketListing(MarketType.Global, 
                    marketData.currentStock, null, null);

                DialogManager.PushNewDialog(dialog);
            }

            else
            {
                RT_Dialog_MarketListing dialog = new RT_Dialog_MarketListing(MarketType.Faction, 
                    marketData.currentStock, null, null);

                DialogManager.PushNewDialog(dialog);
            }
        }
    }
}
