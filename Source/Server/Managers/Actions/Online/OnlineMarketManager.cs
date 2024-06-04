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
        private static List<ItemData> currentStock;

        public static void ParseMarketPacket(ServerClient client, Packet packet)
        {
            MarketData marketData = (MarketData)Serializer.ConvertBytesToObject(packet.contents);

            switch (marketData.marketStepMode)
            {
                case MarketStepMode.Add:
                    AddToMarket(client, marketData);
                    break;

                case MarketStepMode.Request:
                    RemoveFromMarket(client, marketData);
                    break;

                case MarketStepMode.Reload:
                    SendMarketStock(client, marketData);
                    break;
            }
        }

        //TODO
        //MAKE SURE WE ONLY CREATE THE STOCK FILE IF THERE WAS NONE PREVIOUSLY

        public static void LoadMarketStock()
        {
            ItemData itemData = new ItemData();
            itemData.defName = "Steel";
            itemData.materialDefName = "null";
            itemData.quantity = 500;
            itemData.quality = "0";
            itemData.hitpoints = 0;

            ItemData itemData2 = new ItemData();
            itemData2.defName = "Steel";
            itemData2.materialDefName = "null";
            itemData2.quantity = 1;
            itemData2.quality = "0";
            itemData2.hitpoints = 0;

            currentStock = new List<ItemData> { itemData, itemData2 };

            SaveMarketStock();
        }

        private static void AddToMarket(ServerClient client, MarketData marketData)
        {
            Logger.Warning("Added!");

            currentStock.Add(marketData.stockToManage);

            SaveMarketStock();
        }

        private static void RemoveFromMarket(ServerClient client, MarketData marketData) 
        {
            Logger.Warning("Removed!");

            marketData.stockToManage = currentStock[marketData.indexToManage];
            currentStock.RemoveAt(marketData.indexToManage);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);

            SaveMarketStock();
        }

        private static void SendMarketStock(ServerClient client, MarketData marketData)
        {
            marketData.currentStock = currentStock.ToArray();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);
        }

        private static void SaveMarketStock()
        {
            string stockPath = Path.Combine(Master.marketPath, "Market.stock");
            Serializer.SerializeToFile(stockPath, currentStock);
        }
    }
}
