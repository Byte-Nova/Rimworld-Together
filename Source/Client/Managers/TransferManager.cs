using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles all the thing transfers between clients in the mod

    public static class TransferManager
    {
        //Parses the packet into useful orders

        public static void ParseTransferPacket(Packet packet)
        {
            TransferData transferData = Serializer.ConvertBytesToObject<TransferData>(packet.contents);

            switch (transferData.stepMode)
            {
                case TransferStepMode.TradeRequest:
                    ReceiveTransferRequest(transferData);
                    break;

                case TransferStepMode.TradeAccept:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_OK("RTTransferSuccess".Translate()));
                    if (transferData.transferMode == TransferMode.Pod) LaunchDropPods();
                    FinishTransfer(true);
                    break;

                case TransferStepMode.TradeReject:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTTransferPlayerRejected".Translate()));
                    RecoverTradeItems(TransferLocation.Caravan);
                    break;

                case TransferStepMode.TradeReRequest:
                    DialogManager.PopWaitDialog();
                    ReceiveReboundRequest(transferData);
                    break;

                case TransferStepMode.TradeReAccept:
                    DialogManager.PopWaitDialog();
                    GetTransferedItemsToSettlement(TransferManagerHelper.GetAllTransferedItems(SessionValues.incomingManifest));
                    break;

                case TransferStepMode.TradeReReject:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTTransferPlayerRejected".Translate()));
                    RecoverTradeItems(TransferLocation.Settlement);
                    break;

                case TransferStepMode.Recover:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTPlayerNotAvailable".Translate()));
                    RecoverTradeItems(TransferLocation.Caravan);
                    break;
            }
        }

        //Takes transferable items from desired location

        public static void TakeTransferItems(TransferLocation transferLocation)
        {
            SessionValues.outgoingManifest.fromTile = Find.AnyPlayerHomeMap.Tile;

            if (transferLocation == TransferLocation.Caravan)
            {
                SessionValues.outgoingManifest.toTile = SessionValues.chosenSettlement.Tile;
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                SessionValues.outgoingManifest.toTile = SessionValues.incomingManifest.fromTile;
            }

            if (TradeSession.deal.TryExecute(out bool actuallyTraded))
            {
                SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();

                if (transferLocation == TransferLocation.Caravan)
                {
                    TradeSession.playerNegotiator.GetCaravan().RecacheImmobilizedNow();
                }
            }
        }

        //Takes transferable items from drop pods

        public static void TakeTransferItemsFromPods(CompLaunchable representative)
        {
            SessionValues.outgoingManifest.transferMode = TransferMode.Pod;
            SessionValues.outgoingManifest.fromTile = Find.AnyPlayerHomeMap.Tile;
            SessionValues.outgoingManifest.toTile = SessionValues.chosenSettlement.Tile;

            foreach (CompTransporter pod in representative.TransportersInGroup)
            {
                ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();

                for(int i = 0; i < directlyHeldThings.Count(); i++)
                {
                    TransferManagerHelper.AddThingToTransferManifest(directlyHeldThings[i], directlyHeldThings[i].stackCount);
                }
            }
        }

        //Sends a transfer request to the server

        public static void SendTransferRequestToServer(TransferLocation transferLocation)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("RTTransferResponseWait".Translate()));

            if (transferLocation == TransferLocation.Caravan)
            {
                SessionValues.outgoingManifest.stepMode = TransferStepMode.TradeRequest;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), SessionValues.outgoingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                SessionValues.outgoingManifest.stepMode = TransferStepMode.TradeReRequest;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), SessionValues.outgoingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferLocation == TransferLocation.Pod)
            {
                SessionValues.outgoingManifest.stepMode = TransferStepMode.TradeRequest;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), SessionValues.outgoingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferLocation == TransferLocation.World)
            {
                MarketData marketData = new MarketData();
                marketData.stepMode = MarketStepMode.Add;
                marketData.transferThings = SessionValues.outgoingManifest.itemDatas;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.MarketPacket), marketData);
                Network.listener.EnqueuePacket(packet);
            }
        }

        //Recovers transfered items when trade fails

        public static void RecoverTradeItems(TransferLocation transferLocation)
        {
            try
            {
                Thing[] toRecover = TransferManagerHelper.GetAllTransferedItems(SessionValues.outgoingManifest);

                if (transferLocation == TransferLocation.Caravan)
                {
                    GetTransferedItemsToCaravan(toRecover, false);
                }

                else if (transferLocation == TransferLocation.Settlement)
                {
                    GetTransferedItemsToSettlement(toRecover, false);
                }

                else if (transferLocation == TransferLocation.Pod)
                {
                    //Do nothing
                }
            }

            catch
            {
                Logger.Warning("Rethrowing transfer items, might be RimWorld's fault");

                Thread.Sleep(100);

                RecoverTradeItems(transferLocation);
            }
        }

        //Receives the transfered items into the settlement

        public static void GetTransferedItemsToSettlement(Thing[] things, bool success = true, bool customMap = true, bool invokeMessage = true)
        {
            Action r1 = delegate
            {
                Map map = null;
                if (customMap) map = Find.Maps.Find(x => x.Tile == SessionValues.incomingManifest.toTile);
                else map = Find.AnyPlayerHomeMap;

                foreach (Thing thing in things)
                {
                    if (thing.def.CanHaveFaction) thing.SetFactionDirect(Faction.OfPlayer);
                    RimworldManager.PlaceThingIntoMap(thing, map, ThingPlaceMode.Near, true);
                }

                FinishTransfer(success);
            };

            if (invokeMessage)
            {
                if (success) DialogManager.PushNewDialog(new RT_Dialog_OK("RTTransferSuccess".Translate(), r1));
                else DialogManager.PushNewDialog(new RT_Dialog_Error("RTTransferCancelled".Translate(), r1));
            }
            else r1.Invoke();
        }

        //Receives the transfered items into the caravan

        public static void GetTransferedItemsToCaravan(Thing[] things, bool success = true, bool invokeMessage = true)
        {
            Action r1 = delegate
            {
                foreach (Thing thing in things) RimworldManager.PlaceThingIntoCaravan(thing, SessionValues.chosenCaravan);

                FinishTransfer(success);
            };

            if (invokeMessage)
            {
                if (success) DialogManager.PushNewDialog(new RT_Dialog_OK("RTTransferSuccess".Translate(), r1));
                else DialogManager.PushNewDialog(new RT_Dialog_Error("RTTransferCancelled".Translate(), r1));
            }
            else r1.Invoke();
        }

        //Finishes the transfer order

        public static void FinishTransfer(bool success)
        {
            if (success) SaveManager.ForceSave();

            SessionValues.incomingManifest = new TransferData();
            SessionValues.outgoingManifest = new TransferData();
            ClientValues.ToggleTransfer(false);
        }

        //Executes when receiving a transfer request

        public static void ReceiveTransferRequest(TransferData transferData)
        {
            try
            {
                SessionValues.incomingManifest = transferData;

                if (!ClientValues.isReadyToPlay || ClientValues.isInTransfer || ClientValues.rejectTransferBool)
                {
                    RejectRequest(transferData.transferMode, false);
                }

                else
                {
                    Action r1 = delegate
                    {
                        if (transferData.transferMode == TransferMode.Gift)
                        {
                            RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Gift);
                            DialogManager.PushNewDialog(d1);
                        }

                        else if (transferData.transferMode == TransferMode.Trade)
                        {
                            RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Trade);
                            DialogManager.PushNewDialog(d1);
                        }

                        else if (transferData.transferMode == TransferMode.Pod)
                        {
                            RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Pod);
                            DialogManager.PushNewDialog(d1);
                        }
                    };

                    if (transferData.transferMode == TransferMode.Gift)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("RTTransferGiftReceived".Translate(), r1));
                    }

                    else if (transferData.transferMode == TransferMode.Trade)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("RTTransferTradeReceived".Translate(), r1));
                    }

                    else if (transferData.transferMode == TransferMode.Pod)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("RTTransferGiftReceived".Translate(), r1));
                    }
                }
            }

            catch
            {
                Logger.Warning("Rethrowing transfer items, might be RimWorld's fault");

                Thread.Sleep(100);

                ReceiveTransferRequest(transferData);
            }        
        }

        //Executes after receiving a rebound transfer request

        public static void ReceiveReboundRequest(TransferData transferData)
        {
            try
            {
                SessionValues.incomingManifest = transferData;

                RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Rebound);
                DialogManager.PushNewDialog(d1);
            }

            catch
            {
                Logger.Warning("Rethrowing transfer items, might be RimWorld's fault");

                Thread.Sleep(100);

                ReceiveReboundRequest(transferData);
            }
        }

        //Executes when rejecting a transfer request

        public static void RejectRequest(TransferMode transferMode, bool finishTransfer = true)
        {
            if (transferMode == TransferMode.Gift)
            {
                //Nothing should happen here
            }

            else if (transferMode == TransferMode.Trade)
            {
                SessionValues.incomingManifest.stepMode = TransferStepMode.TradeReject;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), SessionValues.incomingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferMode == TransferMode.Pod)
            {
                //Nothing should happen here
            }

            else if (transferMode == TransferMode.Rebound)
            {
                SessionValues.incomingManifest.stepMode = TransferStepMode.TradeReReject;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), SessionValues.incomingManifest);
                Network.listener.EnqueuePacket(packet);

                RecoverTradeItems(TransferLocation.Caravan);
            }

            if (finishTransfer) FinishTransfer(false);
        }

        //Launchs the drop pods with the desired transfer request

        public static void LaunchDropPods()
        {
            SessionValues.chosendPods.TryLaunch(SessionValues.chosenSettlement.Tile, 
                new TransportPodsArrivalAction_GiveGift(SessionValues.chosenSettlement));
        }
    }

    //Helper class of the TransferManager class

    public static class TransferManagerHelper
    {
        //Adds desired thing into transfer manifest

        public static void AddThingToTransferManifest(Thing thing, int thingCount)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing))
            {
                Pawn pawn = thing as Pawn;

                SessionValues.outgoingManifest.humanDatas.Add(HumanScribeManager.HumanToString(pawn, false));

                RimworldManager.RemovePawnFromGame(pawn);
            }

            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
            {
                Pawn pawn = thing as Pawn;

                SessionValues.outgoingManifest.animalDatas.Add(AnimalScribeManager.AnimalToString(pawn));

                RimworldManager.RemovePawnFromGame(pawn);
            }

            else SessionValues.outgoingManifest.itemDatas.Add(ThingScribeManager.ItemToString(thing, thingCount));
        }

        //Gets the transfer location in the desired map

        public static IntVec3 GetTransferLocationInMap(Map map)
        {
            Thing tradingSpot = map.listerThings.AllThings.Find(x => x.def.defName == "RTTransferSpot");
            if (tradingSpot != null) return tradingSpot.Position;
            else
            {
                RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "RTDialogNoTransferSpot1".Translate(),
                    "RTDialogNoTransferSpot2".Translate(),
                    "RTDialogNoTransferSpot3".Translate()});

                DialogManager.PushNewDialog(d1);

                return new IntVec3(map.Center.x, map.Center.y, map.Center.z);
            }
        }

        //Gets all the transfered items from the transfer into usable objects

        public static Thing[] GetAllTransferedItems(TransferData transferData)
        {
            List<Thing> allTransferedItems = new List<Thing>();

            foreach (Pawn pawn in HumanScribeManager.GetHumansFromString(transferData)) allTransferedItems.Add(pawn);

            foreach (Pawn animal in AnimalScribeManager.GetAnimalsFromString(transferData)) allTransferedItems.Add(animal);

            foreach (Thing thing in ThingScribeManager.GetItemsFromString(transferData)) allTransferedItems.Add(thing);

            return allTransferedItems.ToArray();
        }
    }
}
