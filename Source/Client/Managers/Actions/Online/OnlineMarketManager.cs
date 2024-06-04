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
                case MarketStepMode.Add:
                    break;

                case MarketStepMode.Request:
                    break;

                case MarketStepMode.Reload:
                    ReloadMarketStock(marketData);
                    break;
            }
        }

        public static void RequestMarketReload(MarketType marketType)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for server response"));

            MarketData marketData = new MarketData();
            marketData.marketStepMode = MarketStepMode.Reload;
            marketData.marketType = marketType;

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
