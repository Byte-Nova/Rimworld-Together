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
            if (!Master.actionValues.EnableMarket)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            MarketData marketData = Serializer.ConvertBytesToObject<MarketData>(packet.contents);

            switch (marketData._stepMode)
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

        private static void AddToMarket(ServerClient client, MarketData marketData)
        {
            foreach (ThingData item in marketData._transferThings) TryCombineStackIfAvailable(client, item);

            Main_.SaveValueFile(ServerFileMode.Market);

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);

            marketData._stepMode = MarketStepMode.Reload;
            marketData._transferThings = Master.marketValues.MarketStock;

            packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), marketData);
            NetworkHelper.SendPacketToAllClients(packet, client);
        }

        private static void RemoveFromMarket(ServerClient client, MarketData marketData) 
        {
            if (marketData._quantityToManage == 0)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to buy illegal quantity at market");
                return;
            }

            ThingData toGet = Master.marketValues.MarketStock[marketData._indexToManage];
            int reservedQuantity = toGet.quantity;
            toGet.quantity = marketData._quantityToManage;
            marketData._transferThings = new List<ThingData>() { toGet };

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), marketData);

            toGet.quantity = reservedQuantity;
            if (toGet.quantity > marketData._quantityToManage) toGet.quantity -= marketData._quantityToManage;
            else if (toGet.quantity == marketData._quantityToManage) Master.marketValues.MarketStock.RemoveAt(marketData._indexToManage);
            else
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to buy illegal quantity at market");
                return;
            }

            client.listener.EnqueuePacket(packet);
            marketData._stepMode = MarketStepMode.Reload;
            marketData._transferThings = Master.marketValues.MarketStock;
            
            packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), marketData);
            NetworkHelper.SendPacketToAllClients(packet, client);

            Main_.SaveValueFile(ServerFileMode.Market);
        }

        private static void SendMarketStock(ServerClient client, MarketData marketData)
        {
            marketData._transferThings = Master.marketValues.MarketStock;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), marketData);
            client.listener.EnqueuePacket(packet);
        }

        private static void TryCombineStackIfAvailable(ServerClient client, ThingData thingData)
        {
            if (thingData.quantity <= 0)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to sell illegal quantity at market");
                return;
            }

            foreach (ThingData stockedItem in Master.marketValues.MarketStock.ToArray())
            {
                if (stockedItem.defName == thingData.defName && stockedItem.materialDefName == thingData.materialDefName)
                {
                    stockedItem.quantity += thingData.quantity;
                    return;
                }
            }

            Master.marketValues.MarketStock.Add(thingData);
        }
    }
}
