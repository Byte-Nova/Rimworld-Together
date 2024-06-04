using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;
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
                    ReceiveMarketThing(marketData);
                    break;

                case MarketStepMode.Reload:
                    ReloadMarketStock(marketData);
                    break;
            }
        }

        //TODO
        //WORK ON ADDING ITEMS TO MARKET

        public static void AddStockToMarket()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for market response"));

            MarketData data = new MarketData();
            data.marketStepMode = MarketStepMode.Add;
            //data.stockToManage = null;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void RequestStockFromMarket(int marketIndex)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for market response"));

            MarketData data = new MarketData();
            data.marketStepMode = MarketStepMode.Request;
            data.indexToManage = marketIndex;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void ReceiveMarketThing(MarketData marketData)
        {
            DialogManager.PopWaitDialog();

            Thing toReceive = ThingScribeManager.StringToItem(marketData.stockToManage);

            TransferManager.GetTransferedItemsToSettlement(new Thing[] { toReceive }, customMap: false, invokeMessage: false);

            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
        }

        public static void RequestMarketReload()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for market response"));

            MarketData marketData = new MarketData();
            marketData.marketStepMode = MarketStepMode.Reload;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void ReloadMarketStock(MarketData marketData)
        {
            DialogManager.PopWaitDialog();

            Action toDo = delegate { RequestStockFromMarket(DialogManager.dialogMarketListingResult); };

            RT_Dialog_MarketListing dialog = new RT_Dialog_MarketListing(marketData.currentStock, toDo, null);

            DialogManager.PushNewDialog(dialog);
        }
    }
}
