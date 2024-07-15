using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    //Class that is in charge of the online market functions for the mod

    public static class MarketManager
    {
        //Variables

        private static readonly string marketFileName = "Market.json";

        //Parses received market packets into something usable

        public static void ParseMarketPacket(ServerClient client, Packet packet)
        {
            MarketData marketData = Serializer.ConvertBytesToObject<MarketData>(packet.contents);

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
            string path = Path.Combine(Master.corePath, marketFileName);
            if (File.Exists(path)) Master.marketFile = Serializer.SerializeFromFile<MarketFile>(path);
            else
            {
                Master.marketFile = new MarketFile();
                SaveMarketStock();
            }

            Logger.Warning($"Loaded > '{path}'");
        }

        private static void AddToMarket(ServerClient client, MarketData marketData)
        {
            foreach (ThingData item in marketData.transferThings) TryCombineStackIfAvailable(client, item);

            SaveMarketStock();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);

            marketData.marketStepMode = MarketStepMode.Reload;
            marketData.transferThings = Master.marketFile.MarketStock;
            packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            foreach (ServerClient sc in Network.connectedClients.ToArray())
            {
                if (sc == client) continue;
                else sc.listener.EnqueuePacket(packet);
            }
        }

        private static void RemoveFromMarket(ServerClient client, MarketData marketData) 
        {
            ThingData toGet = Master.marketFile.MarketStock[marketData.indexToManage];
            int reservedQuantity = toGet.quantity;
            toGet.quantity = marketData.quantityToManage;
            marketData.transferThings = new List<ThingData>() { toGet };

            ThingData thingData = Master.marketFile.MarketStock[marketData.indexToManage];
            thingData.quantity = reservedQuantity;

            if (marketData.quantityToManage == 0) ResponseShortcutManager.SendIllegalPacket(client, "Tried to buy illegal quantity at market");
            else if (thingData.quantity > marketData.quantityToManage) thingData.quantity -= marketData.quantityToManage;
            else if (thingData.quantity == marketData.quantityToManage) Master.marketFile.MarketStock.RemoveAt(marketData.indexToManage);
            else ResponseShortcutManager.SendIllegalPacket(client, "Tried to buy illegal quantity at market");

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);

            marketData.marketStepMode = MarketStepMode.Reload;
            marketData.transferThings = Master.marketFile.MarketStock;
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
            marketData.transferThings = Master.marketFile.MarketStock;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);
        }

        private static void SaveMarketStock()
        {
            string stockPath = Path.Combine(Master.corePath, marketFileName);
            Serializer.SerializeToFile(stockPath, Master.marketFile);
        }

        private static void TryCombineStackIfAvailable(ServerClient client, ThingData thingData)
        {
            if (thingData.quantity <= 0)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to sell illegal quantity at market");
                return;
            }

            foreach (ThingData stockedItem in Master.marketFile.MarketStock.ToArray())
            {
                if (stockedItem.defName == thingData.defName && stockedItem.materialDefName == thingData.materialDefName)
                {
                    stockedItem.quantity += thingData.quantity;
                    return;
                }
            }

            Master.marketFile.MarketStock.Add(thingData);
        }
    }
}
