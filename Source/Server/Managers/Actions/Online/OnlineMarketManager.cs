using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OnlineMarketManager
    {
        public static void ParseMarketPacket(ServerClient client, Packet packet)
        {
            MarketData marketData = (MarketData)Serializer.ConvertBytesToObject(packet.contents);

            switch (marketData.marketStepMode)
            {
                case MarketStepMode.Add:
                    AddToMarket(client, marketData);
                    break;

                case MarketStepMode.Request:
                    RequestFromMarket(client, marketData);
                    break;

                case MarketStepMode.Reload:
                    SendMarketStock(client, marketData);
                    break;
            }
        }

        private static void AddToMarket(ServerClient client, MarketData marketData)
        {
            AddStockFile(client, marketData);
        }

        private static void RequestFromMarket(ServerClient client, MarketData marketData) 
        {
            RemoveStockFile(client, marketData);
        }

        private static void SendMarketStock(ServerClient client, MarketData marketData)
        {
            ItemData itemData = new ItemData();
            itemData.defName = "Steel";
            itemData.materialDefName = "null";
            itemData.quantity = 5;
            itemData.quality = "0";
            itemData.hitpoints = 0;

            marketData.currentStock = new ItemData[] { itemData };

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);
        }

        private static void AddStockFile(ServerClient client, MarketData marketData)
        {
            string stockPath = Path.Combine(Master.globalMarketsPath, "test.stock");
            Serializer.SerializeToFile(stockPath, marketData.stockToManage);
        }

        private static void RemoveStockFile(ServerClient client, MarketData marketData)
        {
            string stockPath = Path.Combine(Master.globalMarketsPath, "test.stock");
            File.Delete(stockPath);
        }
    }
}
