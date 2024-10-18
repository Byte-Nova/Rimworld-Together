﻿using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    //Class that is in charge of the online market functions for the mod

    public static class MarketManager
    {
        //Variables

        private static readonly string marketFileName = "Market.json";

        //Parses received market packets into something usable

        public static void ParsePacket(ServerClient client, Packet packet)
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
            foreach (ThingDataFile item in marketData._transferThings) TryCombineStackIfAvailable(client, item);

            Main_.SaveValueFile(ServerFileMode.Market);

            Packet packet = Packet.CreatePacketFromObject(nameof(MarketManager), marketData);
            client.listener.EnqueuePacket(packet);

            marketData._stepMode = MarketStepMode.Reload;
            marketData._transferThings = Master.marketValues.MarketStock;

            packet = Packet.CreatePacketFromObject(nameof(MarketManager), marketData);
            NetworkHelper.SendPacketToAllClients(packet, client);
        }

        private static void RemoveFromMarket(ServerClient client, MarketData marketData)
        {
            if (marketData._quantityToManage == 0)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to buy illegal quantity at market");
                return;
            }

            ThingDataFile toGet = Master.marketValues.MarketStock[marketData._indexToManage];
            int reservedQuantity = toGet.Quantity;
            toGet.Quantity = marketData._quantityToManage;
            marketData._transferThings = new List<ThingDataFile>() { toGet };

            Packet packet = Packet.CreatePacketFromObject(nameof(MarketManager), marketData);

            toGet.Quantity = reservedQuantity;
            if (toGet.Quantity > marketData._quantityToManage) toGet.Quantity -= marketData._quantityToManage;
            else if (toGet.Quantity == marketData._quantityToManage) Master.marketValues.MarketStock.RemoveAt(marketData._indexToManage);
            else
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to buy illegal quantity at market");
                return;
            }

            client.listener.EnqueuePacket(packet);
            marketData._stepMode = MarketStepMode.Reload;
            marketData._transferThings = Master.marketValues.MarketStock;
            
            packet = Packet.CreatePacketFromObject(nameof(MarketManager), marketData);
            NetworkHelper.SendPacketToAllClients(packet, client);

            Main_.SaveValueFile(ServerFileMode.Market);
        }

        private static void SendMarketStock(ServerClient client, MarketData marketData)
        {
            marketData._transferThings = Master.marketValues.MarketStock;

            Packet packet = Packet.CreatePacketFromObject(nameof(MarketManager), marketData);
            client.listener.EnqueuePacket(packet);
        }

        private static void TryCombineStackIfAvailable(ServerClient client, ThingDataFile thingData)
        {
            if (thingData.Quantity <= 0) ResponseShortcutManager.SendIllegalPacket(client, "Tried to sell illegal quantity at market");
            else
            {
                //Check if thing is book to handle differently
                if (thingData.BookComponent.Title != "null") Master.marketValues.MarketStock.Add(thingData);

                //Check if thing is genepack to handle differently
                else if (thingData.GenepackComponent.GenepackDefs.Count != 0) Master.marketValues.MarketStock.Add(thingData);

                //Act as if normal thing
                else
                {
                    foreach (ThingDataFile stockedItem in MarketManagerHelper.GetMarketStockSafe())
                    {
                        if (stockedItem.DefName == thingData.DefName && stockedItem.MaterialDefName == thingData.MaterialDefName)
                        {
                            stockedItem.Quantity += thingData.Quantity;
                            return;
                        }
                    }

                    Master.marketValues.MarketStock.Add(thingData);
                }
            }
        }
    }

    public static class MarketManagerHelper
    {
        public static ThingDataFile[] GetMarketStockSafe()
        {
            return Master.marketValues.MarketStock.ToArray();
        }
    }
}
