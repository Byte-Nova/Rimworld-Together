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
        private static readonly string marketFileName = "Market.json";

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

        public static void LoadMarketStock()
        {
            string marketFilePath = Path.Combine(Master.corePath, marketFileName);
            if (File.Exists(marketFilePath)) Master.marketFile = Serializer.SerializeFromFile<MarketFile>(marketFilePath);
            else
            {
                Master.marketFile = new MarketFile();
                SaveMarketStock();
            }

            Logger.WriteToConsole("Loaded market stock", LogMode.Warning);
        }

        private static void AddToMarket(ServerClient client, MarketData marketData)
        {
            List<ItemData> itemsToAdd = new List<ItemData>();
            foreach (byte[] bytes in marketData.transferThingBytes) itemsToAdd.Add((ItemData)Serializer.ConvertBytesToObject(bytes));
            foreach (ItemData item in itemsToAdd) TryCombineStackIfAvailable(item);

            SaveMarketStock();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);

            marketData.marketStepMode = MarketStepMode.Reload;
            marketData.currentStockBytes = ItemsToBytes(Master.marketFile.MarketStock);
            packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            foreach (ServerClient sc in Network.connectedClients.ToArray())
            {
                if (sc == client) continue;
                else sc.listener.EnqueuePacket(packet);
            }
        }

        private static void RemoveFromMarket(ServerClient client, MarketData marketData) 
        {
            marketData.transferThingBytes = new List<byte[]>() { Serializer.ConvertObjectToBytes(Master.marketFile.MarketStock[marketData.indexToManage]) };
            Master.marketFile.MarketStock.RemoveAt(marketData.indexToManage);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);

            marketData.marketStepMode = MarketStepMode.Reload;
            marketData.currentStockBytes = ItemsToBytes(Master.marketFile.MarketStock);
            packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            foreach (ServerClient sc in Network.connectedClients.ToArray())
            {
                if (sc == client) continue;
                else sc.listener.EnqueuePacket(packet);
            }

            SaveMarketStock();
        }

        private static void SendMarketStock(ServerClient client, MarketData marketData)
        {
            marketData.currentStockBytes = ItemsToBytes(Master.marketFile.MarketStock);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);
        }

        private static void SaveMarketStock()
        {
            string stockPath = Path.Combine(Master.corePath, marketFileName);
            Serializer.SerializeToFile(stockPath, Master.marketFile);
        }

        private static List<byte[]> ItemsToBytes(List<ItemData> itemData)
        {
            List<byte[]> itemBytes = new List<byte[]>();
            foreach(ItemData data in itemData) itemBytes.Add(Serializer.ConvertObjectToBytes(data));
            return itemBytes;
        }

        private static void TryCombineStackIfAvailable(ItemData itemData)
        {
            foreach (ItemData stockedItem in Master.marketFile.MarketStock.ToArray())
            {
                if (stockedItem.defName == itemData.defName && stockedItem.materialDefName == itemData.materialDefName)
                {
                    stockedItem.quantity += itemData.quantity;
                    return;
                }
            }

            Master.marketFile.MarketStock.Add(itemData);
        }
    }
}
