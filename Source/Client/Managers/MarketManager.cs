﻿using RimWorld;
using Shared;
using System;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that is in charge of the online market functions for the mod

    public static class MarketManager
    {
        //Parses received market packets into something usable

        public static void ParseMarketPacket(Packet packet)
        {
            MarketData marketData = Serializer.ConvertBytesToObject<MarketData>(packet.contents);

            switch (marketData._stepMode)
            {
                case MarketStepMode.Add:
                    ConfirmAddStock();
                    break;

                case MarketStepMode.Request:
                    ConfirmGetStock(marketData);
                    break;

                case MarketStepMode.Reload:
                    ConfirmReloadStock(marketData);
                    break;
            }
        }

        //Add to stock functions

        public static void RequestAddStock()
        {
            RT_Dialog_TransferMenu d1 = new RT_Dialog_TransferMenu(TransferLocation.Market, true, false, false, false);
            DialogManager.PushNewDialog(d1);
        }

        public static void ConfirmAddStock()
        {
            DialogManager.PopWaitDialog();
            DialogManager.dialogMarketListing = null;

            int silverToGet = 0;
            Thing[] sentItems = TransferManagerHelper.GetAllTransferedItems(SessionValues.outgoingManifest);
            foreach (Thing thing in sentItems) silverToGet += (int)(thing.stackCount * thing.MarketValue * 0.5f);

            if (silverToGet > 0)
            {
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = silverToGet;

                TransferManager.GetTransferedItemsToSettlement(new Thing[] { silver }, customMap: false);
            }

            else
            {
                TransferManager.FinishTransfer(true);
                DialogManager.PushNewDialog(new RT_Dialog_OK("Transfer was a success!"));
            }
        }

        //Get from stock functions

        public static void RequestGetStock(int marketIndex, int quantity)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for market response"));

            MarketData data = new MarketData();
            data._stepMode = MarketStepMode.Request;
            data._indexToManage = marketIndex;
            data._quantityToManage = quantity;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        public static void ConfirmGetStock(MarketData marketData)
        {
            DialogManager.PopWaitDialog();
            DialogManager.dialogMarketListing = null;

            Thing toReceive = ThingScribeManager.StringToItem(marketData._transferThings[0]);
            TransferManager.GetTransferedItemsToSettlement(new Thing[] { toReceive }, customMap: false);

            int silverToPay = (int)(toReceive.MarketValue * toReceive.stackCount);
            RimworldManager.RemoveThingFromSettlement(SessionValues.chosenSettlement.Map, ThingDefOf.Silver, silverToPay);

            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
        }

        //Reload stock functions

        public static void RequestReloadStock()
        {
            DialogManager.PushNewDialog(new RT_Dialog_MarketListing(new ThingDataFile[] { }, SessionValues.chosenSettlement.Map, null, null));
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for market response"));

            MarketData marketData = new MarketData();
            marketData._stepMode = MarketStepMode.Reload;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), marketData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void ConfirmReloadStock(MarketData marketData)
        {
            if (DialogManager.dialogMarketListing != null && !ClientValues.isInTransfer)
            {
                DialogManager.PopWaitDialog();

                Action toDo = delegate { RequestGetStock(DialogManager.dialogMarketListingResult, int.Parse(DialogManager.dialog1ResultOne)); };
                RT_Dialog_MarketListing dialog = new RT_Dialog_MarketListing(marketData._transferThings.ToArray(), SessionValues.chosenSettlement.Map, toDo, null);
                DialogManager.PushNewDialog(dialog);
            }
        }
    }
}
